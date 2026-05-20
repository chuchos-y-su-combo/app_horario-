import { defineConfig } from 'vite'

// Compiles the Electron main process (electron/main.ts) → electron.cjs
// Run via: vite build --config electron.vite.config.ts
export default defineConfig({
    build: {
        lib: {
            entry: 'electron/main.ts',
            formats: ['cjs'],
            // Return full filename so Vite doesn't strip the extension
            fileName: () => 'electron.cjs',
        },
        outDir: '.',
        rollupOptions: {
            // All node builtins and electron stay external – not bundled
            external: [
                'electron',
                'child_process',
                'path',
                'http',
                'https',
                'url',
                'fs',
                'os',
                'net',
                'stream',
                'events',
                'util',
            ],
        },
        emptyOutDir: false, // Never wipe the project root
        minify: false,      // Keep readable for debugging
        sourcemap: false,
    },
})
