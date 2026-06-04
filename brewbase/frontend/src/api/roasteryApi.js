import { apiRequest } from "./apiClient";

export function getRoasteries() {
    return apiRequest("/api/roasteries");
}

export function searchRoasteries(query, limit = 20) {
    const params = new URLSearchParams();

    if (query) {
        params.set("q", query);
    }

    if (limit) {
        params.set("limit", String(limit));
    }

    const queryString = params.toString();

    return apiRequest(
        `/api/roasteries/search${queryString ? `?${queryString}` : ""}`
    );
}

export function createRoastery(name) {
    return apiRequest("/api/roasteries", {
        method: "POST",
        body: JSON.stringify({ name }),
    });
}
