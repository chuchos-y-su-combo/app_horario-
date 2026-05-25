'use strict';

const { app, BrowserWindow, dialog } = require('electron');
const path = require('path');
const { spawn } = require('child_process');
const net = require('net');
const fs = require('fs');

// ── Fijar nombre y userData ANTES del evento 'ready' ─────────────────────────
// Sin esto, Electron hereda el nombre del package.json (que puede ser el nombre
// del scope de pnpm, p. ej. "@figma/my-make-file"), apuntando userData a una
// ruta incorrecta como %AppData%\@figma\my-make-file.
app.setName('AppHorario');
app.setPath('userData', path.join(app.getPath('appData'), 'AppHorario'));
// ─────────────────────────────────────────────────────────────────────────────

const API_PORT = 5000;
let backendProcess = null;
let logStream = null;

// ---------------------------------------------------------------------------
// File logger (writes to %AppData%\AppHorario\electron-backend.log)
// ---------------------------------------------------------------------------

function initLogger() {
  try {
    // Garantizar que el directorio exista antes de abrir el stream
    fs.mkdirSync(app.getPath('userData'), { recursive: true });
    const logPath = path.join(app.getPath('userData'), 'electron-backend.log');
    logStream = fs.createWriteStream(logPath, { flags: 'a' });
    log(`=== AppHorario started at ${new Date().toISOString()} ===`);
    log(`[Electron] userData: ${app.getPath('userData')}`);
    log(`[Electron] resourcesPath: ${process.resourcesPath}`);
    log(`[Electron] isPackaged: ${app.isPackaged}`);
  } catch (err) {
    console.error('[Logger] Could not open log file:', err);
  }
}

function log(msg) {
  const line = `[${new Date().toISOString()}] ${msg}`;
  console.log(line);
  try { logStream?.write(line + '\n'); } catch (_) {}
}

function logErr(msg) {
  const line = `[${new Date().toISOString()}] ERROR: ${msg}`;
  console.error(line);
  try { logStream?.write(line + '\n'); } catch (_) {}
}

// ---------------------------------------------------------------------------
// Paths
// ---------------------------------------------------------------------------

/** Path to the self-contained backend executable inside the packaged app. */
function getBackendExePath() {
  return path.join(process.resourcesPath, 'backend', 'ApplicationSchedule.Api.exe');
}

/**
 * SQLite DB stored in the user-writable AppData folder.
 * This persists between app runs and upgrades.
 */
function getDbPath() {
  return path.join(app.getPath('userData'), 'horarios.db');
}

// ---------------------------------------------------------------------------
// Backend lifecycle
// ---------------------------------------------------------------------------

function startBackend() {
  // In development the user runs the .NET backend manually with `dotnet run`.
  if (!app.isPackaged) {
    log('[Electron] Dev mode – backend must be started manually.');
    return;
  }

  const exePath = getBackendExePath();
  const dbPath  = getDbPath();
  const exeDir  = path.dirname(exePath);

  // ── Crear directorio de datos ANTES de cualquier check ──────────────────
  fs.mkdirSync(app.getPath('userData'), { recursive: true });

  log(`[Electron] Backend exe path: ${exePath}`);
  log(`[Electron] Backend exe dir:  ${exeDir}`);
  log(`[Electron] DB path:          ${dbPath}`);

  // ── Diagnóstico siempre visible: muestra la ruta donde busca el exe ─────
  dialog.showMessageBoxSync({
    type: 'info',
    title: 'AppHorario – Diagnóstico de ruta',
    message: 'Ruta donde se busca el backend:',
    detail:
      `resourcesPath : ${process.resourcesPath}\n` +
      `exePath       : ${exePath}\n` +
      `existe        : ${fs.existsSync(exePath) ? 'SÍ ✓' : 'NO ✗'}\n` +
      `userData      : ${app.getPath('userData')}`,
    buttons: ['OK'],
  });

  // ── Pre-spawn check ──────────────────────────────────────────────────────
  if (!fs.existsSync(exePath)) {
    const msg = `No se encontró el ejecutable del backend:\n${exePath}\n\nReinstala la aplicación.`;
    logErr(msg);
    dialog.showErrorBox('AppHorario – Error de inicio', msg);
    return;
  }

  log('[Electron] Backend exe found. Spawning…');

  backendProcess = spawn(exePath, [], {
    cwd: exeDir,                      // ← critical: ASP.NET resolves appsettings.json relative to cwd
    stdio: ['ignore', 'pipe', 'pipe'],
    detached: false,
    windowsHide: true,
    env: {
      ...process.env,
      // ASP.NET Core reads ASPNETCORE_URLS automatically
      ASPNETCORE_URLS: `http://localhost:${API_PORT}`,
      // Custom env var read in Program.cs to override the connection string
      APP_CONNECTION_STRING: `Data Source=${dbPath}`,
      ASPNETCORE_ENVIRONMENT: 'Production',
      // Control where the .NET single-file bundle extracts native libs
      DOTNET_BUNDLE_EXTRACT_BASE_DIR: path.join(app.getPath('userData'), 'dotnet-extract'),
    },
  });

  backendProcess.stdout.on('data', (d) => log(`[Backend] ${d.toString().trimEnd()}`));
  backendProcess.stderr.on('data', (d) => logErr(`[Backend] ${d.toString().trimEnd()}`));

  backendProcess.on('error', (err) => {
    logErr(`[Backend] Failed to start: ${err.message}`);
    dialog.showErrorBox(
      'AppHorario – Error de inicio',
      `El backend no pudo iniciarse:\n${err.message}\n\nRevisa el log en:\n${path.join(app.getPath('userData'), 'electron-backend.log')}`
    );
  });

  backendProcess.on('exit', (code, signal) => {
    log(`[Backend] Exited with code=${code} signal=${signal}`);
    if (code !== 0 && code !== null) {
      dialog.showErrorBox(
        'AppHorario – Backend cerrado inesperadamente',
        `El backend terminó con código ${code}.\n\nRevisa el log en:\n${path.join(app.getPath('userData'), 'electron-backend.log')}`
      );
    }
  });

  log('[Electron] Backend process spawned successfully.');
}

