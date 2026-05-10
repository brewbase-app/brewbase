import { useState } from "react";

import { useNavigate } from "react-router-dom";

import "../../styles/wiki/BrewingMethods.css";

import {
    Search
} from "lucide-react";

function BrewingMethods() {

    const navigate = useNavigate();

    const methods = [

        {
            id: 1,
            name: "V60",
            type: "Pour over",
            difficulty: "Średni",
            brewTime: "2:30–3:00",
            description:
                "Jedna z najpopularniejszych metod przelewowych specialty coffee.",
        },

        {
            id: 2,
            name: "Chemex",
            type: "Pour over",
            difficulty: "Łatwy",
            brewTime: "4:00–5:00",
            description:
                "Metoda zapewniająca bardzo czysty i delikatny napar.",
        },

        {
            id: 3,
            name: "French Press",
            type: "Immersion",
            difficulty: "Łatwy",
            brewTime: "4:00",
            description:
                "Klasyczna metoda immersyjna dająca pełne body i intensywny smak.",
        }
    ];

    const types = [
        ...new Set(
            methods.map((method) => method.type)
        )
    ];

    const [search, setSearch] = useState("");

    const [selectedType, setSelectedType] =
        useState("");

    const filteredMethods = methods.filter((method) => {

        const matchesSearch =
            method.name
                .toLowerCase()
                .includes(search.toLowerCase());

        const matchesType =
            selectedType === "" ||
            method.type === selectedType;

        return (
            matchesSearch &&
            matchesType
        );
    });

    return (

        <div className="methods-page">

            <div className="methods-header">

                <h1>Metody parzenia</h1>

                <p>
                    Poznaj najpopularniejsze metody
                    parzenia kaw specialty.
                </p>

                <div className="methods-search-container">

                    <Search size={18} />

                    <input
                        type="text"
                        placeholder="Szukaj metod..."
                        className="methods-search"
                        value={search}
                        onChange={(e) =>
                            setSearch(e.target.value)
                        }
                    />

                </div>

            </div>

            <div className="methods-content">

                {/* FILTERS */}

                <div className="methods-filters">

                    <h3>Filtry</h3>

                    <div className="filter-group">

                        <label>Typ metody</label>

                        <select
                            value={selectedType}
                            onChange={(e) =>
                                setSelectedType(
                                    e.target.value
                                )
                            }
                        >

                            <option value="">
                                Wszystkie
                            </option>

                            {types.map((type) => (

                                <option
                                    key={type}
                                    value={type}
                                >
                                    {type}
                                </option>

                            ))}

                        </select>

                    </div>

                </div>

                {/* GRID */}

                <div className="methods-grid">

                    {filteredMethods.map((method) => (

                        <div
                            className="method-card"
                            key={method.id}
                            onClick={() =>
                                navigate(
                                    `/wiki/methods/${method.id}`
                                )
                            }
                        >

                            <div className="method-image" />

                            <div className="method-card-content">

                                <div className="method-top">

                                    <h2>
                                        {method.name}
                                    </h2>

                                    <span>
                                        {method.difficulty}
                                    </span>

                                </div>

                                <p>
                                    {method.description}
                                </p>

                                <div className="method-tags">

                                    <span>
                                        {method.type}
                                    </span>

                                    <span>
                                        {method.brewTime}
                                    </span>

                                </div>

                            </div>

                        </div>

                    ))}

                </div>

            </div>

        </div>

    );
}

export default BrewingMethods;

