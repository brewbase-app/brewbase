import { apiRequest } from "./apiClient";

export function getCoffeeRanking() {
    return apiRequest("/api/Ranking/coffees");
}

export function getUserRanking() {
    return apiRequest("/api/Ranking/users");
}