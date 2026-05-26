'use strict';

const { contextBridge } = require('electron');

/**
 * Expose a minimal API surface to the renderer world.
 * The renderer reads `window.electronAPI.apiPort` to build the axios base URL.
 */
contextBridge.exposeInMainWorld('electronAPI', {
  /** Port on which the .NET backend listens inside the packaged app. */
  apiPort: 5000,
  /** Flag that lets the renderer detect it is running inside Electron. */
  isElectron: true,
});
