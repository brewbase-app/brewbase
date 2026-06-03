import { describe, expect, it, vi } from "vitest";

import {
    DASHBOARD_LOAD_ERROR_MESSAGE,
    loadDashboardData,
} from "./loadDashboardData";
import { sampleProfile } from "../../test/fixtures";

function createSuccessfulApi(overrides = {}) {
    return {
        getProfile: vi.fn().mockResolvedValue(sampleProfile),
        getQuickNotes: vi.fn().mockResolvedValue([]),
        getMyRecipes: vi.fn().mockResolvedValue([]),
        getCuppingSessions: vi.fn().mockResolvedValue([]),
        getRecommendations: vi
            .fn()
            .mockResolvedValue({ coffees: [{ coffeeId: 1 }], recipes: [] }),
        getFavoriteCoffees: vi.fn().mockResolvedValue([]),
        getFavoriteRecipes: vi.fn().mockResolvedValue([]),
        getNotifications: vi.fn().mockResolvedValue([]),
        getCommunityFeed: vi.fn().mockResolvedValue([]),
        getCuppingSessionDetails: vi.fn().mockResolvedValue(null),
        ...overrides,
    };
}

describe("loadDashboardData", () => {
    it("returns normalized dashboard data when all calls succeed", async () => {
        const api = createSuccessfulApi({
            getQuickNotes: vi.fn().mockResolvedValue([{ id: 1 }]),
            getCuppingSessions: vi
                .fn()
                .mockResolvedValue([{ id: 10, sessionDate: "2024-01-01" }]),
            getCuppingSessionDetails: vi
                .fn()
                .mockResolvedValue({ id: 10, coffees: [{ overallScore: 8 }] }),
        });

        const result = await loadDashboardData(api);

        expect(result.ok).toBe(true);
        expect(result.data.profile).toEqual(sampleProfile);
        expect(result.data.quickNotes).toEqual([{ id: 1 }]);
        expect(result.data.recommendedCoffees).toEqual([{ coffeeId: 1 }]);
        expect(result.data.cuppingDetails).toEqual([
            { id: 10, coffees: [{ overallScore: 8 }] },
        ]);
        expect(api.getCuppingSessionDetails).toHaveBeenCalledWith(10);
    });

    it("returns error result when a critical api call fails", async () => {
        const api = createSuccessfulApi({
            getProfile: vi.fn().mockRejectedValue(new Error("fail")),
        });

        const result = await loadDashboardData(api);

        expect(result).toEqual({
            ok: false,
            error: DASHBOARD_LOAD_ERROR_MESSAGE,
        });
    });

    it("still succeeds when optional feed and notifications fail", async () => {
        const api = createSuccessfulApi({
            getNotifications: vi.fn().mockRejectedValue(new Error("fail")),
            getCommunityFeed: vi.fn().mockRejectedValue(new Error("fail")),
        });

        const result = await loadDashboardData(api);

        expect(result.ok).toBe(true);
        expect(result.data.notifications).toEqual([]);
        expect(result.data.followingFeed).toEqual([]);
    });

    it("skips failed cupping details without failing the whole load", async () => {
        const api = createSuccessfulApi({
            getCuppingSessions: vi.fn().mockResolvedValue([{ id: 11 }]),
            getCuppingSessionDetails: vi
                .fn()
                .mockRejectedValue(new Error("details fail")),
        });

        const result = await loadDashboardData(api);

        expect(result.ok).toBe(true);
        expect(result.data.cuppingDetails).toEqual([]);
    });
});
