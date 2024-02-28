/**
 * Represents a keyboard modifier.
 */
export enum KeyboardModifier {
    CTRL = 1,
    SHIFT = 2,
    ALT = 4
}

/**
 * Represents a keyboard shortcut.
 */
export class KeyboardShortcut {
    private readonly _key: string;
    private readonly _modifier: KeyboardModifier;

    /**
     * Initializes a new instance of the {@link KeyboardShortcut} class.
     * @param key The key.
     * @param modifier The modifier, if any.
     */
    constructor(key: string, modifier?: KeyboardModifier) {
        this._key = key;
        this._modifier = modifier;
    }

    /**
     * Gets the key for this keyboard shortcut.
     */
    public get key(): string {
        return this._key;
    }

    /**
     * Gets the modifier for this keyboard shortcut.
     */
    public get modifier(): KeyboardModifier {
        return this._modifier;
    }
}

export default {KeyboardShortcut, KeyboardModifier};