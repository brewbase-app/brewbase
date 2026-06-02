import { afterEach, describe, expect, it, vi } from "vitest";

import { ApiError, apiRequest } from "./apiClient";
import { setAuthToken } from "../utils/auth";

describe("apiClient", () => {
    afterEach(() => {
        vi.unstubAllGlobals();
    });

    it("returns parsed JSON for successful responses", async () => {
        vi.stubGlobal(
            "fetch",
            vi.fn().mockResolvedValue({
                ok: true,
                status: 200,
                text: async () => JSON.stringify({ id: 1, login: "tester" }),
            })
        );

        await expect(apiRequest("/api/test")).resolves.toEqual({
            id: 1,
            login: "tester",
        });
    });

    it("returns null for 204 responses", async () => {
        vi.stubGlobal(
            "fetch",
            vi.fn().mockResolvedValue({
                ok: true,
                status: 204,
                text: async () => "",
            })
        );

        await expect(apiRequest("/api/test", { method: "DELETE" })).resolves.toBeNull();
    });

    it("throws ApiError with message from JSON body", async () => {
        vi.stubGlobal(
            "fetch",
            vi.fn().mockResolvedValue({
                ok: false,
                status: 400,
                text: async () =>
                    JSON.stringify({ message: "Validation failed" }),
            })
        );

        await expect(apiRequest("/api/test")).rejects.toMatchObject({
            name: "ApiError",
            message: "Validation failed",
            status: 400,
        });
    });

    it("throws ApiError with passwordHint on failed login", async () => {
        vi.stubGlobal(
            "fetch",
            vi.fn().mockResolvedValue({
                ok: false,
                status: 401,
                text: async () =>
                    JSON.stringify({
                        message: "Unauthorized",
                        passwordHint: "moja podpowiedz",
                    }),
            })
        );

        try {
            await apiRequest("/api/Auth/login", { method: "POST" });
            throw new Error("Expected apiRequest to throw");
        } catch (error) {
            expect(error).toBeInstanceOf(ApiError);
            expect(error.status).toBe(401);
            expect(error.passwordHint).toBe("moja podpowiedz");
        }
    });

    it("sends Authorization header when token exists", async () => {
        setAuthToken("jwt-token");

        const fetchMock = vi.fn().mockResolvedValue({
            ok: true,
            status: 200,
            text: async () => "{}",
        });

        vi.stubGlobal("fetch", fetchMock);

        await apiRequest("/api/secure");

        expect(fetchMock).toHaveBeenCalledWith(
            "/api/secure",
            expect.objectContaining({
                headers: expect.objectContaining({
                    Authorization: "Bearer jwt-token",
                }),
            })
        );
    });
});
