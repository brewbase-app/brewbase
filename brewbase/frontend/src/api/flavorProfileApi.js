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

export function createFlavorProfile(name) {
    return apiRequest("/api/flavor-profiles", {
        method: "POST",
        body: JSON.stringify({ name }),
    });
}
