import { apiRequest } from "./apiClient";

export function getTastingSessions() {
    return apiRequest("/api/TastingSessions");
}

export function getTastingSessionDetails(id) {
    return apiRequest(`/api/TastingSessions/${id}`);
}

export function createTastingSession(data) {
    return apiRequest("/api/TastingSessions", {
        method: "POST",
        body: JSON.stringify(data),
    });
}

export function addCoffeeToTastingSession(sessionId, data) {
    return apiRequest(`/api/TastingSessions/${sessionId}/coffees`, {
        method: "POST",
        body: JSON.stringify(data),
    });
}

export function updateTastingSessionCoffee(sessionId, sessionCoffeeId, data) {
    return apiRequest(`/api/TastingSessions/${sessionId}/coffees/${sessionCoffeeId}`, {
        method: "PUT",
        body: JSON.stringify(data),
    });
}