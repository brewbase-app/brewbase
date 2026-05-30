import { apiRequest } from "./apiClient";

export function createArticle({ title, content, module, coffeeId }) {
    const payload = {
        title,
        content,
        module,
    };

    if (coffeeId) {
        payload.coffeeId = coffeeId;
    }

    return apiRequest("/api/articles", {
        method: "POST",
        body: JSON.stringify(payload),
    });
}

export function getArticles(module, search) {
    const params = new URLSearchParams();

    if (module) {
        params.set("module", module);
    }

    if (search) {
        params.set("search", search);
    }

    const query = params.toString();

    return apiRequest(`/api/articles${query ? `?${query}` : ""}`);
}

export function getArticleById(id) {
    return apiRequest(`/api/articles/${id}`);
}

export function getMyArticles(status) {
    const params = new URLSearchParams();

    if (status) {
        params.set("status", status);
    }

    const query = params.toString();

    return apiRequest(`/api/articles/mine${query ? `?${query}` : ""}`);
}

export function getMyArticleById(id) {
    return apiRequest(`/api/articles/mine/${id}`);
}

export function deleteMyArticle(id) {
    return apiRequest(`/api/articles/mine/${id}`, {
        method: "DELETE",
    });
}
