import BlogPost from "../app/BlogPost";
import UI from "./MarkdownEditor/UI"
import API from "../app/API";
import "./TabSupport"
import Interop from "./Interop";
import MarkdownEditor from "./MarkdownEditor/MarkdownEditor";
import SaveButtonMode from "./MarkdownEditor/SaveButtonMode";

(() => {
    getCurrentBlogPost().then(post => {
        if (!post) {
            return;
        }

        UI.init();
        UI.addSaveButtonListener(savePost);

        const editor = new MarkdownEditor(UI.markdownInput);
        editor.addSaveListener(savePost);
        editor.registerDefaultShortcuts();
        editor.registerEvents();

        async function savePost(): Promise<void> {
            UI.setSaveButtonMode(SaveButtonMode.SAVING);
            await Interop.invoke("Save", post.id, UI.markdownInput.value);
            post = await API.getBlogPost(post.id);
            UI.setSaveButtonMode(SaveButtonMode.SAVED);

            setTimeout(() => UI.setSaveButtonMode(SaveButtonMode.NORMAL), 2000);
        }

        UI.redraw();
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