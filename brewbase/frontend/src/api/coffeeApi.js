import { apiRequest } from "./apiClient";

export function getCoffees() {
    return apiRequest("/api/Coffee");
}

export function getCoffeeById(id) {
    return apiRequest(`/api/Coffee/${id}`);
}

export function rateCoffee(id, value) {
    return apiRequest(`/api/Coffee/${id}/rating`, {
        method: "POST",
        body: JSON.stringify({
            value: value
        }),
    });
}