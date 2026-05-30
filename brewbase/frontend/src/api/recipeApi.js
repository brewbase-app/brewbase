import { apiRequest } from "./apiClient";
export function createRecipe(recipeData) {

    return apiRequest("/api/Recipe", {
        method: "POST",

        body: JSON.stringify(recipeData),
    });
}

export function getRecipes(params = {}) {

    const searchParams = new URLSearchParams();

    if (params.userId != null) {
        searchParams.set("userId", String(params.userId));
    }

    const query = searchParams.toString();

    return apiRequest(query ? `/api/Recipe?${query}` : "/api/Recipe");
}

export async function getMyRecipes() {

    const currentUser = await apiRequest("/api/CurrentUser");

    if (currentUser?.userId == null) {
        throw new Error("Nie udało się pobrać danych użytkownika.");
    }

    return getRecipes({ userId: currentUser.userId });
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