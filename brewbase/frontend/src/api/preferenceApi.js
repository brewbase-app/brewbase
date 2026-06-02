import { apiRequest } from "./apiClient";

export async function savePreferences(data) {
    return apiRequest("/api/preferences", {
        method: "POST",
        body: JSON.stringify(data),
    });
}

export async function getRecommendations() {
    return apiRequest("/api/recommendations");
}