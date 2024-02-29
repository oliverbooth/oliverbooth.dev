import BlogPost from "../app/BlogPost";
import UI from "./MarkdownEditor/UI"
import API from "../app/API";
import "./TabSupport"
import Interop from "./Interop";
import SaveButtonMode from "./MarkdownEditor/SaveButtonMode";
import EditorJS from "@editorjs/editorjs";
import Header from "@editorjs/header";
import SimpleImage from "./BlockTools/SimpleImage";

(() => {
    getCurrentBlogPost().then(post => {
        if (!post) {
            return;
        }

        // UI.init();
        // UI.addSaveButtonListener(savePost);

        const editor = new EditorJS({
            tools: {
                header: {
                    class: Header,
                    config: {
                        placeholder: 'Heading',
                        levels: [2, 3, 4],
                        defaultLevel: 2
                    }
                },
                image: SimpleImage
            }
        });

        /*const editor = new MarkdownEditor(UI.markdownInput);
        editor.addSaveListener(savePost);
        editor.registerDefaultShortcuts();
        editor.registerEvents();*/

        async function savePost(): Promise<void> {
            return;
            UI.setSaveButtonMode(SaveButtonMode.SAVING);
            await Interop.invoke("Save", post.id, UI.markdownInput.value);
            post = await API.getBlogPost(post.id);
            UI.setSaveButtonMode(SaveButtonMode.SAVED);
            UI.setPreviewContent(post.content);
            UI.redraw();

            setTimeout(() => UI.setSaveButtonMode(SaveButtonMode.NORMAL), 2000);
        }

        // UI.redraw();
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