import adminUI from "./AdminUI";

declare const Prism: any;
class AdminUI {
    static highlightingContent: HTMLElement;
    static highlighting: HTMLElement;
    static _content: HTMLTextAreaElement;
    static _saveButton: HTMLButtonElement;
    
    static init() {
        const content = AdminUI.content;
        AdminUI.highlightingContent = document.getElementById("highlighting-content");
        AdminUI.highlighting = document.getElementById("highlighting");
        content.addEventListener("input", () => AdminUI.updateEditView());
        content.addEventListener("scroll", () => AdminUI.syncEditorScroll());
    }

    public static get content() {
        if (!AdminUI._content) {
            AdminUI._content = document.getElementById("content") as HTMLTextAreaElement;
        }
        
        return AdminUI._content;
    }

    public static get saveButton() {
        if (!AdminUI._saveButton) {
            AdminUI._saveButton = document.getElementById("save-button") as HTMLButtonElement;
        }

        return AdminUI._saveButton;
    }
    
    public static updateEditView() {
        AdminUI.highlightingContent.innerHTML = Prism.highlight(AdminUI.content.value, Prism.languages.markdown);

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

        AdminUI.syncEditorScroll();
    }

    static syncEditorScroll() {
        AdminUI.highlighting.scrollTop = AdminUI._content.scrollTop;
        AdminUI.highlighting.scrollLeft = AdminUI._content.scrollLeft;
    }
}

export default AdminUI;