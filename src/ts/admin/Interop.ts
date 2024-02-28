declare interface DotNetHelper {
    invokeMethodAsync: (methodName: string, ...args: any) => Promise<any>;
}

class Interop {
    private static _dotNetHelper: DotNetHelper;
    
    public static invoke<T>(methodName: string, ...args: any): Promise<T> {
        return Interop.dotNetHelper.invokeMethodAsync(methodName, ...args);
    }

    public static get dotNetHelper() {
        return this._dotNetHelper;
    }

    static setDotNetHelper(value: DotNetHelper) {
        this._dotNetHelper = value;
    }
}

// @ts-ignore
window.Interop = Interop;
export default Interop;