import AdminUI from "./AdminUI";
import adminUI from "./AdminUI";

(() => {
    const textareas = document.querySelectorAll("textarea.tab-support");
    textareas.forEach((textarea: HTMLTextAreaElement) => {
        textarea.addEventListener("keydown", (e: KeyboardEvent) => {
            let text: string;

            // Enter Key?
            if (e.key === "Enter") {
                const selStart = textarea.selectionStart;
                const selEnd = textarea.selectionEnd;
                let sel = selStart;
                // selection?
                if (sel == selEnd) {
                    // find start of the current line
                    let text = textarea.value;
                    while (sel > 0 && text[sel - 1] !== '\n')
                        sel--;

                    console.log(`Line starts at index ${sel}`);

                    const lineStart = sel;
                    while (text[sel] === ' ' || text[sel] === '\t')
                        sel++;

                    console.log(`Identation ends at ${sel} (sel + ${sel - lineStart})`);

                    if (sel > lineStart) {
                        const lineEnd = lineStart + text.indexOf('\n', lineStart);
                        console.log(`Line starts at index ${lineEnd}`);
                        e.preventDefault();

                        const indentStr = text.slice(lineStart, sel);
                        console.log(`Indent string is "${indentStr}"`);

                        // insert carriage return and indented text
                        textarea.value = `${text.slice(0, selStart)}\n${indentStr}${text.slice(selStart)}`;

                        // Scroll caret visible
                        textarea.blur();
                        textarea.focus();
                        textarea.selectionEnd = selEnd + indentStr.length + 1; // +1 for \n
                        AdminUI.updateEditView();
                        return false;
                    }
                }
            }

            // Tab key?
            if (e.key === "Tab") {
                e.preventDefault();
                let selStart = textarea.selectionStart;

                // selection?
                if (selStart == textarea.selectionEnd) {
                    // These single character operations are undoable
                    if (e.shiftKey) {
                        text = textarea.value;
                        if (selStart > 0 && (text[selStart - 1] === '\t' || text.slice(selStart - 4, selStart) === "    ")) {
                            document.execCommand("delete");
                        }
                    } else {
                        document.execCommand("insertText", false, "    ");
                    }
                } else {
                    // Block indent/unindent trashes undo stack.
                    // Select whole lines
                    let selEnd = textarea.selectionEnd;
                    text = textarea.value;
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
                        if (e.shiftKey) {
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
                    textarea.value = text.slice(0, selStart) + result + text.slice(selEnd);
                    textarea.selectionStart = selStart;
                    textarea.selectionEnd = selStart + lines.length;
                }

                AdminUI.updateEditView();
                return false;
            }

            return true;
        });
    });
})();