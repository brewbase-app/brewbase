import { apiRequest } from "./apiClient";

export async function getPreferences() {
    return apiRequest("/api/Preferences");
}

export async function savePreferences(data) {
    return apiRequest("/api/preferences", {
        method: "POST",
        body: JSON.stringify(data),
    });
}

export async function getRecommendations() {
    return apiRequest("/api/recommendations");
}

export async function submitRecommendationSummaryFeedback(data) {
    return apiRequest("/api/recommendations/summary-feedback", {
        method: "POST",
        body: JSON.stringify(data),
    });
}