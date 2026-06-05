import { describe, expect, it } from "vitest";

import {
    DEFAULT_USER_PREFERENCES,
    USER_PREFERENCES_STORAGE_KEY,
    hasAnyPreferences,
    loadUserPreferences,
    saveUserPreferences,
} from "./userPreferences";

describe("userPreferences utils", () => {
    it("returns defaults when storage is empty", () => {
        expect(loadUserPreferences()).toEqual(DEFAULT_USER_PREFERENCES);
    });

    it("saves and loads preferences from localStorage", () => {
        const preferences = {
            ...DEFAULT_USER_PREFERENCES,
            experienceLevel: "Początkujący",
            brewingMethods: ["V60"],
        };

        saveUserPreferences(preferences);

        expect(localStorage.getItem(USER_PREFERENCES_STORAGE_KEY)).not.toBeNull();
        expect(loadUserPreferences()).toEqual(preferences);
    });

    it("merges partial stored data with defaults", () => {
        localStorage.setItem(
            USER_PREFERENCES_STORAGE_KEY,
            JSON.stringify({
                experienceLevel: "Zaawansowany",
            })
        );

        expect(loadUserPreferences()).toEqual({
            ...DEFAULT_USER_PREFERENCES,
            experienceLevel: "Zaawansowany",
        });
    });

    it("returns defaults when stored JSON is invalid", () => {
        localStorage.setItem(USER_PREFERENCES_STORAGE_KEY, "{not-json");

        expect(loadUserPreferences()).toEqual(DEFAULT_USER_PREFERENCES);
    });

    it("detects whether any preference was selected", () => {
        expect(hasAnyPreferences(DEFAULT_USER_PREFERENCES)).toBe(false);

        expect(
            hasAnyPreferences({
                ...DEFAULT_USER_PREFERENCES,
                regions: ["Kenia"],
            })
        ).toBe(true);
    });
});
