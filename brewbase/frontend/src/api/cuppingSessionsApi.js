import { apiRequest } from "./apiClient";

export function getCuppingSessions() {
    return apiRequest("/api/CuppingSessions");
}

export function getCuppingSessionDetails(id) {
    return apiRequest(`/api/CuppingSessions/${id}`);
}

export function createCuppingSession(data) {
    return apiRequest("/api/CuppingSessions", {
        method: "POST",
        body: JSON.stringify(data),
    });
}

export function updateCuppingSession(id, data) {
    return apiRequest(`/api/CuppingSessions/${id}`, {
        method: "PUT",
        body: JSON.stringify(data),
    });
}

export function deleteCuppingSession(id) {
    return apiRequest(`/api/CuppingSessions/${id}`, {
        method: "DELETE",
    });
}

export function addCoffeeToCuppingSession(sessionId, data) {
    return apiRequest(`/api/CuppingSessions/${sessionId}/coffees`, {
        method: "POST",
        body: JSON.stringify(data),
    });
}

export function updateCuppingSessionCoffee(sessionId, sessionCoffeeId, data) {
    return apiRequest(`/api/CuppingSessions/${sessionId}/coffees/${sessionCoffeeId}`, {
        method: "PUT",
        body: JSON.stringify(data),
    });
}

export function deleteCuppingSessionCoffee(sessionId, sessionCoffeeId) {
    return apiRequest(`/api/CuppingSessions/${sessionId}/coffees/${sessionCoffeeId}`, {
        method: "DELETE",
    });
}
