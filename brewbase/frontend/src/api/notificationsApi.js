import { apiRequest } from "./apiClient";

export function getNotifications() {
    return apiRequest("/api/notifications");
}

export function markNotificationsAsRead() {
    return apiRequest("/api/notifications/read", {
        method: "POST",
    });
}
