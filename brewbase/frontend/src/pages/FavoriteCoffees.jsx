import { useEffect, useState } from "react";

import "../styles/FavoriteCoffees.css";

import { useNavigate } from "react-router-dom";

import {
    Heart,
    Star
} from "lucide-react";

import {
    getFavoriteCoffees,
    removeCoffeeFavorite,
} from "../api/coffeeApi";

function FavoriteCoffees() {
    const navigate = useNavigate();

    const [favoriteCoffees, setFavoriteCoffees] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadFavorites = async () => {
            try {
                setIsLoading(true);
                setError("");

                const data = await getFavoriteCoffees();
                setFavoriteCoffees(Array.isArray(data) ? data : []);
            } catch {
                setError("Nie udało się pobrać ulubionych kaw.");
            } finally {
                setIsLoading(false);
            }
        };

        loadFavorites();
    }, []);

    const handleRemoveFavorite = async (coffeeId, event) => {
        event.stopPropagation();

        const previous = favoriteCoffees;

        setFavoriteCoffees((current) =>
            current.filter((coffee) => coffee.id !== coffeeId)
        );

        try {
            await removeCoffeeFavorite(coffeeId);
        } catch {
            setFavoriteCoffees(previous);
            alert("Nie udało się usunąć z ulubionych.");
        }
    };

    const getCoffeeNotes = (coffee) => {
        const profiles = Array.isArray(coffee.flavorProfiles)
            ? coffee.flavorProfiles
            : [];

        if (profiles.length > 0) {
            return profiles.join(", ");
        }

        return coffee.roastery ?? "Brak opisu";
    };

    return (
        <div className="favorite-coffees-page">
            <div className="favorite-coffees-header">
                <h1>Ulubione kawy</h1>

                <p>
                    Wszystkie kawy oznaczone przez Ciebie jako ulubione.
                </p>
            </div>

            <div className="favorite-coffees-list">
                {isLoading ? (
                    <p>Ładowanie...</p>
                ) : error ? (
                    <p>{error}</p>
                ) : favoriteCoffees.length === 0 ? (
                    <p>Brak ulubionych kaw.</p>
                ) : (
                    favoriteCoffees.map((coffee) => (
                        <div
                            className="favorite-coffee-card"
                            key={coffee.id}
                            onClick={() =>
                                navigate(`/wiki/coffees/${coffee.id}`)
                            }
                        >
                            <div className="favorite-coffee-image"></div>

                            <div className="favorite-coffee-content">
                                <div className="favorite-coffee-top">
                                    <div>
                                        <h3>{coffee.name}</h3>

                                        <span>
                                            {coffee.beanOriginCountry ??
                                                coffee.region ??
                                                "Brak kraju pochodzenia"}
                                        </span>
                                    </div>

                                    <button
                                        type="button"
                                        onClick={(event) =>
                                            handleRemoveFavorite(
                                                coffee.id,
                                                event
                                            )
                                        }
                                        aria-label="Usuń z ulubionych"
                                    >
                                        <Heart
                                            size={20}
                                            fill="black"
                                        />
                                    </button>
                                </div>

                                <p>{getCoffeeNotes(coffee)}</p>

                                <div className="favorite-coffee-rating">
                                    <Star size={15} />

                                    {coffee.averageRating != null
                                        ? coffee.averageRating.toFixed(1)
                                        : "Brak ocen"}
                                </div>
                            </div>
                        </div>
                    ))
                )}
            </div>
        </div>
    );
}

export default FavoriteCoffees;
