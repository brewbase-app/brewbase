export const USER_PREFERENCES_STORAGE_KEY = "brewbase_user_preferences";

export const DEFAULT_USER_PREFERENCES = {
    experienceLevel: "",
    brewingMethods: [],
    flavorProfiles: [],
    acidity: "",
    body: "",
    regions: [],
    processingMethods: [],
    recommendationStyle: "",
    allowExploration: false,
};

export const USER_PREFERENCE_OPTIONS = {
    experienceLevel: [
        "Początkujący",
        "Średniozaawansowany",
        "Zaawansowany",
        "Jeszcze nie wiem",
    ],
    brewingMethods: [
        "Espresso",
        "V60",
        "Aeropress",
        "French Press",
        "Cold Brew",
        "Jeszcze nie wiem",
    ],
    acidity: ["Niska", "Średnia", "Wysoka", "Nie mam zdania"],
    body: ["Lekkie", "Zbalansowane", "Ciężkie", "Nie mam zdania"],
    regions: [
        "Etiopia",
        "Kolumbia",
        "Brazylia",
        "Kenia",
        "Gwatemala",
        "Nie mam preferencji",
    ],
    recommendationStyle: [
        "Bezpieczne wybory",
        "Zbalansowane",
        "Zaskocz mnie",
    ],
};

export function loadUserPreferences() {
    try {
        const raw = localStorage.getItem(USER_PREFERENCES_STORAGE_KEY);

        if (!raw) {
            return { ...DEFAULT_USER_PREFERENCES };
        }

        const parsed = JSON.parse(raw);

        return {
            ...DEFAULT_USER_PREFERENCES,
            ...parsed,
            brewingMethods: parsed.brewingMethods ?? [],
            flavorProfiles: parsed.flavorProfiles ?? [],
            regions: parsed.regions ?? [],
            processingMethods: parsed.processingMethods ?? [],
        };
    } catch {
        return { ...DEFAULT_USER_PREFERENCES };
    }
}

export function saveUserPreferences(preferences) {
    localStorage.setItem(
        USER_PREFERENCES_STORAGE_KEY,
        JSON.stringify(preferences)
    );
}

export function hasAnyPreferences(preferences) {
    return (
        Boolean(preferences.experienceLevel) ||
        preferences.brewingMethods.length > 0 ||
        preferences.flavorProfiles.length > 0 ||
        Boolean(preferences.acidity) ||
        Boolean(preferences.body) ||
        preferences.regions.length > 0 ||
        Boolean(preferences.recommendationStyle) ||
        preferences.allowExploration
    );
}
