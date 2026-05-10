import { useState } from "react";

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

function Coffees() {

    const navigate = useNavigate();

    const coffees = [

        {
            id: 1,
            name: "Geisha",
            region: "Etiopia",
            variety: "Geisha",
            processingMethod: "Washed",
            roastery: "Coffee Lab"
        },

        {
            id: 2,
            name: "Bourbon",
            region: "Brazylia",
            variety: "Bourbon",
            processingMethod: "Natural",
            roastery: "Story Coffee"
        },

        {
            id: 3,
            name: "SL28",
            region: "Kenia",
            variety: "SL28",
            processingMethod: "Honey",
            roastery: "Audun Coffee"
        },

        {
            id: 4,
            name: "Typica",
            region: "Kolumbia",
            variety: "Typica",
            processingMethod: "Washed",
            roastery: "Coffee Plant"
        }
    ];

    /* dynamic filters */

    const regions = [
        ...new Set(
            coffees.map((coffee) => coffee.region)
        )
    ];

    const processingMethods = [
        ...new Set(
            coffees.map(
                (coffee) => coffee.processingMethod
            )
        )
    ];

    const varieties = [
        ...new Set(
            coffees.map((coffee) => coffee.variety)
        )
    ];

    /* state */

    const [search, setSearch] = useState("");

    const [selectedRegion, setSelectedRegion] =
        useState("");

    const [selectedProcessing, setSelectedProcessing] =
        useState("");

    const [selectedVariety, setSelectedVariety] =
        useState("");

    const [favorites, setFavorites] = useState(
        getFavoriteCoffees()
    );

    /* favorites */

    const handleFavorite = (coffeeId) => {

        const updated =
            toggleFavoriteCoffee(coffeeId);

        setFavorites(updated);
    };

    /* filtering */

    const filteredCoffees = coffees.filter((coffee) => {

        const matchesSearch =
            coffee.name
                .toLowerCase()
                .includes(search.toLowerCase());

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
                        onChange={(e) =>
                            setSearch(e.target.value)
                        }
                    />

                </div>

            </div>

            <div className="coffees-content">

                {/* FILTERS */}

                <div className="coffees-filters">

                    <h3>Filtry</h3>

                    {/* REGION */}

                    <div className="filter-group">

                        <label>
                            Region
                        </label>

                        <select
                            value={selectedRegion}
                            onChange={(e) =>
                                setSelectedRegion(
                                    e.target.value
                                )
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

                    {/* PROCESSING */}

                    <div className="filter-group">

                        <label>
                            Processing
                        </label>

                        <select
                            value={selectedProcessing}
                            onChange={(e) =>
                                setSelectedProcessing(
                                    e.target.value
                                )
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

                    {/* VARIETY */}

                    <div className="filter-group">

                        <label>
                            Variety
                        </label>

                        <select
                            value={selectedVariety}
                            onChange={(e) =>
                                setSelectedVariety(
                                    e.target.value
                                )
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

                {/* COFFEES */}

                <div className="coffees-grid">

                    {filteredCoffees.map((coffee) => (

                        <div
                            className="coffee-card"
                            key={coffee.id}
                            onClick={() =>
                                navigate(
                                    `/wiki/coffees/${coffee.id}`
                                )
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
                                        onClick={(e) => {

                                            e.stopPropagation();

                                            handleFavorite(
                                                coffee.id
                                            );
                                        }}
                                    >

                                        <Heart
                                            size={18}
                                            fill={
                                                favorites.includes(
                                                    coffee.id
                                                )
                                                    ? "currentColor"
                                                    : "none"
                                            }
                                        />

                                    </button>

                                </div>

                                <p>
                                    {coffee.region}
                                </p>

                                <div className="coffee-tags">

                                    <span>
                                        {coffee.variety}
                                    </span>

                                    <span>
                                        {
                                            coffee.processingMethod
                                        }
                                    </span>

                                </div>

                                <small>
                                    {coffee.roastery}
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