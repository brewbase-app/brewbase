import {
    useEffect,
    useState
} from "react";

import { useNavigate } from "react-router-dom";

import {
    Coffee,
    BookOpen,
    Users,
    Star,
    Trophy
} from "lucide-react";

import {
    getCoffeeRanking,
    getUserRanking,
    getRecipeRanking
} from "../../api/rankingApi";

import "../../styles/Ranking.css";

function Ranking() {
    const [activeTab, setActiveTab] = useState("coffees");

    const [coffeeRanking, setCoffeeRanking] = useState([]);
    const [userRanking, setUserRanking] = useState([]);
    const [recipeRanking, setRecipeRanking] = useState([]);

    const [isCoffeeLoading, setIsCoffeeLoading] = useState(true);
    const [isUserLoading, setIsUserLoading] = useState(true);
    const [isRecipeLoading, setIsRecipeLoading] = useState(true);

    const [coffeeError, setCoffeeError] = useState("");
    const [userError, setUserError] = useState("");
    const [recipeError, setRecipeError] = useState("");

    const navigate = useNavigate();

    useEffect(() => {
        const loadCoffeeRanking = async () => {
            try {
                setIsCoffeeLoading(true);
                setCoffeeError("");

                const data = await getCoffeeRanking();

                const mappedRanking = data.map((coffee) => ({
                    id: coffee.coffeeId,
                    position: coffee.position,
                    name: coffee.name,
                    rating: coffee.averageRating ?? 0,
                    ratingCount: coffee.ratingCount ?? 0,
                    subtitle: [
                        coffee.region,
                        coffee.processingMethod,
                        coffee.variety,
                        coffee.roastery
                    ]
                        .filter(Boolean)
                        .join(" • ")
                }));

                setCoffeeRanking(mappedRanking);
            } catch {
                setCoffeeError("Nie udało się pobrać rankingu kaw.");
            } finally {
                setIsCoffeeLoading(false);
            }
        };

        const loadUserRanking = async () => {
            try {
                setIsUserLoading(true);
                setUserError("");

                const data = await getUserRanking();

                const mappedRanking = data.map((user) => ({
                    id: user.userId,
                    login: user.login,
                    position: user.position,
                    name: user.login,
                    score: user.activityScore ?? 0,
                    subtitle: [
                        `${user.publicRecipeCount ?? 0} receptur`,
                        `${user.followersCount ?? 0} obserwujących`,
                        `${user.coffeeRatingCount ?? 0} ocen kaw`
                    ].join(" • ")
                }));

                setUserRanking(mappedRanking);
            } catch {
                setUserError("Nie udało się pobrać rankingu użytkowników.");
            } finally {
                setIsUserLoading(false);
            }
        };

        const loadRecipeRanking = async () => {
            try {
                setIsRecipeLoading(true);
                setRecipeError("");

                const data = await getRecipeRanking();

                const mappedRanking = data.map((recipe) => ({
                    id: recipe.recipeId,
                    position: recipe.position,
                    name: recipe.title,
                    rating: recipe.averageRating ?? 0,
                    ratingCount: recipe.ratingCount ?? 0,
                    saveCount: recipe.saveCount ?? 0,
                    subtitle: [
                        recipe.coffee,
                        recipe.brewingMethod,
                        recipe.userLogin ? `@${recipe.userLogin}` : null,
                        `${recipe.saveCount ?? 0} zapisów`
                    ]
                        .filter(Boolean)
                        .join(" • ")
                }));

                setRecipeRanking(mappedRanking);
            } catch {
                setRecipeError("Nie udało się pobrać rankingu receptur.");
            } finally {
                setIsRecipeLoading(false);
            }
        };

        loadCoffeeRanking();
        loadUserRanking();
        loadRecipeRanking();
    }, []);

    const handleNavigate = (item) => {
        if (activeTab === "coffees") {
            navigate(`/wiki/coffees/${item.id}`);
        }

        if (activeTab === "recipes") {
            navigate(`/recipes/${item.id}`);
        }

        if (activeTab === "users") {
            navigate(`/profile/${item.login}`);
        }
    };

    const renderPlaceholder = (title, description) => {
        return (
            <div className="leaderboard">
                <div className="leaderboard-header">
                    <h3>{title}</h3>
                </div>

                <p>{description}</p>
            </div>
        );
    };

    const renderScore = (item) => {
        if (activeTab === "users") {
            return (
                <div className="top-rating">
                    <span>
                        {item.score} pkt
                    </span>
                </div>
            );
        }

        return (
            <div className="top-rating">
                <Star size={16} fill="currentColor" />

                <span>
                    {item.rating.toFixed(1)}
                </span>

                <small>
                    ({item.ratingCount} ocen)
                </small>
            </div>
        );
    };

    const renderPodium = (data) => {
        return (
            <div className="top-ranking-podium">
                {data[1] && (
                    <div
                        className="top-card second-place"
                        onClick={() => handleNavigate(data[1])}
                    >
                        <div className="top-badge">
                            #2
                        </div>

                        <h2>{data[1].name}</h2>

                        <p>{data[1].subtitle}</p>

                        {renderScore(data[1])}
                    </div>
                )}

                {data[0] && (
                    <div
                        className="top-card first-place"
                        onClick={() => handleNavigate(data[0])}
                    >
                        <div className="top-badge">
                            <Trophy size={16} />
                            #1
                        </div>

                        <h2>{data[0].name}</h2>

                        <p>{data[0].subtitle}</p>

                        {renderScore(data[0])}
                    </div>
                )}

                {data[2] && (
                    <div
                        className="top-card third-place"
                        onClick={() => handleNavigate(data[2])}
                    >
                        <div className="top-badge">
                            #3
                        </div>

                        <h2>{data[2].name}</h2>

                        <p>{data[2].subtitle}</p>

                        {renderScore(data[2])}
                    </div>
                )}
            </div>
        );
    };

    const renderLeaderboard = (data) => {
        return (
            <>
                {renderPodium(data)}

                <div className="leaderboard">
                    <div className="leaderboard-header">
                        <h3>Pełny ranking</h3>
                    </div>

                    <div className="leaderboard-list">
                        {data.map((item) => (
                            <div
                                className="leaderboard-item"
                                key={item.id}
                                onClick={() => handleNavigate(item)}
                            >
                                <div className="leaderboard-position">
                                    #{item.position}
                                </div>

                                <div className="leaderboard-content">
                                    <h4>{item.name}</h4>

                                    <p>{item.subtitle}</p>
                                </div>

                                <div className="leaderboard-score">
                                    {activeTab === "users" ? (
                                        <>
                                            {item.score} pkt
                                        </>
                                    ) : (
                                        <>
                                            <Star size={16} fill="currentColor" />

                                            {item.rating.toFixed(1)}

                                            <small>
                                                ({item.ratingCount} ocen)
                                            </small>
                                        </>
                                    )}
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            </>
        );
    };

    const renderCoffeeRanking = () => {
        if (isCoffeeLoading) {
            return renderPlaceholder(
                "Ładowanie rankingu...",
                "Pobieramy aktualne dane rankingu kaw."
            );
        }

        if (coffeeError) {
            return renderPlaceholder(
                "Wystąpił błąd",
                coffeeError
            );
        }

        if (coffeeRanking.length === 0) {
            return renderPlaceholder(
                "Brak danych rankingowych",
                "Ranking kaw pojawi się po wystawieniu pierwszych ocen."
            );
        }

        return renderLeaderboard(coffeeRanking);
    };

    const renderRecipeRanking = () => {
        if (isRecipeLoading) {
            return renderPlaceholder(
                "Ładowanie rankingu...",
                "Pobieramy aktualne dane rankingu receptur."
            );
        }

        if (recipeError) {
            return renderPlaceholder(
                "Wystąpił błąd",
                recipeError
            );
        }

        if (recipeRanking.length === 0) {
            return renderPlaceholder(
                "Brak danych rankingowych",
                "Ranking receptur pojawi się po wystawieniu pierwszych ocen publicznym recepturom."
            );
        }

        return renderLeaderboard(recipeRanking);
    };

    const renderUserRanking = () => {
        if (isUserLoading) {
            return renderPlaceholder(
                "Ładowanie rankingu...",
                "Pobieramy aktualne dane rankingu użytkowników."
            );
        }

        if (userError) {
            return renderPlaceholder(
                "Wystąpił błąd",
                userError
            );
        }

        if (userRanking.length === 0) {
            return renderPlaceholder(
                "Brak danych rankingowych",
                "Ranking użytkowników pojawi się po pierwszych aktywnościach."
            );
        }

        return renderLeaderboard(userRanking);
    };

    return (
        <div className="ranking-page">
            <div className="ranking-header">
                <h1>Rankingi</h1>

                <p>
                    Sprawdź najwyżej oceniane kawy, receptury i użytkowników.
                </p>
            </div>

            <div className="ranking-tabs">
                <button
                    className={activeTab === "coffees" ? "active" : ""}
                    onClick={() => setActiveTab("coffees")}
                >
                    <Coffee size={18} />
                    Kawy
                </button>

                <button
                    className={activeTab === "recipes" ? "active" : ""}
                    onClick={() => setActiveTab("recipes")}
                >
                    <BookOpen size={18} />
                    Receptury
                </button>

                <button
                    className={activeTab === "users" ? "active" : ""}
                    onClick={() => setActiveTab("users")}
                >
                    <Users size={18} />
                    Użytkownicy
                </button>
            </div>

            {activeTab === "coffees" && renderCoffeeRanking()}

            {activeTab === "recipes" && renderRecipeRanking()}

            {activeTab === "users" && renderUserRanking()}
        </div>
    );
}

export default Ranking;