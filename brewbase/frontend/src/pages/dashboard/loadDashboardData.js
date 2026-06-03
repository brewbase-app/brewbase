import { getProfile } from "../../api/profileApi";
import { getQuickNotes } from "../../api/quickNotesApi";
import { getMyRecipes, getFavoriteRecipes } from "../../api/recipeApi";
import { getFavoriteCoffees } from "../../api/coffeeApi";
import {
    getCuppingSessions,
    getCuppingSessionDetails,
} from "../../api/cuppingSessionsApi";
import { getCommunityFeed } from "../../api/communityApi";
import { getNotifications } from "../../api/notificationsApi";
import { getRecommendations } from "../../api/preferenceApi";

export const DASHBOARD_LOAD_ERROR_MESSAGE =
    "Nie udało się załadować danych pulpitu.";

export function createDefaultDashboardApi() {
    return {
        getProfile,
        getQuickNotes,
        getMyRecipes,
        getCuppingSessions,
        getRecommendations,
        getFavoriteCoffees,
        getFavoriteRecipes,
        getNotifications,
        getCommunityFeed,
        getCuppingSessionDetails,
    };
}

function normalizeList(value) {
    return Array.isArray(value) ? value : [];
}

export async function loadDashboardData(api = createDefaultDashboardApi()) {
    try {
        const [
            profileData,
            notesData,
            recipesData,
            sessionsData,
            recommendationsData,
            favoritesCoffeesData,
            favoritesRecipesData,
            notificationsData,
            feedData,
        ] = await Promise.all([
            api.getProfile(),
            api.getQuickNotes(),
            api.getMyRecipes(),
            api.getCuppingSessions(),
            api.getRecommendations(),
            api.getFavoriteCoffees(),
            api.getFavoriteRecipes(),
            api.getNotifications().catch(() => []),
            api.getCommunityFeed().catch(() => []),
        ]);

        let cuppingDetails = [];

        if (Array.isArray(sessionsData) && sessionsData.length > 0) {
            const details = await Promise.all(
                sessionsData.map((session) =>
                    api.getCuppingSessionDetails(session.id).catch(() => null)
                )
            );

            cuppingDetails = details.filter(Boolean);
        }

        return {
            ok: true,
            data: {
                profile: profileData,
                quickNotes: normalizeList(notesData),
                myRecipes: normalizeList(recipesData),
                cuppingSessions: normalizeList(sessionsData),
                cuppingDetails,
                recommendedCoffees: recommendationsData?.coffees ?? [],
                recommendedRecipes: recommendationsData?.recipes ?? [],
                favoriteCoffees: normalizeList(favoritesCoffeesData),
                favoriteRecipes: normalizeList(favoritesRecipesData),
                notifications: normalizeList(notificationsData),
                followingFeed: normalizeList(feedData),
            },
        };
    } catch {
        return {
            ok: false,
            error: DASHBOARD_LOAD_ERROR_MESSAGE,
        };
    }
}
