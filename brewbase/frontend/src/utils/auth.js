const TOKEN_STORAGE_KEY = "token";
const ROLE_STORAGE_KEY = "userRole";

export function getAuthToken() {
    return localStorage.getItem(TOKEN_STORAGE_KEY);
}

export function setAuthToken(token) {
    localStorage.setItem(TOKEN_STORAGE_KEY, token);
}

export function clearAuthToken() {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
}

export function getUserRole() {
    return localStorage.getItem(ROLE_STORAGE_KEY);
}

export function setUserRole(role) {
    if (role) {
        localStorage.setItem(ROLE_STORAGE_KEY, role);
        return;
    }

    localStorage.removeItem(ROLE_STORAGE_KEY);
}

export function clearUserRole() {
    localStorage.removeItem(ROLE_STORAGE_KEY);
}

export function isAdmin() {
    return getUserRole() === "Admin";
}

export function logout() {
    clearAuthToken();
    clearUserRole();
}
