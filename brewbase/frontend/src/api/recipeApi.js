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