export { }

declare global {
    interface Window {
        chrome: {
            webview: {
                postMessage: (message: any) => void,
                addEventListener(name: string, callback: (event: any) => void),
                hostObjects: {
                    host
                }
            }
        }
    }
}