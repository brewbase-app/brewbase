import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import {
    filterByPeriod,
    getAverageCuppingScore,
    getDashboardGreeting,
    getMostUsedBrewingMethod,
    isNotificationUnread,
} from "./dashboardUtils";

describe("getDashboardGreeting", () => {
    it("returns generic greeting while dashboard is loading", () => {
        expect(
            getDashboardGreeting({
                profile: { login: "maria" },
                isLoading: true,
            })
        ).toBe("Dzień dobry!");
    });

    it("returns personalized greeting when profile has login", () => {
        expect(
            getDashboardGreeting({
                profile: { login: "maria" },
                isLoading: false,
            })
        ).toBe("Dzień dobry, maria!");
    });

    it("returns generic greeting when profile is missing after load", () => {
        expect(
            getDashboardGreeting({
                profile: null,
                isLoading: false,
            })
        ).toBe("Dzień dobry!");
    });

    it("ignores blank login values", () => {
        expect(
            getDashboardGreeting({
                profile: { login: "   " },
                isLoading: false,
            })
        ).toBe("Dzień dobry!");
    });
});

describe("filterByPeriod", () => {
    beforeEach(() => {
        vi.useFakeTimers();
        vi.setSystemTime(new Date("2024-06-15T12:00:00Z"));
    });

    afterEach(() => {
        vi.useRealTimers();
    });

    it("keeps items created within selected period", () => {
        const items = [
            { id: 1, createdAt: "2024-06-14T12:00:00Z" },
            { id: 2, createdAt: "2024-05-01T12:00:00Z" },
        ];

        expect(filterByPeriod(items, 7)).toEqual([items[0]]);
    });

    it("returns empty array for non-array input", () => {
        expect(filterByPeriod(null, 7)).toEqual([]);
    });
});

describe("getMostUsedBrewingMethod", () => {
    it("returns placeholder when there are no methods", () => {
        expect(getMostUsedBrewingMethod([])).toEqual({ name: "—", share: 0 });
    });

    it("picks the most frequent brewing method", () => {
        const recipes = [
            { brewingMethod: "V60" },
            { brewingMethod: "V60" },
            { brewingMethod: "Aeropress" },
        ];

        expect(getMostUsedBrewingMethod(recipes)).toEqual({
            name: "V60",
            share: 67,
        });
    });
});

describe("getAverageCuppingScore", () => {
    it("returns null when there are no scores", () => {
        expect(getAverageCuppingScore([])).toBeNull();
    });

    it("averages scores from cupping session details", () => {
        const sessions = [
            { coffees: [{ overallScore: 8 }, { overallScore: 9 }] },
            { coffees: [{ overallScore: 7 }] },
        ];

        expect(getAverageCuppingScore(sessions)).toBe("8.0");
    });
});

describe("isNotificationUnread", () => {
    it("treats missing isRead as unread", () => {
        expect(isNotificationUnread({})).toBe(true);
    });

    it("treats explicitly read notifications as read", () => {
        expect(isNotificationUnread({ isRead: true })).toBe(false);
    });
});
