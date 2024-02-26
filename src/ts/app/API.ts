import BlogPost from "./BlogPost";
import Author from "./Author";

class API {
    private static readonly BASE_URL: string = "/api/v1";
    private static readonly BLOG_URL: string = "/blog";

    static async getBlogPostCount(): Promise<number> {
        const response = await API.get(`${API.BLOG_URL}/count`);
        return response.count;
    }

    static async getBlogPost(id: string): Promise<BlogPost> {
        const response = await API.get(`${API.BLOG_URL}/post/${id}`);
        return new BlogPost(response);
    }

    static async getBlogPosts(page: number): Promise<BlogPost[]> {
        const response = await API.get(`${API.BLOG_URL}/posts/${page}`);
        return response.map(obj => new BlogPost(obj));
    }

    static async getBlogPostsByTag(tag: string, page: number): Promise<BlogPost[]> {
        const response = await API.get(`${API.BLOG_URL}/posts/tagged/${tag}/${page}`);
        return response.map(obj => new BlogPost(obj));
    }

    static async getAuthor(id: string): Promise<Author> {
        const response = await API.get(`${API.BLOG_URL}/author/${id}`);
        return new Author(response);
    }

    static async updatePost(post: BlogPost, options: any): Promise<BlogPost> {
        try {
            await API.patch(`/post/${post.id}`, {body: JSON.stringify(options)});
        } catch {
            return post;
        }

        return await API.getBlogPost(post.id);
    }

    private static get(url: string, options?: any): Promise<any> {
        return API.perform(url, "GET", options);
    }

    private static patch(url: string, options?: any): Promise<any> {
        return API.perform(url, "PATCH", options);
    }

    private static async perform(url: string, method: string, options?: any): Promise<any> {
        const opt = Object.assign({}, options, {method: method});
        console.log("Sending options", opt);
        const response = await fetch(`${API.BASE_URL}${url}`, opt);
        if (response.status !== 200) {
            throw new Error("Invalid response from server");
        }

        const text = await response.text();
        return JSON.parse(text);
    }
}

export default API;