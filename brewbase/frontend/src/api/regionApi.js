import { apiRequest } from "./apiClient";

export function getRegions(countryId) {
    const params = new URLSearchParams();

    if (countryId != null && countryId !== "") {
        params.set("countryId", String(countryId));
    }

    const query = params.toString();

    return apiRequest(`/api/regions${query ? `?${query}` : ""}`);
}

export function searchRegions(countryId, query, limit = 20) {
    const params = new URLSearchParams();

    params.set("countryId", String(countryId));

    if (query) {
        params.set("q", query);
    }

    if (limit) {
        params.set("limit", String(limit));
    }

    const queryString = params.toString();

    return apiRequest(`/api/regions/search?${queryString}`);
}

export function createRegion(name, countryId) {
    return apiRequest("/api/regions", {
        method: "POST",
        body: JSON.stringify({ name, countryId }),
    });
}
