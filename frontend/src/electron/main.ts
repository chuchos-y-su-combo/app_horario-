import { app, BrowserWindow } from "electron";
import path from "path";

app.setPath("userData", path.join(app.getPath("temp"), "my-electron-app"));

function createWindow() {
    const win = new BrowserWindow({
    width: 1200,
    height: 800
    });

    win.loadURL("http://localhost:5173");
}

app.whenReady().then(createWindow);