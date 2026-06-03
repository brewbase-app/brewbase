import { apiRequest } from "./apiClient";

export function getBody() {
    return apiRequest("/api/Body");
}

