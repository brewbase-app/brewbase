import { apiRequest } from "./apiClient";

export function getCoffeeRanking() {
    return apiRequest("/api/Ranking/coffees");
}

export function getUserRanking(limit = 100) {
    return apiRequest(`/api/Ranking/users?limit=${limit}`);
}

export function getRecipeRanking() {
    return apiRequest("/api/Ranking/recipes");
}