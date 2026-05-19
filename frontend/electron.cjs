const { app, BrowserWindow } = require('electron')
const path = require('path')

const isDev = process.env.NODE_ENV !== 'production'

function createWindow() {
    const win = new BrowserWindow({
        width: 1920,
        height: 1080,
        minWidth: 1280,
        minHeight: 720,
        webPreferences: {
            nodeIntegration: false,
            contextIsolation: true,
        },
        titleBarStyle: 'default',
        title: 'Sistema de Horarios Académicos - UAM',
        show: false,
    })

    // En desarrollo carga desde Vite, en producción desde los archivos build
    if (isDev) {
        win.loadURL('http://localhost:5173')
        // win.webContents.openDevTools() // descomentar para debug
    } else {
        win.loadFile(path.join(__dirname, 'dist', 'index.html'))
    }

    // Mostrar ventana cuando esté lista para evitar flash blanco
    win.once('ready-to-show', () => {
        win.show()
        win.maximize()
    })
}

app.whenReady().then(() => {
    createWindow()

    app.on('activate', () => {
        if (BrowserWindow.getAllWindows().length === 0) createWindow()
    })
})

app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') app.quit()
})
