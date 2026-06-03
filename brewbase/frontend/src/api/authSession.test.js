import { beforeEach, describe, expect, it, vi } from "vitest";

import { establishAuthSession } from "./authSession";
import { getAuthToken, getUserRole } from "../utils/auth";

vi.mock("./profileApi", () => ({
    getProfile: vi.fn(),
}));

import { getProfile } from "./profileApi";

describe("establishAuthSession", () => {
    beforeEach(() => {
        vi.mocked(getProfile).mockReset();
    });

    it("stores token and role from profile", async () => {
        vi.mocked(getProfile).mockResolvedValue({
            userId: 1,
            login: "tester",
            role: "User",
        });

        await establishAuthSession("jwt-token");

        expect(getAuthToken()).toBe("jwt-token");
        expect(getUserRole()).toBe("User");
    });

    it("stores token and clears role when profile request fails", async () => {
        vi.mocked(getProfile).mockRejectedValue(new Error("Unauthorized"));

        await establishAuthSession("jwt-token");

        expect(getAuthToken()).toBe("jwt-token");
        expect(getUserRole()).toBeNull();
    });
});
