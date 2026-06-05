import { apiRequest } from "./apiClient";

export function getAcidity() {
    return apiRequest("/api/Acidity");
}