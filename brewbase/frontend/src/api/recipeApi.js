import { apiRequest } from "./apiClient";
export function createRecipe(recipeData) {

    return apiRequest("/api/Recipe", {
        method: "POST",

        body: JSON.stringify(recipeData),
    });
}

export function getRecipes() {

    return apiRequest("/api/Recipe");
}

export function getFavoriteRecipes() {

    return apiRequest("/api/Recipe/favorites");
}

export function deleteRecipe(id) {

    return apiRequest(`/api/Recipe/${id}`, {
        method: "DELETE",
    });
}
export function getRecipeById(id) {

    return apiRequest(`/api/Recipe/${id}`);
}
export function updateRecipe(id, recipeData) {

    return apiRequest(`/api/Recipe/${id}`, {
        method: "PUT",

        body: JSON.stringify(recipeData),
    });
}
export function addFavorite(recipeId) {

    return apiRequest(
        `/api/Recipe/${recipeId}/favorite`,
        {
            method: "POST",
        }
    );
}

export function removeFavorite(recipeId) {

    return apiRequest(
        `/api/Recipe/${recipeId}/favorite`,
        {
            method: "DELETE",
        }
    );
}
export function rateRecipe(recipeId, value) {

    return apiRequest(
        `/api/Recipe/${recipeId}/rating`,
        {
            method: "POST",

            body: JSON.stringify({
                value
            }),
        }
    );
}