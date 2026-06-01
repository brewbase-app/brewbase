export class ApiError extends Error {
    constructor(message, status, errors = {}, passwordHint = null) {
        super(message);
        this.name = "ApiError";
        this.status = status;
        this.errors = errors;
        this.passwordHint = passwordHint;
    }
}

import { getAuthToken } from "../utils/auth";

const API_BASE = import.meta.env.VITE_API_URL ?? "";

export async function apiRequest(path, options = {}) {
    const token = getAuthToken();
    const hasBody = options.body != null && options.body !== "";

    const response = await fetch(`${API_BASE}${path}`, {
        ...options,
        headers: {
            ...(hasBody ? { "Content-Type": "application/json" } : {}),
            ...(token ? { Authorization: `Bearer ${token}` } : {}),
            ...options.headers,
        },
    });

    if (!response.ok) {
        const errorText = await response.text();
        let message = errorText || "Request failed";
        let errors = {};
        let passwordHint = null;

        try {
            const parsed = JSON.parse(errorText);
            if (parsed?.message) {
                message = parsed.message;
            } else if (parsed?.title) {
                message = parsed.title;
            }
            if (parsed?.errors && typeof parsed.errors === "object") {
                errors = parsed.errors;
            }
            if (parsed?.passwordHint) {
                passwordHint = parsed.passwordHint;
            }
        } catch {
            // keep raw text
        }

        throw new ApiError(message, response.status, errors, passwordHint);
    }

    if (response.status === 204) {
        return null;
    }

    const responseText = await response.text();

    if (!responseText) {
        return null;
    }

    return JSON.parse(responseText);
}
