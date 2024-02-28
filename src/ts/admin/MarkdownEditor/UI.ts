import SaveButtonMode from "./SaveButtonMode";

/**
 * Represents a class which interacts with the editor DOM.
 */
class UI {
    private static readonly SB_ColorClasses = ["danger", "primary", "primary", "success"];
    private static readonly SB_IconClasses = ["exclamation", "floppy-disk", "spinner spin", "circle-check"];
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
        UI._saveButton = document.getElementById("save-button") as HTMLButtonElement;
        UI._content = document.getElementById("content") as HTMLTextAreaElement;
        UI._highlightingContent = document.getElementById("highlighting-content");
        UI._highlighting = document.getElementById("highlighting");

        UI._content.addEventListener("input", () => UI.redraw());
        UI._content.addEventListener("scroll", () => UI.syncEditorScroll());

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

        const button = UI._saveButton;
        button.innerHTML = `<i class="fa-solid fa-fw"></i> ${UI.SB_Text[+mode]}`;

        const icon = button.querySelector("i");
        button.classList.remove(...UI.SB_ColorClasses.map(v => v.split(' ').map(c => `btn-${c}`).reduce((a, b) => a.concat(b))));
        icon.classList.remove(...UI.SB_IconClasses.map(v => v.split(' ').map(c => `fa-${c}`).reduce((a, b) => a.concat(b))));

        UI.SB_ColorClasses[+mode].split(' ').forEach(c => button.classList.add(`btn-${c}`));
        UI.SB_IconClasses[+mode].split(' ').forEach(c => icon.classList.add(`fa-${c}`));

        if (mode !== SaveButtonMode.SAVING) {
            button.removeAttribute("disabled");
        } else {
            button.setAttribute("disabled", "disabled");
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
