import { describe, expect, it, vi } from "vitest";

import { savePreferences } from "./preferenceApi";

vi.mock("./apiClient", () => ({
    apiRequest: vi.fn(),
}));

import { apiRequest } from "./apiClient";

describe("preferenceApi", () => {
    it("posts preferences to backend", async () => {
        vi.mocked(apiRequest).mockResolvedValue(null);

        const payload = {
            experienceLevel: "Początkujący",
            preferredRoastLevel: "Średnie",
            preferredAcidity: "Niska",
            preferredBody: "Lekkie",
            recommendationStyle: "Zbalansowane",
            allowExploration: false,
            flavorProfiles: ["Owocowe"],
            brewingMethods: ["V60"],
            regions: ["Etiopia"],
        };

        await savePreferences(payload);

        expect(apiRequest).toHaveBeenCalledWith("/api/preferences", {
            method: "POST",
            body: JSON.stringify(payload),
        });
    });
});
