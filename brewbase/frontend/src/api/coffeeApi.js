import { apiRequest } from "./apiClient";

export function getCoffees() {
    return apiRequest("/api/Coffee");
}