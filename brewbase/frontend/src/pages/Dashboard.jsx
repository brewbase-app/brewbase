import "../styles/Dashboard.css";

import { useMemo, useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import GlobalSearch from "../components/GlobalSearch";
import { getProfile } from "../api/profileApi";
import { getQuickNotes } from "../api/quickNotesApi";
import { getMyRecipes, getFavoriteRecipes } from "../api/recipeApi";
import { getFavoriteCoffees } from "../api/coffeeApi";
import {
    getCuppingSessions,
    getCuppingSessionDetails,
} from "../api/cuppingSessionsApi";
import { getCoffeeRanking, getRecipeRanking } from "../api/rankingApi";
import { getCommunityFeed } from "../api/communityApi";
import { getNotifications, markNotificationsAsRead } from "../api/notificationsApi";

import {
    Bell,
    Coffee,
    NotebookPen,
    Star,
    FileText,
    Bookmark,
    Users,
} from "lucide-react";

const PERIOD_OPTIONS = [
    { label: "Ostatnie 7 dni", value: 7 },
    { label: "Ostatnie 30 dni", value: 30 },
    { label: "Ostatnie 90 dni", value: 90 },
];

function filterByPeriod(items, days) {
    if (!Array.isArray(items) || !days) {
        return items ?? [];
    }

    const cutoff = Date.now() - days * 24 * 60 * 60 * 1000;

    return items.filter((item) => {
        const dateValue = item.createdAt ?? item.sessionDate;

        if (!dateValue) {
            return true;
        }

        const parsed = new Date(dateValue);

        if (Number.isNaN(parsed.getTime())) {
            return true;
        }

        return parsed.getTime() >= cutoff;
    });
}

function getMostUsedBrewingMethod(recipes) {
    const counts = new Map();

    recipes.forEach((recipe) => {
        const method = recipe.brewingMethod?.trim();

        if (!method) {
            return;
        }

        counts.set(method, (counts.get(method) ?? 0) + 1);
    });

    let topMethod = null;
    let topCount = 0;

    counts.forEach((count, method) => {
        if (count > topCount) {
            topMethod = method;
            topCount = count;
        }
    });

    if (!topMethod) {
        return { name: "—", share: 0 };
    }

    return {
        name: topMethod,
        share: Math.round((topCount / recipes.length) * 100),
    };
}

function getAverageCuppingScore(sessionsDetails) {
    const scores = sessionsDetails
        .flatMap((session) => session.coffees ?? [])
        .map((coffee) => coffee.overallScore)
        .filter((score) => score != null && !Number.isNaN(Number(score)));

    if (scores.length === 0) {
        return null;
    }

    const sum = scores.reduce((total, score) => total + Number(score), 0);

    return (sum / scores.length).toFixed(1);
}

function buildCoffeeSubtitle(coffee) {
    return [
        coffee.region,
        coffee.processingMethod,
        coffee.variety,
        coffee.roastery,
    ]
        .filter(Boolean)
        .join(", ");
}

function isNotificationUnread(notification) {
    return notification.isRead !== true;
}

function Dashboard() {
    const navigate = useNavigate();

    const [selectedPeriod, setSelectedPeriod] = useState(30);
    const [showNotifications, setShowNotifications] = useState(false);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState("");

    const [profile, setProfile] = useState(null);
    const [notifications, setNotifications] = useState([]);
    const [quickNotes, setQuickNotes] = useState([]);
    const [myRecipes, setMyRecipes] = useState([]);
    const [cuppingSessions, setCuppingSessions] = useState([]);
    const [cuppingDetails, setCuppingDetails] = useState([]);
    const [recommendedCoffees, setRecommendedCoffees] = useState([]);
    const [recommendedRecipes, setRecommendedRecipes] = useState([]);
    const [favoriteCoffees, setFavoriteCoffees] = useState([]);
    const [favoriteRecipes, setFavoriteRecipes] = useState([]);
    const [followingFeed, setFollowingFeed] = useState([]);

    useEffect(() => {
        const loadDashboard = async () => {
            setIsLoading(true);
            setError("");

            try {
                const [
                    profileData,
                    notesData,
                    recipesData,
                    sessionsData,
                    coffeesRanking,
                    recipesRanking,
                    favoritesCoffeesData,
                    favoritesRecipesData,
                    notificationsData,
                    feedData,
                ] = await Promise.all([
                    getProfile(),
                    getQuickNotes(),
                    getMyRecipes(),
                    getCuppingSessions(),
                    getCoffeeRanking(4),
                    getRecipeRanking(4),
                    getFavoriteCoffees(),
                    getFavoriteRecipes(),
                    getNotifications().catch(() => []),
                    getCommunityFeed().catch(() => []),
                ]);

                setProfile(profileData);
                setQuickNotes(Array.isArray(notesData) ? notesData : []);
                setMyRecipes(Array.isArray(recipesData) ? recipesData : []);
                setCuppingSessions(Array.isArray(sessionsData) ? sessionsData : []);
                setRecommendedCoffees(Array.isArray(coffeesRanking) ? coffeesRanking : []);
                setRecommendedRecipes(Array.isArray(recipesRanking) ? recipesRanking : []);
                setFavoriteCoffees(Array.isArray(favoritesCoffeesData) ? favoritesCoffeesData : []);
                setFavoriteRecipes(Array.isArray(favoritesRecipesData) ? favoritesRecipesData : []);
                setNotifications(Array.isArray(notificationsData) ? notificationsData : []);
                setFollowingFeed(Array.isArray(feedData) ? feedData : []);

                if (Array.isArray(sessionsData) && sessionsData.length > 0) {
                    const details = await Promise.all(
                        sessionsData.map((session) =>
                            getCuppingSessionDetails(session.id).catch(() => null)
                        )
                    );

                    setCuppingDetails(details.filter(Boolean));
                } else {
                    setCuppingDetails([]);
                }
            } catch {
                setError("Nie udało się załadować danych pulpitu.");
            } finally {
                setIsLoading(false);
            }
        };

        loadDashboard();
    }, []);

    const filteredRecipes = useMemo(
        () => filterByPeriod(myRecipes, selectedPeriod),
        [myRecipes, selectedPeriod]
    );

    const filteredNotes = useMemo(
        () => filterByPeriod(quickNotes, selectedPeriod),
        [quickNotes, selectedPeriod]
    );

    const filteredSessions = useMemo(
        () => filterByPeriod(cuppingSessions, selectedPeriod),
        [cuppingSessions, selectedPeriod]
    );

    const topBrewingMethod = useMemo(
        () => getMostUsedBrewingMethod(filteredRecipes),
        [filteredRecipes]
    );

    const averageCuppingScore = useMemo(
        () => getAverageCuppingScore(cuppingDetails),
        [cuppingDetails]
    );

    const unreadNotificationsCount = useMemo(
        () => notifications.filter(isNotificationUnread).length,
        [notifications]
    );

    const greetingName = profile?.login ?? "Kawoszu";

    const handleNotificationToggle = async () => {
        const willOpen = !showNotifications;
        setShowNotifications(willOpen);

        if (!willOpen || unreadNotificationsCount === 0) {
            return;
        }

        setNotifications((currentNotifications) =>
            currentNotifications.map((notification) => ({
                ...notification,
                isRead: true,
            }))
        );

        try {
            await markNotificationsAsRead();
        } catch {
            // Badge is cleared locally even if backend sync fails.
        }
    };

    const getActivityIcon = (activityType) => {
        if (activityType === "Article") {
            return FileText;
        }

        if (activityType === "Follow") {
            return Users;
        }

        return Star;
    };

    return (
        <div className="dashboard">
            <div className="dashboard-top">
                <div className="dashboard-top-head">
                    <div className="dashboard-greeting">
                        <h1>Dzień dobry, {greetingName}!</h1>
                        <p>Oto co dzieje się w Twoim kawowym świecie.</p>
                    </div>

                    <div className="top-icons">
                        <div
                            className="notification-wrapper"
                            onClick={handleNotificationToggle}
                        >
                            <Bell size={20} />

                            {unreadNotificationsCount > 0 && (
                                <span className="notification-badge">
                                    {unreadNotificationsCount > 99
                                        ? "99+"
                                        : unreadNotificationsCount}
                                </span>
                            )}

                            {showNotifications && (
                                <div className="notifications-dropdown">
                                    {notifications.length === 0 ? (
                                        <div className="notification-item">
                                            Brak powiadomień
                                        </div>
                                    ) : (
                                        notifications.slice(0, 6).map((notification) => (
                                            <div
                                                className="notification-item"
                                                key={notification.id}
                                            >
                                                {notification.content}
                                            </div>
                                        ))
                                    )}
                                </div>
                            )}
                        </div>
                    </div>
                </div>

                <div className="dashboard-top-search">
                    <GlobalSearch />
                </div>
            </div>

            {error && (
                <p className="dashboard-error">{error}</p>
            )}

            <div className="dashboard-layout">
                <div className="dashboard-main">
                    <div className="dashboard-card">
                        <div className="card-header">
                            <h3>Twoje statystyki</h3>

                            <select
                                value={selectedPeriod}
                                onChange={(event) =>
                                    setSelectedPeriod(Number(event.target.value))
                                }
                            >
                                {PERIOD_OPTIONS.map((option) => (
                                    <option
                                        key={option.value}
                                        value={option.value}
                                    >
                                        {option.label}
                                    </option>
                                ))}
                            </select>
                        </div>

                        <div className="stats-grid">
                            <div className="stat-item">
                                <Coffee size={30} />
                                <h2>{isLoading ? "—" : filteredRecipes.length}</h2>
                                <p>Dodane receptury</p>
                            </div>

                            <div className="stat-item">
                                <NotebookPen size={30} />
                                <h2>{isLoading ? "—" : filteredSessions.length}</h2>
                                <p>Cuppingi</p>
                            </div>

                            <div className="stat-item">
                                <FileText size={30} />
                                <h2>{isLoading ? "—" : filteredNotes.length}</h2>
                                <p>Szybkie notatki</p>
                            </div>

                            <div className="stat-item">
                                <Star size={30} />
                                <h2>{isLoading ? "—" : averageCuppingScore ?? "—"}</h2>
                                <p>Średnia ocen cuppingu</p>
                            </div>

                            <div className="stat-item">
                                <Coffee size={30} />
                                <h2>{isLoading ? "—" : topBrewingMethod.name}</h2>
                                <p>Najczęściej używana metoda</p>

                                <div className="mini-progress">
                                    <div
                                        className="mini-progress-fill"
                                        style={{ width: `${topBrewingMethod.share}%` }}
                                    />
                                </div>
                            </div>
                        </div>
                    </div>

                    <div className="dashboard-card">
                        <div className="card-header">
                            <h3>Rekomendowane kawy</h3>
                            <span onClick={() => navigate("/wiki/coffees")}>
                                Zobacz wszystkie kawy
                            </span>
                        </div>

                        <div className="coffee-grid">
                            {isLoading ? (
                                <p className="dashboard-empty">Ładowanie...</p>
                            ) : recommendedCoffees.length === 0 ? (
                                <p className="dashboard-empty">Brak rekomendowanych kaw.</p>
                            ) : (
                                recommendedCoffees.map((coffee) => (
                                    <div
                                        className="coffee-card dashboard-clickable"
                                        key={coffee.coffeeId}
                                        onClick={() =>
                                            navigate(`/wiki/coffees/${coffee.coffeeId}`)
                                        }
                                    >
                                        <div className="coffee-image" />

                                        <h4>{coffee.name}</h4>

                                        <p>{buildCoffeeSubtitle(coffee) || "Brak opisu"}</p>

                                        <div className="rating">
                                            <Star size={14} />
                                            {(coffee.averageRating ?? 0).toFixed(1)}
                                        </div>
                                    </div>
                                ))
                            )}
                        </div>
                    </div>

                    <div className="dashboard-card">
                        <div className="card-header">
                            <h3>Rekomendowane receptury</h3>
                            <span onClick={() => navigate("/recipes/all")}>
                                Zobacz wszystkie receptury
                            </span>
                        </div>

                        <div className="coffee-grid">
                            {isLoading ? (
                                <p className="dashboard-empty">Ładowanie...</p>
                            ) : recommendedRecipes.length === 0 ? (
                                <p className="dashboard-empty">Brak rekomendowanych receptur.</p>
                            ) : (
                                recommendedRecipes.map((recipe) => (
                                    <div
                                        className="coffee-card dashboard-clickable"
                                        key={recipe.recipeId}
                                        onClick={() =>
                                            navigate(`/recipes/${recipe.recipeId}`)
                                        }
                                    >
                                        <div className="coffee-image" />

                                        <h4>{recipe.title}</h4>

                                        <p>
                                            {recipe.userLogin
                                                ? `by ${recipe.userLogin}`
                                                : "Receptura społeczności"}
                                        </p>

                                        <div className="rating">
                                            <Star size={14} />
                                            {(recipe.averageRating ?? 0).toFixed(1)}
                                        </div>
                                    </div>
                                ))
                            )}
                        </div>
                    </div>

                    <div className="dashboard-card">
                        <div className="card-header">
                            <h3>Obserwujesz</h3>
                        </div>

                        <div className="activity-list">
                            {isLoading ? (
                                <p className="dashboard-empty">Ładowanie aktywności...</p>
                            ) : followingFeed.length === 0 ? (
                                <p className="dashboard-empty">
                                    Brak aktywności osób, które obserwujesz.
                                </p>
                            ) : (
                                followingFeed.slice(0, 6).map((activity, index) => {
                                    const Icon = getActivityIcon(activity.activityType);

                                    return (
                                        <div
                                            className="activity-item"
                                            key={`${activity.username}-${activity.createdAt}-${index}`}
                                        >
                                            <Icon size={18} />

                                            <p>
                                                <strong>{activity.username}</strong>
                                                {" "}
                                                {activity.description}
                                            </p>
                                        </div>
                                    );
                                })
                            )}
                        </div>
                    </div>

                    <div className="quick-actions">
                        <button onClick={() => navigate("/recipes/new")}>
                            Dodaj recepturę
                        </button>

                        <button onClick={() => navigate("/cupping/new")}>
                            Dodaj cupping
                        </button>

                        <button onClick={() => navigate("/quicknotes")}>
                            Szybka notatka
                        </button>

                        <button onClick={() => navigate("/wiki/add")}>
                            Dodaj treść wiki
                        </button>
                    </div>
                </div>

                <div className="dashboard-sidebar">
                    <div className="dashboard-card">
                        <div className="card-header">
                            <h3>Ulubione kawy</h3>
                            <span onClick={() => navigate("/favorite-coffees")}>
                                Zobacz wszystkie
                            </span>
                        </div>

                        <div className="favorites-list">
                            {isLoading ? (
                                <p className="dashboard-empty">Ładowanie...</p>
                            ) : favoriteCoffees.length === 0 ? (
                                <p className="dashboard-empty">Brak ulubionych kaw.</p>
                            ) : (
                                favoriteCoffees.slice(0, 4).map((coffee) => (
                                    <div
                                        className="favorite-item dashboard-clickable"
                                        key={coffee.id}
                                        onClick={() =>
                                            navigate(`/wiki/coffees/${coffee.id}`)
                                        }
                                    >
                                        <Bookmark size={18} />

                                        <div>
                                            <h4>{coffee.name}</h4>
                                            <p>
                                                {[coffee.region, coffee.roastery]
                                                    .filter(Boolean)
                                                    .join(", ") || "Brak opisu"}
                                            </p>
                                        </div>
                                    </div>
                                ))
                            )}
                        </div>
                    </div>

                    <div className="dashboard-card">
                        <div className="card-header">
                            <h3>Ulubione receptury</h3>
                            <span onClick={() => navigate("/recipes/favorites")}>
                                Zobacz wszystkie
                            </span>
                        </div>

                        <div className="favorites-list">
                            {isLoading ? (
                                <p className="dashboard-empty">Ładowanie...</p>
                            ) : favoriteRecipes.length === 0 ? (
                                <p className="dashboard-empty">Brak ulubionych receptur.</p>
                            ) : (
                                favoriteRecipes.slice(0, 4).map((recipe) => (
                                    <div
                                        className="favorite-item dashboard-clickable"
                                        key={recipe.id}
                                        onClick={() => navigate(`/recipes/${recipe.id}`)}
                                    >
                                        <Bookmark size={18} />

                                        <div>
                                            <h4>{recipe.title}</h4>
                                            <p>
                                                {[recipe.coffee, recipe.brewingMethod]
                                                    .filter(Boolean)
                                                    .join(" • ") || "Receptura"}
                                            </p>
                                        </div>
                                    </div>
                                ))
                            )}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default Dashboard;
