import { apiRequest } from "./apiClient";

export function getCoffeeRanking(limit = 10) {
    return apiRequest(`/api/Ranking/coffees?limit=${limit}`);
}

export function getUserRanking(limit = 100) {
    return apiRequest(`/api/Ranking/users?limit=${limit}`);
}

export function getRecipeRanking(limit = 10) {
    return apiRequest(`/api/Ranking/recipes?limit=${limit}`);
}