import { apiRequest } from "./apiClient";

export function getCountries() {
    return apiRequest("/api/countries");
}

export function searchCountries(query, limit = 20) {
    const params = new URLSearchParams();

    if (query) {
        params.set("q", query);
    }

    if (limit) {
        params.set("limit", String(limit));
    }

    const queryString = params.toString();

    return apiRequest(
        `/api/countries/search${queryString ? `?${queryString}` : ""}`
    );
}

export function createCountry(name) {
    return apiRequest("/api/countries", {
        method: "POST",
        body: JSON.stringify({ name }),
    });
}
