import { apiRequest } from "./apiClient";

export function getCuppingSessions() {
    return apiRequest("/api/TastingSessions");
}

export function getCuppingSessionDetails(id) {
    return apiRequest(`/api/TastingSessions/${id}`);
}

export function createCuppingSession(data) {
    return apiRequest("/api/TastingSessions", {
        method: "POST",
        body: JSON.stringify(data),
    });
}

export function updateCuppingSession(id, data) {
    return apiRequest(`/api/TastingSessions/${id}`, {
        method: "PUT",
        body: JSON.stringify(data),
    });
}

export function deleteCuppingSession(id) {
    return apiRequest(`/api/TastingSessions/${id}`, {
        method: "DELETE",
    });
}

export function addCoffeeToCuppingSession(sessionId, data) {
    return apiRequest(`/api/TastingSessions/${sessionId}/coffees`, {
        method: "POST",
        body: JSON.stringify(data),
    });
}

export function updateCuppingSessionCoffee(sessionId, sessionCoffeeId, data) {
    return apiRequest(`/api/TastingSessions/${sessionId}/coffees/${sessionCoffeeId}`, {
        method: "PUT",
        body: JSON.stringify(data),
    });
}

export function deleteCuppingSessionCoffee(sessionId, sessionCoffeeId) {
    return apiRequest(`/api/TastingSessions/${sessionId}/coffees/${sessionCoffeeId}`, {
        method: "DELETE",
    });
}
