import BlogPost from "../app/BlogPost";
import UI from "./MarkdownEditor/UI"
import API from "../app/API";
import "./TabSupport"
import Interop from "./Interop";
import SaveButtonMode from "./MarkdownEditor/SaveButtonMode";
import EditorJS from "@editorjs/editorjs";
import Header from "@editorjs/header";
import CodeTool from "@editorjs/code";
import SimpleImage from "./BlockTools/SimpleImage";
import Utility from "../app/Utility";

(() => {
    getCurrentBlogPost().then(async post => {
        if (!post) {
            return;
        }

        await Utility.delay(1000); // hack to wait for setDotNetHelper invocation. TODO fix this shit
        const blocks = JSON.parse(await Interop.invoke<string>("GetEditorObject", post.id));
        console.log("JSON object is", blocks);

        // UI.init();
        // UI.addSaveButtonListener(savePost);

        const editor = new EditorJS({
            autofocus: true,
            tools: {
                code: CodeTool,
                header: {
                    class: Header,
                    config: {
                        placeholder: 'Heading',
                        levels: [2, 3, 4],
                        defaultLevel: 2
                    }
                },
                image: SimpleImage
            },
            data: blocks
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