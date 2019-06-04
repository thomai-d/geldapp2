export function isOfflineException(ex: any): boolean {
    return ex.status === 0 || ex.status > 500;
}
