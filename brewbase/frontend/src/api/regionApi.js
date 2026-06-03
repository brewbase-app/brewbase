import { apiRequest } from "./apiClient";

export function getRegions(countryId) {
    const params = new URLSearchParams();

    if (countryId != null && countryId !== "") {
        params.set("countryId", String(countryId));
    }

    const query = params.toString();

    return apiRequest(`/api/regions${query ? `?${query}` : ""}`);
}
