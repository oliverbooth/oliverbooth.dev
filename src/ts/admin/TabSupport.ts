import "./Prototypes"
import UI from "./MarkdownEditor/UI";

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

                    const lineStart = sel;
                    while (text[sel] === ' ' || text[sel] === '\t')
                        sel++;

                    if (sel > lineStart) {
                        e.preventDefault();

                        const indentStr = text.slice(lineStart, sel);

                        // insert carriage return and indented text
                        textarea.value = `${text.slice(0, selStart)}\n${indentStr}${text.slice(selStart)}`;

                        // Scroll caret visible
                        textarea.blur();
                        textarea.focus();
                        textarea.selectionEnd = selEnd + indentStr.length + 1; // +1 for \n
                        UI.redraw();
                        return false;
                    }
                }
            }

            return true;
        });
    });
})();