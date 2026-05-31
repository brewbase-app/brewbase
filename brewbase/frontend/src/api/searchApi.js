import { apiRequest } from "./apiClient";

export function globalSearch(query, limit = 30) {
    const params = new URLSearchParams();

    if (query) {
        params.set("query", query);
    }

    if (limit) {
        params.set("limit", String(limit));
    }

    const queryString = params.toString();
    return apiRequest(`/api/search${queryString ? `?${queryString}` : ""}`);
}
