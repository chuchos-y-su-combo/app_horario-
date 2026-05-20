"use strict";
const electron = require("electron");
const child_process = require("child_process");
const path = require("path");
const http = require("http");
function _interopNamespaceDefault(e) {
  const n = Object.create(null, { [Symbol.toStringTag]: { value: "Module" } });
  if (e) {
    for (const k in e) {
      if (k !== "default") {
        const d = Object.getOwnPropertyDescriptor(e, k);
        Object.defineProperty(n, k, d.get ? d : {
          enumerable: true,
          get: () => e[k]
        });
      }
    }
  }
  n.default = e;
  return Object.freeze(n);
}
const path__namespace = /* @__PURE__ */ _interopNamespaceDefault(path);
const http__namespace = /* @__PURE__ */ _interopNamespaceDefault(http);
const isDev = process.env.NODE_ENV !== "production";
let backendProcess = null;
let mainWindow = null;
let appIsQuitting = false;
function getBackendDir() {
  if (isDev) {
    return path__namespace.join(__dirname, "..", "..", "publish-test");
  }
  return path__namespace.join(process.resourcesPath, "backend");
}
function spawnBackend() {
  var _a, _b;
  const backendDir = getBackendDir();
  const exePath = path__namespace.join(backendDir, "ApplicationSchedule.Api.exe");
  const dbPath = path__namespace.join(electron.app.getPath("userData"), "horarios.db");
  console.log("[Electron] Iniciando backend:", exePath);
  console.log("[Electron] Base de datos:", dbPath);
  backendProcess = child_process.spawn(exePath, [], {
    env: {
      ...process.env,
      ASPNETCORE_URLS: "http://localhost:5213",
      ASPNETCORE_ENVIRONMENT: "Production",
      // Overrides appsettings.json — keeps DB in user's AppData (writable)
      ConnectionStrings__DefaultConnection: `Data Source=${dbPath}`
    },
    windowsHide: true
    // No console window visible to user
  });
  (_a = backendProcess.stdout) == null ? void 0 : _a.on("data", (d) => console.log("[Backend]", d.toString().trim()));
  (_b = backendProcess.stderr) == null ? void 0 : _b.on("data", (d) => console.error("[Backend ERR]", d.toString().trim()));
  backendProcess.on("exit", (code) => {
    console.log("[Backend] Proceso terminado, código:", code);
    if (!appIsQuitting && code !== 0 && code !== null) {
      electron.dialog.showErrorBox(
        "El servidor interno falló",
        `El servidor interno se detuvo inesperadamente (código ${code}).
Revisá que el puerto 5213 no esté ocupado y reiniciá la aplicación.`
      );
      electron.app.quit();
    }
  });
}
function killBackend() {
  if (backendProcess) {
    backendProcess.kill();
    backendProcess = null;
  }
}
function waitForBackend(maxMs = 3e4) {
  return new Promise((resolve, reject) => {
    const started = Date.now();
    const check = () => {
      const req = http__namespace.get(
        "http://localhost:5213/api/asignaciones/periodos-historicos",
        (res) => {
          res.resume();
          resolve();
        }
      );
      req.setTimeout(2e3);
      req.on("error", () => {
        if (Date.now() - started >= maxMs) {
          reject(new Error("El servidor interno no respondió en 30 segundos."));
        } else {
          setTimeout(check, 600);
        }
      });
      req.end();
    };
    setTimeout(check, 1500);
  });
}
async function createWindow() {
  mainWindow = new electron.BrowserWindow({
    width: 1920,
    height: 1080,
    minWidth: 1280,
    minHeight: 720,
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true
    },
    title: "Sistema de Horarios Académicos - UAM",
    show: false,
    backgroundColor: "#1A1A2E"
  });
  mainWindow.loadURL(
    'data:text/html,<html><body style="background:#1A1A2E;display:flex;flex-direction:column;align-items:center;justify-content:center;height:100vh;margin:0;gap:12px"><p style="color:#fff;font-family:sans-serif;font-size:20px;margin:0">Sistema de Horarios Académicos</p><p style="color:#888;font-family:sans-serif;font-size:14px;margin:0">Iniciando servidor interno…</p></body></html>'
  );
  mainWindow.once("ready-to-show", () => {
    mainWindow == null ? void 0 : mainWindow.show();
    mainWindow == null ? void 0 : mainWindow.maximize();
  });
  spawnBackend();
  try {
    await waitForBackend();
  } catch (err) {
    electron.dialog.showErrorBox(
      "No se pudo iniciar el servidor",
      String(err) + "\n\nVerificá que el puerto 5213 esté libre y volvé a abrir la aplicación."
    );
    killBackend();
    electron.app.quit();
    return;
  }
  if (isDev) {
    await mainWindow.loadURL("http://localhost:5173");
  } else {
    await mainWindow.loadFile(path__namespace.join(__dirname, "dist", "index.html"));
  }
}
electron.app.whenReady().then(createWindow);
electron.app.on("before-quit", () => {
  appIsQuitting = true;
  killBackend();
});
electron.app.on("window-all-closed", () => {
  killBackend();
  if (process.platform !== "darwin") electron.app.quit();
});
electron.app.on("activate", () => {
  if (electron.BrowserWindow.getAllWindows().length === 0) createWindow();
});
