import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";

import {
    Heart,
    Star,
    Flag
} from "lucide-react";

import {
    addCoffeeFavorite,
    getCoffeeById,
    rateCoffee,
    removeCoffeeFavorite
} from "../../api/coffeeApi";

import "../../styles/wiki/CoffeeDetails.css";

function CoffeeDetails() {
    const { id } = useParams();
    const navigate = useNavigate();

    const [coffee, setCoffee] = useState(null);
    const [selectedRating, setSelectedRating] = useState(0);
    const [hoveredRating, setHoveredRating] = useState(0);
    const [isRated, setIsRated] = useState(false);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState("");

    const loadCoffee = async () => {
        try {
            setIsLoading(true);
            setError("");

            const data = await getCoffeeById(id);

            setCoffee(data);
        } catch {
            setError("Nie udało się pobrać kawy.");
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        loadCoffee();
    }, [id]);

    const handleFavorite = async () => {
        const wasFavorite = coffee.isFavorite ?? false;

        setCoffee((previous) => ({
            ...previous,
            isFavorite: !wasFavorite
        }));

        try {
            if (wasFavorite) {
                await removeCoffeeFavorite(coffee.id);
            } else {
                await addCoffeeFavorite(coffee.id);
            }
        } catch {
            setCoffee((previous) => ({
                ...previous,
                isFavorite: wasFavorite
            }));
        }
    };

    const handleRating = async () => {
        if (selectedRating === 0) {
            return;
        }

        try {
            setError("");

            await rateCoffee(id, selectedRating);
            await loadCoffee();

            setIsRated(true);
        } catch {
            setError("Nie udało się zapisać oceny. Sprawdź, czy jesteś zalogowany.");
        }
    };

    if (isLoading) {
        return <h1>Ładowanie...</h1>;
    }

    if (error) {
        return <h1>{error}</h1>;
    }

    if (!coffee) {
        return <h1>Nie znaleziono kawy.</h1>;
    }

    const averageRating = coffee.averageRating ?? 0;
    const ratingCount = coffee.ratingCount ?? 0;

    return (
        <div className="article-page">
            <div className="article-hero">
                <div className="article-overlay">
                    <span>
                        {coffee.region ?? "Brak regionu"}
                    </span>

                    <div className="details-top">
                        <h1>
                            {coffee.name}
                        </h1>

                        <button
                            className="details-favorite-button"
                            onClick={handleFavorite}
                        >
                            <Heart
                                size={22}
                                fill={
                                    coffee.isFavorite
                                        ? "currentColor"
                                        : "none"
                                }
                            />
                        </button>
                    </div>

                    <div className="article-tags">
                        {coffee.variety && (
                            <div>
                                {coffee.variety}
                            </div>
                        )}

                        {coffee.processingMethod && (
                            <div>
                                {coffee.processingMethod}
                            </div>
                        )}
                    </div>

                    <div className="rating-section">
                        <span className="rating-title">
                            Oceń tę kawę
                        </span>

                        <div className="average-rating">
                            <div className="average-stars">
                                {[1, 2, 3, 4, 5].map((star) => (
                                    <Star
                                        key={star}
                                        className="average-star"
                                        fill={
                                            star <= Math.floor(averageRating)
                                                ? "currentColor"
                                                : "none"
                                        }
                                    />
                                ))}
                            </div>

                            <span className="average-rating-text">
                                {averageRating.toFixed(1)} ({ratingCount} ocen)
                            </span>
                        </div>

                        <div className="stars-container">
                            {[1, 2, 3, 4, 5].map((star) => (
                                <button
                                    key={star}
                                    className="star-button"
                                    onMouseEnter={() => setHoveredRating(star)}
                                    onMouseLeave={() => setHoveredRating(0)}
                                    onClick={() => setSelectedRating(star)}
                                >
                                    <Star
                                        fill={
                                            star <= (hoveredRating || selectedRating)
                                                ? "currentColor"
                                                : "none"
                                        }
                                    />
                                </button>
                            ))}
                        </div>

                        <button
                            className="submit-rating-button"
                            onClick={handleRating}
                        >
                            Oceń
                        </button>

                        {isRated && (
                            <p className="rating-success">
                                Dziękujemy za ocenę!
                            </p>
                        )}
                    </div>
                </div>
            </div>

            <div className="article-content">
                <section>
                    <h2>Informacje o kawie</h2>

                    <div className="details-tags">
                        {coffee.roastery && (
                            <span>
                                Palarnia: {coffee.roastery}
                            </span>
                        )}

                        {coffee.region && (
                            <span>
                                Region: {coffee.region}
                            </span>
                        )}

                        {coffee.variety && (
                            <span>
                                Odmiana: {coffee.variety}
                            </span>
                        )}

                        {coffee.processingMethod && (
                            <span>
                                Obróbka: {coffee.processingMethod}
                            </span>
                        )}
                    </div>
                </section>

                {coffee.wikiArticle && (
                    <section>
                        <h2>Artykuł wiki</h2>

                        <p className="wiki-article-author">
                            Autor artykułu: {coffee.wikiArticle.authorLogin}
                        </p>

                        <p style={{ whiteSpace: "pre-wrap" }}>
                            {coffee.wikiArticle.content}
                        </p>
                    </section>
                )}

                <section>
                    <button
                        type="button"
                        style={reportButtonStyle}
                        onClick={() =>
                            navigate("/report", {
                                state: {
                                    contentType: "coffee",
                                    contentId: coffee.id,
                                    contentTitle: coffee.name,
                                    returnPath: `/wiki/coffees/${coffee.id}`,
                                },
                            })
                        }
                    >
                        <Flag size={16} />
                        Zgłoś treść
                    </button>
                </section>
            </div>
        </div>
    );
}

const reportButtonStyle = {
    display: "inline-flex",
    alignItems: "center",
    gap: "8px",
    padding: "10px 16px",
    border: "1px solid #d4d4d4",
    borderRadius: "16px",
    background: "transparent",
    color: "#6b6b6b",
    fontSize: "14px",
    fontWeight: "600",
    cursor: "pointer",
};

export default CoffeeDetails;