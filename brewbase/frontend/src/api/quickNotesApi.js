import { apiRequest } from "./apiClient";

export function getQuickNotes(search) {
    const query = search ? `?search=${encodeURIComponent(search)}` : "";
    return apiRequest(`/api/QuickNotes${query}`);
}

export function getQuickNote(id) {
    return apiRequest(`/api/QuickNotes/${id}`);
}

export function createQuickNote(content) {
    return apiRequest("/api/QuickNotes", {
        method: "POST",
        body: JSON.stringify({ content }),
    });
}

export function updateQuickNote(id, content) {
    return apiRequest(`/api/QuickNotes/${id}`, {
        method: "PUT",
        body: JSON.stringify({ content }),
    });
}

export function deleteQuickNote(id) {
    return apiRequest(`/api/QuickNotes/${id}`, {
        method: "DELETE",
    });
}
