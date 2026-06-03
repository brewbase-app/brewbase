import { apiRequest } from "./apiClient";

export function getFlavorProfiles() {
    return apiRequest("/api/flavor-profiles");
}

export function getRandomFlavorProfiles(limit = 10) {
    const params = new URLSearchParams();

    if (limit) {
        params.set("limit", String(limit));
    }

    const query = params.toString();

    return apiRequest(
        `/api/flavor-profiles/random${query ? `?${query}` : ""}`
    );
}

export function getOnboardingFlavorProfiles(limit = 10) {
    const params = new URLSearchParams();

    if (limit) {
        params.set("limit", String(limit));
    }

    const query = params.toString();

    return apiRequest(
        `/api/flavor-profiles/onboarding${query ? `?${query}` : ""}`
    );
}

export function searchFlavorProfiles(query, limit = 20) {
    const params = new URLSearchParams();

    if (query) {
        params.set("q", query);
    }

    if (limit) {
        params.set("limit", String(limit));
    }

    const queryString = params.toString();

    return apiRequest(
        `/api/flavor-profiles/search${queryString ? `?${queryString}` : ""}`
    );
}

export function createFlavorProfile(name) {
    return apiRequest("/api/flavor-profiles", {
        method: "POST",
        body: JSON.stringify({ name }),
    });
}
