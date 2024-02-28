import SaveButtonMode from "./SaveButtonMode";

/**
 * Represents a class which interacts with the editor DOM.
 */
class UI {
    private static readonly SB_ColorClasses = ["danger", "primary", "primary", "success"];
    private static readonly SB_IconClasses = ["exclamation", "floppy-disk", "spinner fa-spin", "circle-check"];
    private static readonly SB_Text = ["Error Saving", "Save <span class=\"text-muted\">(Ctrl+S)</span>", "Saving ...", "Saved"];

    private static _highlightingContent: HTMLElement;
    private static _highlighting: HTMLElement;
    private static _content: HTMLTextAreaElement;
    private static _saveButton: HTMLButtonElement;

    private static _saveCallbacks: Function[] = [];

    /**
     * Returns the {@link HTMLTextAreaElement} where the Markdown is inputted by the user.
     */
    public static get markdownInput(): HTMLTextAreaElement {
        return UI._content;
    }

    /**
     * Adds a callback that will be invoked when the save button is pressed.
     * @param callback The callback to invoke.
     */
    public static addSaveButtonListener(callback: Function) {
        if (!callback) {
            return;
        }

        this._saveCallbacks.push(callback);
    }

    /**
     * Initializes the editor's user interface.
     */
    public static init() {
        const content = UI._content;
        UI._highlightingContent = document.getElementById("highlighting-content");
        UI._highlighting = document.getElementById("highlighting");
        content.addEventListener("input", () => UI.redraw());
        content.addEventListener("scroll", () => UI.syncEditorScroll());
        
        UI._saveButton.addEventListener("click", (_: MouseEvent) => {
            UI._saveCallbacks.forEach(fn => fn());
        });
    }

    /**
     * Redraws the Markdown editor UI.
     */
    public static redraw() {
        UI._highlightingContent.innerHTML = Prism.highlight(UI._content.value, Prism.languages.markdown);

        document.querySelectorAll("#highlighting-content span.token.code").forEach(el => {
            const languageSpan = el.querySelector(".code-language") as HTMLSpanElement;
            if (!languageSpan) {
                return;
            }

            const language = languageSpan.innerText;
            const span = el.querySelector(".code-block");
            if (!span) {
                return;
            }

            span.outerHTML = `<code class="${span.className} language-${language}" style="padding:0;">${span.innerHTML}</code>`;
            Prism.highlightAllUnder(languageSpan.parentElement);
        });

        UI.syncEditorScroll();
    }

    /**
     * Sets the display mode of the save button.
     * @param mode The display mode.
     */
    public static setSaveButtonMode(mode: SaveButtonMode) {
        if (mode >= SaveButtonMode._UNUSED) {
            throw new Error("Invalid display mode");
        }

        UI._saveButton.innerHTML = UI.SB_Text[mode];
        UI.SB_ColorClasses.concat(UI.SB_IconClasses).forEach(c => UI._saveButton.classList.remove(c));
        UI._saveButton.classList.add(UI.SB_ColorClasses[mode]);
        UI._saveButton.classList.add(UI.SB_IconClasses[mode]);

        if (mode === SaveButtonMode.NORMAL) {
            UI._saveButton.removeAttribute("disabled");
        } else {
            UI._saveButton.setAttribute("disabled", "disabled");
        }
    }

    /**
     * Synchronises the syntax highlighting element's scroll value with that of the input element.
     */
    public static syncEditorScroll() {
        UI._highlighting.scrollTop = UI._content.scrollTop;
        UI._highlighting.scrollLeft = UI._content.scrollLeft;
    }
}

export default UI;
