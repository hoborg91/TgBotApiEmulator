export type Req<T> = T extends object ? {
    [P in keyof T]-?: Req<T[P]>;
} : T;

export function check<T extends object>(obj: T): Req<T> {
    function validate(current: any, path: string[] = []): void {
        const currentPath = path.join('.') || 'root';
        
        // Check for null or undefined
        if (//current === null ||
            current === undefined) {
            throw new Error(`Required value is missing at path: ${currentPath}`);
        }

        // Handle objects (but not arrays, dates, etc.)
        if (typeof current === 'object' && !Array.isArray(current) && !(current instanceof Date) && current !== null) {
            const keys = Object.keys(current);
            
            if (keys.length === 0) {
                throw new Error(`Empty object at path: ${currentPath}`);
            }
            
            for (const key of keys) {
                validate(current[key], [...path, key]);
            }
        }
        
        // Handle arrays
        if (Array.isArray(current)) {
            if (current.length === 0) {
                throw new Error(`Empty array at path: ${currentPath}`);
            }
            
            current.forEach((item, index) => {
                validate(item, [...path, `[${index}]`]);
            });
        }
    }

    validate(obj);
    return obj as Req<T>;
}
