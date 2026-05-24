import { useEffect, useState } from "react";

import "../../styles/wiki/Coffees.css";

import { useNavigate } from "react-router-dom";

import {
    Search,
    Heart
} from "lucide-react";

import {
    getFavoriteCoffees,
    toggleFavoriteCoffee
} from "../../utils/favorites";

import {
    getCoffees
} from "../../api/coffeeApi";

function Coffees() {
    const navigate = useNavigate();

    const [coffees, setCoffees] = useState([]);
    const [search, setSearch] = useState("");
    const [selectedRegion, setSelectedRegion] = useState("");
    const [selectedProcessing, setSelectedProcessing] = useState("");
    const [selectedVariety, setSelectedVariety] = useState("");
    const [favorites, setFavorites] = useState(getFavoriteCoffees());
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadCoffees = async () => {
            try {
                setIsLoading(true);
                setError("");

                const data = await getCoffees();

                setCoffees(Array.isArray(data) ? data : []);
            } catch {
                setError("Nie udało się pobrać kaw.");
            } finally {
                setIsLoading(false);
            }
        };

        loadCoffees();
    }, []);

    const regions = [
        ...new Set(
            coffees
                .map((coffee) => coffee.region)
                .filter(Boolean)
        )
    ];

    const processingMethods = [
        ...new Set(
            coffees
                .map((coffee) => coffee.processingMethod)
                .filter(Boolean)
        )
    ];

    const varieties = [
        ...new Set(
            coffees
                .map((coffee) => coffee.variety)
                .filter(Boolean)
        )
    ];

    const handleFavorite = (coffeeId) => {
        const updated = toggleFavoriteCoffee(coffeeId);

        setFavorites(updated);
    };

    const filteredCoffees = coffees.filter((coffee) => {
        const query = search.toLowerCase();

        const matchesSearch =
            (coffee.name ?? "")
                .toLowerCase()
                .includes(query);

        const matchesRegion =
            selectedRegion === "" ||
            coffee.region === selectedRegion;

        const matchesProcessing =
            selectedProcessing === "" ||
            coffee.processingMethod === selectedProcessing;

        const matchesVariety =
            selectedVariety === "" ||
            coffee.variety === selectedVariety;

        return (
            matchesSearch &&
            matchesRegion &&
            matchesProcessing &&
            matchesVariety
        );
    });

    if (isLoading) {
        return (
            <div className="coffees-page">
                <h1>Ładowanie kaw...</h1>
            </div>
        );
    }

    if (error) {
        return (
            <div className="coffees-page">
                <h1>{error}</h1>
            </div>
        );
    }

    return (
        <div className="coffees-page">
            <div className="coffees-header">
                <h1>Kawy</h1>

                <p>
                    Poznaj odmiany kaw specialty i ich pochodzenie.
                </p>

                <div className="coffees-search-container">
                    <Search size={18} />

                    <input
                        type="text"
                        placeholder="Szukaj kaw..."
                        className="coffees-search"
                        value={search}
                        onChange={(event) =>
                            setSearch(event.target.value)
                        }
                    />
                </div>
            </div>

            <div className="coffees-content">
                <div className="coffees-filters">
                    <h3>Filtry</h3>

                    <div className="filter-group">
                        <label>
                            Region
                        </label>

                        <select
                            value={selectedRegion}
                            onChange={(event) =>
                                setSelectedRegion(event.target.value)
                            }
                        >
                            <option value="">
                                Wszystkie
                            </option>

                            {regions.map((region) => (
                                <option
                                    key={region}
                                    value={region}
                                >
                                    {region}
                                </option>
                            ))}
                        </select>
                    </div>

                    <div className="filter-group">
                        <label>
                            Processing
                        </label>

                        <select
                            value={selectedProcessing}
                            onChange={(event) =>
                                setSelectedProcessing(event.target.value)
                            }
                        >
                            <option value="">
                                Wszystkie
                            </option>

                            {processingMethods.map((method) => (
                                <option
                                    key={method}
                                    value={method}
                                >
                                    {method}
                                </option>
                            ))}
                        </select>
                    </div>

                    <div className="filter-group">
                        <label>
                            Variety
                        </label>

                        <select
                            value={selectedVariety}
                            onChange={(event) =>
                                setSelectedVariety(event.target.value)
                            }
                        >
                            <option value="">
                                Wszystkie
                            </option>

                            {varieties.map((variety) => (
                                <option
                                    key={variety}
                                    value={variety}
                                >
                                    {variety}
                                </option>
                            ))}
                        </select>
                    </div>
                </div>

                <div className="coffees-grid">
                    {filteredCoffees.map((coffee) => (
                        <div
                            className="coffee-card"
                            key={coffee.id}
                            onClick={() =>
                                navigate(`/wiki/coffees/${coffee.id}`)
                            }
                        >
                            <div className="coffee-image" />

                            <div className="coffee-card-content">
                                <div className="coffee-top">
                                    <h2>
                                        {coffee.name}
                                    </h2>

                                    <button
                                        className="favorite-button"
                                        onClick={(event) => {
                                            event.stopPropagation();

                                            handleFavorite(coffee.id);
                                        }}
                                    >
                                        <Heart
                                            size={18}
                                            fill={
                                                favorites.includes(coffee.id)
                                                    ? "currentColor"
                                                    : "none"
                                            }
                                        />
                                    </button>
                                </div>

                                <p>
                                    {coffee.region ?? "Brak regionu"}
                                </p>

                                <div className="coffee-tags">
                                    {coffee.variety && (
                                        <span>
                                            {coffee.variety}
                                        </span>
                                    )}

                                    {coffee.processingMethod && (
                                        <span>
                                            {coffee.processingMethod}
                                        </span>
                                    )}
                                </div>

                                <small>
                                    {coffee.roastery ?? "Brak palarni"}
                                </small>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
}

export default Coffees;