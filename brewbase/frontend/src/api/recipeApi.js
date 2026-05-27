import { apiRequest } from "./apiClient";

export function getRecipes(params = {}) {
    const searchParams = new URLSearchParams();

    Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== "") {
            searchParams.append(key, value);
        }
    });

    const query = searchParams.toString();

    return apiRequest(`/api/Recipe${query ? `?${query}` : ""}`);
}

export function getRecipeById(id) {
    return apiRequest(`/api/Recipe/${id}`);
}

export function getFavoriteRecipes() {
    return apiRequest("/api/Recipe/favorites");
}

export function addRecipeFavorite(id) {
    return apiRequest(`/api/Recipe/${id}/favorite`, {
        method: "POST",
    });
}

export function removeRecipeFavorite(id) {
    return apiRequest(`/api/Recipe/${id}/favorite`, {
        method: "DELETE",
    });
}