function killBackend() {
  if (backendProcess && !backendProcess.killed) {
    log('[Electron] Killing backend process…');
    try { backendProcess.kill(); } catch (_) {}
    backendProcess = null;
  }
}

// ---------------------------------------------------------------------------
// Wait for backend readiness (TCP port check)
// ---------------------------------------------------------------------------

function waitForBackend(onReady) {
  const MAX_WAIT_MS = 30_000;
  const POLL_MS = 1_000;
  let elapsed = 0;

  const check = () => {
    const socket = new net.Socket();
    socket.setTimeout(800);
    socket
      .on('connect', () => {
        socket.destroy();
        log(`[Electron] Backend ready on port ${API_PORT}.`);
        onReady();
      })
      .on('error', retry)
      .on('timeout', retry)
      .connect(API_PORT, '127.0.0.1');
  };

  const retry = () => {
    elapsed += POLL_MS;
    if (elapsed < MAX_WAIT_MS) {
      setTimeout(check, POLL_MS);
    } else {
      log('[Electron] Backend did not start in time; opening window anyway.');
      onReady();
    }
  };

  // Give the process a moment to begin initializing before first poll.
  setTimeout(check, 1_500);
}

// ---------------------------------------------------------------------------
// BrowserWindow
// ---------------------------------------------------------------------------

function createWindow() {
  // Icon path: inside ASAR (packaged) or local filesystem (dev).
  // electron-builder burns the same icon into AppHorario.exe, so Windows
  // taskbar / alt-tab / shortcuts automatically use it.  Setting it here
  // also applies it to the window chrome on Linux and as a fallback on Windows.
  const iconPath = path.join(__dirname, '../resources/icon.ico');

  const win = new BrowserWindow({
    width: 1400,
    height: 900,
    minWidth: 1024,
    minHeight: 700,
    title: 'AppHorario',
    icon: iconPath,
    show: false,
    webPreferences: {
      preload: path.join(__dirname, 'preload.cjs'),
      contextIsolation: true,
      nodeIntegration: false,
    },
  });

  // Reveal only after the page has rendered to avoid a blank flash.
  win.once('ready-to-show', () => win.show());

  if (app.isPackaged) {
    // Production: load the Vite build via file://
    win.loadFile(path.join(__dirname, '../dist/index.html'));
  } else {
    // Development: load the Vite dev server
    win.loadURL('http://localhost:5173');
    win.webContents.openDevTools({ mode: 'detach' });
  }

  return win;
}

// ---------------------------------------------------------------------------
// App events
// ---------------------------------------------------------------------------

app.whenReady().then(() => {
  initLogger();
  startBackend();

  if (app.isPackaged) {
    // Wait until the backend is accepting connections before showing the UI.
    waitForBackend(() => createWindow());
  } else {
    createWindow();
  }
});

// Kill backend when all windows close (Windows / Linux behaviour).
app.on('window-all-closed', () => {
  killBackend();
  app.quit();
});

// Kill backend when the app is about to quit (macOS safe-quit, etc.).
app.on('before-quit', () => killBackend());
