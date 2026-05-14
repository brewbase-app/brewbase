import { apiRequest } from "./apiClient";

export function getCoffeeRanking() {
    return apiRequest("/api/Ranking/coffees");
}