import { apiRequest } from "./apiClient";

export function getCountries() {
    return apiRequest("/api/countries");
}
