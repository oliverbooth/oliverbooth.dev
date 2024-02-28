import "../Prototypes";
import {KeyboardModifier, KeyboardShortcut} from "./KeyboardShortcut";
import MarkdownFormatting from "./MarkdownFormatting";
import UI from "./UI";

/**
 * Represents a class which implements the Markdown editor and its controls.
 */
class MarkdownEditor {
    private readonly _element: HTMLTextAreaElement;
    private _nextSelection?: number[];
    private _saveListeners: Function[] = [];

    /**
     * Initializes a new instance of the {@link MarkdownEditor} class.
     * @param element The editor's {@link HTMLTextAreaElement}.
     */
    constructor(element: HTMLTextAreaElement) {
        this._element = element;
    }

    /**
     * Adds a callback that will be invoked when the save shortcut is executed.
     * @param callback The callback to invoke.
     */
    public addSaveListener(callback: Function) {
        if (callback) {
            this._saveListeners.push(callback);
        }
    }

    /**
     * Applies Markdown formatting to the current selection.
     * @param formatting The formatting to apply.
     */
    public formatSelection(formatting: MarkdownFormatting): void {
        if (!formatting) {
            return;
        }

        const el: HTMLTextAreaElement = this._element;
        const selectionStart: number = el.selectionStart;
        const selectionEnd: number = el.selectionEnd;

        const formatString = formatting.toString();
        const len = formatString.length;

        el.insertAt(formatString, selectionStart);
        el.insertAt(formatString, selectionEnd + len);

        el.selectionStart = selectionStart + len;
        el.selectionEnd = selectionEnd + len;

        UI.redraw();
    }

    /**
     * Inserts a Markdown hyperlink at the current caret position.
     */
    public insertLink(): void {
        const el: HTMLTextAreaElement = this._element;
        const selectionStart: number = el.selectionStart;
        const selectionEnd: number = el.selectionEnd;

        if (selectionStart === selectionEnd) {
            el.insertAt("[<text>](<url>)", selectionStart);
            el.selectionStart = selectionStart + 1;
            el.selectionEnd = selectionStart + 7;

            this._nextSelection = [2, 5];
        } else {
            el.insertAt("[", selectionStart);
            el.insertAt("](<url>)", selectionEnd + 1);
            el.selectionStart = selectionEnd + 3;
            el.selectionEnd = el.selectionStart + 5;
        }

        UI.redraw();
    }

    /**
     * Pastes the current clipboard content into the editor.
     */
    public async pasteClipboard(event: ClipboardEvent): Promise<void> {
        const files: FileList = event.clipboardData.files;
        if (files.length) {
            const file: File = files[0];
            const data: ArrayBuffer = await file.arrayBuffer();
            console.log(data);
        }

        console.log(files);
    }

    /**
     * Registers all default keyboard shortcuts for the editor.
     */
    public registerDefaultShortcuts() {
        this.registerShortcut(new KeyboardShortcut('b', KeyboardModifier.CTRL), () => this.formatSelection(MarkdownFormatting.BOLD));
        this.registerShortcut(new KeyboardShortcut('i', KeyboardModifier.CTRL), () => this.formatSelection(MarkdownFormatting.ITALIC));
        this.registerShortcut(new KeyboardShortcut('k', KeyboardModifier.CTRL), () => this.insertLink());
        this.registerShortcut(new KeyboardShortcut('s', KeyboardModifier.CTRL), () => this.save());

        this._element.addEventListener("paste", (ev: ClipboardEvent) => this.pasteClipboard(ev));
    }

    /**
     * Registers a new keyboard shortcut.
     * @param shortcut The keyboard shortcut for which to listen.
     * @param action The action to invoke when the shortcut is executed.
     */
    public registerShortcut(shortcut: KeyboardShortcut, action: Function): void {
        if (!shortcut) throw new Error("No shortcut provided");
        if (!action) throw new Error("No callback function provided");

        this._element.addEventListener("keydown", (ev: KeyboardEvent) => {
            if ((shortcut.modifier & KeyboardModifier.CTRL) && !ev.ctrlKey) {
                return;
            }

            if ((shortcut.modifier & KeyboardModifier.SHIFT) && !ev.shiftKey) {
                return;
            }

            if ((shortcut.modifier & KeyboardModifier.ALT) && !ev.altKey) {
                return;
            }

            if (ev.key == shortcut.key) {
                ev.preventDefault();
                action();
            }
        });

        console.log("Bound shortcut", shortcut, "to", action);
    }

    public registerEvents() {
        this.handleQuoteNewLine();
        this.handleTab();
    }

    /**
     * Saves the content in the editor.
     */
    public save() {
        this._saveListeners.forEach(fn => fn());
        UI.redraw();
    }

    private handleTab() {
        const el = this._element;
        el.addEventListener("keydown", (ev: KeyboardEvent) => {
            if (ev.key !== "Tab") {
                return;
            }

            ev.preventDefault();

            if (this._nextSelection) {
                el.selectionStart += this._nextSelection[0];
                el.selectionEnd = el.selectionEnd + this._nextSelection[1];
                return;
            }

            let text = el.value;
            let selStart = el.selectionStart;
            let selEnd = el.selectionEnd;

            // selection?
            if (selStart === selEnd) {
                // These single character operations are undoable
                if (ev.shiftKey) {
                    text = el.value;
                    if (selStart > 0 && (text[selStart - 1] === '\t' || text.slice(selStart - 4, selStart) === "    ")) {
                        document.execCommand("delete");
                    }
                } else {
                    document.execCommand("insertText", false, "    ");
                }
            } else {
                // Block indent/unindent trashes undo stack.
                // Select whole lines
                text = el.value;
                while (selStart > 0 && text[selStart - 1] !== '\n')
                    selStart--;
                while (selEnd > 0 && text[selEnd - 1] !== '\n' && selEnd < text.length)
                    selEnd++;

                // Get selected text
                let lines = text.slice(selStart, selEnd - selStart).split('\n');

                // Insert tabs
                for (let i = 0; i < lines.length; i++) {
                    // Don't indent last line if cursor at start of line
                    if (i == lines.length - 1 && lines[i].length == 0)
                        continue;

                    // Tab or Shift+Tab?
                    if (ev.shiftKey) {
                        if (lines[i].startsWith('\t'))
                            lines[i] = lines[i].slice(1);
                        else if (lines[i].startsWith("    "))
                            lines[i] = lines[i].slice(4);
                    } else {
                        lines[i] = `    ${lines[i]}`;
                    }
                }

                const result = lines.join('\n');

                // Update the text area
                el.value = text.slice(0, selStart) + result + text.slice(selEnd);
                el.selectionStart = selStart;
                el.selectionEnd = selStart + lines.length;

                UI.redraw();
            }
        });
    }

    private handleQuoteNewLine() {
        const el = this._element;
        el.addEventListener("keydown", (ev: KeyboardEvent) => {
            if (ev.key !== "Enter") {
                return;
            }

            const selectionStart = el.selectionStart;
            const selectionEnd = el.selectionEnd;

            let lineStart = selectionStart;
            while (lineStart > 0 && el.value[lineStart - 1] !== '\n') {
                lineStart--;
            }

            const lineEnd = el.value.indexOf('\n', lineStart);
            const line = el.value.slice(lineStart, lineEnd);

            if (line.startsWith("> ")) {
                ev.preventDefault();

                el.insertAt("\n> ", lineEnd);
                el.blur();
                el.focus();
                el.selectionEnd = selectionEnd + 3;

                UI.redraw();
            }
        });
    }
}

export default MarkdownEditor;