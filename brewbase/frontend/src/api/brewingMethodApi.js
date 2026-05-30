import { apiRequest } from "./apiClient";

export function getBrewingMethods() {
    return apiRequest("/api/BrewingMethods");
}
