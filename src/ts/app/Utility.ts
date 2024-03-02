class Utility {
    public static delay(timeout: number): Promise<void> {
        return new Promise(resolve => setTimeout(resolve, timeout));
    }
}

export default Utility;