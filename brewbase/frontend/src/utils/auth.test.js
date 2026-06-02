import { describe, expect, it } from "vitest";

import {
    clearAuthToken,
    clearUserRole,
    getAuthToken,
    getUserRole,
    isAdmin,
    logout,
    setAuthToken,
    setUserRole,
} from "./auth";

describe("auth utils", () => {
    it("stores and reads auth token", () => {
        setAuthToken("jwt-token");

        expect(getAuthToken()).toBe("jwt-token");
    });

    it("stores and reads user role", () => {
        setUserRole("Admin");

        expect(getUserRole()).toBe("Admin");
        expect(isAdmin()).toBe(true);
    });

    it("clears role when setUserRole receives falsy value", () => {
        setUserRole("User");
        setUserRole(null);

        expect(getUserRole()).toBeNull();
    });

    it("logout clears token and role", () => {
        setAuthToken("jwt-token");
        setUserRole("User");

        logout();

        expect(getAuthToken()).toBeNull();
        expect(getUserRole()).toBeNull();
    });

    it("clearAuthToken removes only token", () => {
        setAuthToken("jwt-token");
        setUserRole("User");

        clearAuthToken();

        expect(getAuthToken()).toBeNull();
        expect(getUserRole()).toBe("User");
    });

    it("clearUserRole removes only role", () => {
        setAuthToken("jwt-token");
        setUserRole("User");

        clearUserRole();

        expect(getAuthToken()).toBe("jwt-token");
        expect(getUserRole()).toBeNull();
    });
});
