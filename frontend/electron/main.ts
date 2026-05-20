import { app, BrowserWindow, dialog } from 'electron'
import { spawn, ChildProcess } from 'child_process'
import * as path from 'path'
import * as http from 'http'

const isDev = process.env.NODE_ENV !== 'production'

let backendProcess: ChildProcess | null = null
let mainWindow: BrowserWindow | null = null
let appIsQuitting = false

// ──────────────────────────────────────────────
// Backend path
// ──────────────────────────────────────────────
function getBackendDir(): string {
    if (isDev) {
        // Dev: use the already-published self-contained build at project root
        return path.join(__dirname, '..', '..', 'publish-test')
    }
    // Production: electron-builder places extraResources in process.resourcesPath
    return path.join(process.resourcesPath, 'backend')
}

// ──────────────────────────────────────────────
// Spawn the ASP.NET backend
// ──────────────────────────────────────────────
function spawnBackend(): void {
    const backendDir = getBackendDir()
    const exePath = path.join(backendDir, 'ApplicationSchedule.Api.exe')
    const dbPath = path.join(app.getPath('userData'), 'horarios.db')

    console.log('[Electron] Iniciando backend:', exePath)
    console.log('[Electron] Base de datos:', dbPath)

    backendProcess = spawn(exePath, [], {
        env: {
            ...process.env,
            ASPNETCORE_URLS: 'http://localhost:5213',
            ASPNETCORE_ENVIRONMENT: 'Production',
            // Overrides appsettings.json — keeps DB in user's AppData (writable)
            ConnectionStrings__DefaultConnection: `Data Source=${dbPath}`,
        },
        windowsHide: true, // No console window visible to user
    })

    backendProcess.stdout?.on('data', (d: Buffer) =>
        console.log('[Backend]', d.toString().trim()))
    backendProcess.stderr?.on('data', (d: Buffer) =>
        console.error('[Backend ERR]', d.toString().trim()))
    backendProcess.on('exit', (code: number | null) => {
        console.log('[Backend] Proceso terminado, código:', code)
        if (!appIsQuitting && code !== 0 && code !== null) {
            dialog.showErrorBox(
                'El servidor interno falló',
                `El servidor interno se detuvo inesperadamente (código ${code}).\nRevisá que el puerto 5213 no esté ocupado y reiniciá la aplicación.`
            )
            app.quit()
        }
    })
}

function killBackend(): void {
    if (backendProcess) {
        backendProcess.kill()
        backendProcess = null
    }
}

// ──────────────────────────────────────────────
// Poll until the backend HTTP server responds
// ──────────────────────────────────────────────
function waitForBackend(maxMs = 30_000): Promise<void> {
    return new Promise((resolve, reject) => {
        const started = Date.now()

        const check = (): void => {
            const req = http.get(
                'http://localhost:5213/api/asignaciones/periodos-historicos',
                (res) => {
                    res.resume() // drain response body
                    resolve()
                }
            )
            req.setTimeout(2000)
            req.on('error', () => {
                if (Date.now() - started >= maxMs) {
                    reject(new Error('El servidor interno no respondió en 30 segundos.'))
                } else {
                    setTimeout(check, 600)
                }
            })
            req.end()
        }

        // Give the process 1.5 s to start before the first check
        setTimeout(check, 1500)
    })
}

// ──────────────────────────────────────────────
// Main window
// ──────────────────────────────────────────────
async function createWindow(): Promise<void> {
    mainWindow = new BrowserWindow({
        width: 1920,
        height: 1080,
        minWidth: 1280,
        minHeight: 720,
        webPreferences: {
            nodeIntegration: false,
            contextIsolation: true,
        },
        title: 'Sistema de Horarios Académicos - UAM',
        show: false,
        backgroundColor: '#1A1A2E',
    })

    // Show a loading splash immediately (before backend is ready)
    mainWindow.loadURL(
        'data:text/html,<html><body style="background:#1A1A2E;display:flex;flex-direction:column;align-items:center;justify-content:center;height:100vh;margin:0;gap:12px"><p style="color:#fff;font-family:sans-serif;font-size:20px;margin:0">Sistema de Horarios Académicos</p><p style="color:#888;font-family:sans-serif;font-size:14px;margin:0">Iniciando servidor interno…</p></body></html>'
    )

    mainWindow.once('ready-to-show', () => {
        mainWindow?.show()
        mainWindow?.maximize()
    })

    spawnBackend()

    try {
        await waitForBackend()
    } catch (err) {
        dialog.showErrorBox(
            'No se pudo iniciar el servidor',
            String(err) + '\n\nVerificá que el puerto 5213 esté libre y volvé a abrir la aplicación.'
        )
        killBackend()
        app.quit()
        return
    }

    if (isDev) {
        await mainWindow.loadURL('http://localhost:5173')
    } else {
        await mainWindow.loadFile(path.join(__dirname, 'dist', 'index.html'))
    }
}

// ──────────────────────────────────────────────
// App lifecycle
// ──────────────────────────────────────────────
app.whenReady().then(createWindow)

app.on('before-quit', () => {
    appIsQuitting = true
    killBackend()
})

app.on('window-all-closed', () => {
    killBackend()
    if (process.platform !== 'darwin') app.quit()
})

app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) createWindow()
})
