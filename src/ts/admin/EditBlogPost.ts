import BlogPost from "../app/BlogPost";
import API from "../app/API";
import UI from "../app/UI";

declare const Prism: any;

(() => {
    getCurrentBlogPost().then(post => {
        if (!post) {
            return;
        }

        const saveButton = document.getElementById("save-button") as HTMLButtonElement;
        const preview = document.getElementById("article-preview") as HTMLAnchorElement;
        const content = document.getElementById("content") as HTMLTextAreaElement;
        const title = document.getElementById("post-title") as HTMLInputElement;
        const highlighting = document.getElementById("highlighting");
        const highlightingContent = document.getElementById("highlighting-content");

        saveButton.addEventListener("click", async (e: MouseEvent) => {
            await savePost();
        });

        document.addEventListener("keydown", async (e: KeyboardEvent) => {
            if (e.ctrlKey && e.key === "s") {
                e.preventDefault();
                await savePost();
                preview.innerHTML = post.content;
                UI.updateUI(preview);
                // Prism.highlightAllUnder(preview);
            }
        });

        content.addEventListener("keydown", async (e: KeyboardEvent) => {
            if (e.key === "Tab") {
                e.preventDefault();

                const start = content.selectionStart;
                const end = content.selectionEnd;
                const text = content.value;
                content.value = `${text.slice(0, start)}    ${text.slice(start, end)}`;
                updateEditView();
                content.selectionStart = start + 4;
                content.selectionEnd = end ? end + 4 : start + 4;
            }
        });

        async function savePost(): Promise<void> {
            saveButton.classList.add("btn-primary");
            saveButton.classList.remove("btn-success");

            saveButton.setAttribute("disabled", "disabled");
            saveButton.innerHTML = '<i class="fa-solid fa-spinner fa-spin fa-fw"></i> Saving ...';

            post = await API.updatePost(post, {content: content.value, title: title.value});

            saveButton.classList.add("btn-success");
            saveButton.classList.remove("btn-primary");
            saveButton.removeAttribute("disabled");
            saveButton.innerHTML = '<i class="fa-solid fa-circle-check fa-fw"></i> Saved';

            setTimeout(() => {
                saveButton.classList.add("btn-primary");
                saveButton.classList.remove("btn-success");
                saveButton.innerHTML = '<i class="fa-solid fa-floppy-disk fa-fw"></i> Save <span class="text-muted">(Ctrl+S)</span>';
            }, 2000);
        }

        updateEditView();
        content.addEventListener("input", () => updateEditView());
        content.addEventListener("scroll", () => syncEditorScroll());
        function updateEditView() {
            highlightingContent.innerHTML = Prism.highlight(content.value, Prism.languages.markdown);
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
                Prism.highlightAllUnder(highlightingContent);
            });
            syncEditorScroll();
        }
        
        function syncEditorScroll() {
            highlighting.scrollTop = content.scrollTop;
            highlighting.scrollLeft = content.scrollLeft;
        }
    });

    async function getCurrentBlogPost(): Promise<BlogPost> {
        const blogPostRef: Element = document.querySelector('input[type="hidden"][data-blog-pid]');

        if (blogPostRef) {
            const pid: string = blogPostRef.getAttribute("data-blog-pid");
            return await API.getBlogPost(pid);
        }

        return null;
    }
})();