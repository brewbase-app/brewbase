import { apiRequest } from "./apiClient";

export function getCoffees() {
    return apiRequest("/api/Coffee");
}

export function getCoffeeById(id) {
    return apiRequest(`/api/Coffee/${id}`);
}

export function lookupCoffeesByName(name, limit = 10) {
    const params = new URLSearchParams();

    if (name) {
        params.set("name", name);
    }

    if (limit) {
        params.set("limit", String(limit));
    }

    const query = params.toString();

    return apiRequest(`/api/Coffee/lookup${query ? `?${query}` : ""}`);
}

export function rateCoffee(id, value) {
    return apiRequest(`/api/Coffee/${id}/rating`, {
        method: "POST",
        body: JSON.stringify({
            value: value
        }),
    });
}

export function addCoffeeFavorite(id) {
    return apiRequest(`/api/Coffee/${id}/favorite`, {
        method: "POST",
    });
}

export function removeCoffeeFavorite(id) {
    return apiRequest(`/api/Coffee/${id}/favorite`, {
        method: "DELETE",
    });
}

export function getFavoriteCoffees() {
    return apiRequest("/api/Coffee/favorites");
}