import { useState } from "react";

import { useParams } from "react-router-dom";

import {
    Heart,
    Star
} from "lucide-react";

import {
    getFavoriteCoffees,
    toggleFavoriteCoffee
} from "../../utils/favorites";

import "../../styles/wiki/CoffeeDetails.css";

function CoffeeDetails() {

    const { id } = useParams();

    const [favorites, setFavorites] = useState(
        getFavoriteCoffees()
    );

    const [selectedRating, setSelectedRating] =
        useState(0);

    const [hoveredRating, setHoveredRating] =
        useState(0);

    const [isRated, setIsRated] =
        useState(false);

    const coffees = [

        {
            id: 1,
            name: "Geisha",
            region: "Etiopia",
            variety: "Geisha",
            processingMethod: "Washed",
            roastery: "Coffee Lab",

            rating: {
                average: 4.8,
                count: 128
            },

            description:
                "Geisha to jedna z najbardziej cenionych odmian specialty coffee. Znana jest z wyjątkowej floralności, wysokiej słodyczy i herbacianego body.",

            flavorNotes: [
                "Jaśmin",
                "Bergamotka",
                "Cytrusy",
                "Miód"
            ],

            brewing: [
                "V60",
                "Origami",
                "Chemex"
            ]
        },

        {
            id: 2,
            name: "Bourbon",
            region: "Brazylia",
            variety: "Bourbon",
            processingMethod: "Natural",
            roastery: "Story Coffee",

            rating: {
                average: 4.4,
                count: 72
            },

            description:
                "Bourbon charakteryzuje się dużą słodyczą, czekoladowym profilem i niską kwasowością.",

            flavorNotes: [
                "Czekolada",
                "Karmel",
                "Orzechy"
            ],

            brewing: [
                "French Press",
                "Espresso"
            ]
        }
    ];

    const coffee = coffees.find(
        (coffee) => coffee.id === Number(id)
    );

    const handleFavorite = () => {

        const updated =
            toggleFavoriteCoffee(coffee.id);

        setFavorites(updated);
    };

    const handleRating = () => {

        if (selectedRating > 0) {

            setIsRated(true);
        }
    };

    if (!coffee) {
        return <h1>Nie znaleziono artykułu.</h1>;
    }

    return (

        <div className="article-page">

            <div className="article-hero">

                <div className="article-overlay">

                    <span>
                        {coffee.region}
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
                                    favorites.includes(coffee.id)
                                        ? "currentColor"
                                        : "none"
                                }
                            />

                        </button>

                    </div>

                    <div className="article-tags">

                        <div>
                            {coffee.variety}
                        </div>

                        <div>
                            {coffee.processingMethod}
                        </div>

                    </div>

                    {/* RATING */}

                    <div className="rating-section">

                        <span className="rating-title">
                            Oceń tę kawę
                        </span>

                        {/* AVERAGE */}

                        <div className="average-rating">

                            <div className="average-stars">

                                {[1, 2, 3, 4, 5].map((star) => (

                                    <Star
                                        key={star}
                                        className="average-star"
                                        fill={
                                            star <= Math.floor(
                                                coffee.rating.average
                                            
                                            )
                                                ? "currentColor"
                                                : "none"
                                        }
                                    />

                                ))}

                            </div>

                            <span className="average-rating-text">

                                {coffee.rating.average}

                                {" "}

                                ({coffee.rating.count} ocen)

                            </span>

                        </div>

                        {/* USER RATING */}

                        <div className="stars-container">

                            {[1, 2, 3, 4, 5].map((star) => (

                                <button
                                    key={star}
                                    className="star-button"
                                    onMouseEnter={() =>
                                        setHoveredRating(star)
                                    }
                                    onMouseLeave={() =>
                                        setHoveredRating(0)
                                    }
                                    onClick={() =>
                                        setSelectedRating(star)
                                    }
                                >

                                    <Star
                                        fill={
                                            star <=
                                            (
                                                hoveredRating ||
                                                selectedRating
                                            )
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

                    <h2>Opis</h2>

                    <p>
                        {coffee.description}
                    </p>

                </section>

                <section>

                    <h2>Profil smakowy</h2>

                    <div className="details-tags">

                        {coffee.flavorNotes.map((note) => (

                            <span key={note}>
                                {note}
                            </span>

                        ))}

                    </div>

                </section>

                <section>

                    <h2>Rekomendowane metody parzenia</h2>

                    <div className="details-tags">

                        {coffee.brewing.map((method) => (

                            <span key={method}>
                                {method}
                            </span>

                        ))}

                    </div>

                </section>

            </div>

        </div>

    );
}

export default CoffeeDetails;