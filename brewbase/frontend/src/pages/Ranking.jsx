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

import { getCoffeeRanking } from "../api/rankingApi";

import "../styles/Ranking.css";

function Ranking() {
    const [activeTab, setActiveTab] = useState("coffees");
    const [coffeeRanking, setCoffeeRanking] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState("");

    const navigate = useNavigate();

    useEffect(() => {
        const loadCoffeeRanking = async () => {
            try {
                setIsLoading(true);
                setError("");

                const data = await getCoffeeRanking();

                const mappedRanking = data.map((coffee) => ({
                    id: coffee.coffeeId,
                    position: coffee.position,
                    name: coffee.name,
                    rating: coffee.averageRating ?? 0,
                    ratingCount: coffee.ratingCount ?? 0,
                    recipeUsedCount: coffee.recipeUsedCount ?? 0,
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
                setError("Nie udało się pobrać rankingu kaw.");
            } finally {
                setIsLoading(false);
            }
        };

        loadCoffeeRanking();
    }, []);

    const handleNavigate = (itemId) => {
        if (activeTab === "coffees") {
            navigate(`/wiki/coffees/${itemId}`);
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

    const renderCoffeeRating = (item) => {
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

    const renderPodium = () => {
        return (
            <div className="top-ranking-podium">
                {coffeeRanking[1] && (
                    <div
                        className="top-card second-place"
                        onClick={() => handleNavigate(coffeeRanking[1].id)}
                    >
                        <div className="top-badge">
                            #2
                        </div>

                        <h2>{coffeeRanking[1].name}</h2>

                        <p>{coffeeRanking[1].subtitle}</p>

                        {renderCoffeeRating(coffeeRanking[1])}
                    </div>
                )}

                {coffeeRanking[0] && (
                    <div
                        className="top-card first-place"
                        onClick={() => handleNavigate(coffeeRanking[0].id)}
                    >
                        <div className="top-badge">
                            <Trophy size={16} />
                            #1
                        </div>

                        <h2>{coffeeRanking[0].name}</h2>

                        <p>{coffeeRanking[0].subtitle}</p>

                        {renderCoffeeRating(coffeeRanking[0])}
                    </div>
                )}

                {coffeeRanking[2] && (
                    <div
                        className="top-card third-place"
                        onClick={() => handleNavigate(coffeeRanking[2].id)}
                    >
                        <div className="top-badge">
                            #3
                        </div>

                        <h2>{coffeeRanking[2].name}</h2>

                        <p>{coffeeRanking[2].subtitle}</p>

                        {renderCoffeeRating(coffeeRanking[2])}
                    </div>
                )}
            </div>
        );
    };

    const renderCoffeeRanking = () => {
        if (isLoading) {
            return renderPlaceholder(
                "Ładowanie rankingu...",
                "Pobieramy aktualne dane rankingu kaw."
            );
        }

        if (error) {
            return renderPlaceholder(
                "Wystąpił błąd",
                error
            );
        }

        if (coffeeRanking.length === 0) {
            return renderPlaceholder(
                "Brak danych rankingowych",
                "Ranking kaw pojawi się po wystawieniu pierwszych ocen."
            );
        }

        return (
            <>
                {renderPodium()}

                <div className="leaderboard">
                    <div className="leaderboard-header">
                        <h3>Pełny ranking</h3>
                    </div>

                    <div className="leaderboard-list">
                        {coffeeRanking.map((item) => (
                            <div
                                className="leaderboard-item"
                                key={item.id}
                                onClick={() => handleNavigate(item.id)}
                            >
                                <div className="leaderboard-position">
                                    #{item.position}
                                </div>

                                <div className="leaderboard-content">
                                    <h4>{item.name}</h4>

                                    <p>{item.subtitle}</p>
                                </div>

                                <div className="leaderboard-score">
                                    <Star size={16} fill="currentColor" />

                                    {item.rating.toFixed(1)}

                                    <small>
                                        ({item.ratingCount} ocen)
                                    </small>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            </>
        );
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

            {activeTab === "recipes" &&
                renderPlaceholder(
                    "Ranking receptur będzie dostępny później",
                    "Ta część zostanie podpięta po implementacji backendowego rankingu receptur."
                )}

            {activeTab === "users" &&
                renderPlaceholder(
                    "Ranking użytkowników będzie dostępny później",
                    "Ta część zostanie podpięta po implementacji backendowego rankingu użytkowników."
                )}
        </div>
    );
}

export default Ranking;