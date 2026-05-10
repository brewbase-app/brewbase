
import { useState } from "react";

import { useNavigate } from "react-router-dom";

import "../../styles/wiki/Regions.css";

import {
    Search
} from "lucide-react";

function Regions() {

    const navigate = useNavigate();

    const regions = [

        {
            id: 1,
            name: "Yirgacheffe",
            country: "Etiopia",
            altitude: "1800–2200 m n.p.m.",
            flavorProfile: [
                "Jaśmin",
                "Cytrusy"
            ]
        },

        {
            id: 2,
            name: "Huila",
            country: "Kolumbia",
            altitude: "1400–2000 m n.p.m.",
            flavorProfile: [
                "Karmel",
                "Czekolada"
            ]
        },

        {
            id: 3,
            name: "Nyeri",
            country: "Kenia",
            altitude: "1700–2100 m n.p.m.",
            flavorProfile: [
                "Porzeczka",
                "Cytrusy"
            ]
        }
    ];

    const countries = [
        ...new Set(
            regions.map((region) => region.country)
        )
    ];

    const [search, setSearch] = useState("");

    const [selectedCountry, setSelectedCountry] =
        useState("");

    const filteredRegions = regions.filter((region) => {

        const matchesSearch =
            region.name
                .toLowerCase()
                .includes(search.toLowerCase());

        const matchesCountry =
            selectedCountry === "" ||
            region.country === selectedCountry;

        return (
            matchesSearch &&
            matchesCountry
        );
    });

    return (

        <div className="regions-page">

            <div className="regions-header">

                <h1>Regiony</h1>

                <p>
                    Poznaj najważniejsze regiony upraw
                    kaw specialty na świecie.
                </p>

                <div className="regions-search-container">

                    <Search size={18} />

                    <input
                        type="text"
                        placeholder="Szukaj regionów..."
                        className="regions-search"
                        value={search}
                        onChange={(e) =>
                            setSearch(e.target.value)
                        }
                    />

                </div>

            </div>

            <div className="regions-content">

                {/* FILTERS */}

                <div className="regions-filters">

                    <h3>Filtry</h3>

                    <div className="filter-group">

                        <label>Kraj</label>

                        <select
                            value={selectedCountry}
                            onChange={(e) =>
                                setSelectedCountry(
                                    e.target.value
                                )
                            }
                        >

                            <option value="">
                                Wszystkie
                            </option>

                            {countries.map((country) => (

                                <option
                                    key={country}
                                    value={country}
                                >
                                    {country}
                                </option>

                            ))}

                        </select>

                    </div>

                </div>

                {/* GRID */}

                <div className="regions-grid">

                    {filteredRegions.map((region) => (

                        <div
                            className="region-card"
                            key={region.id}
                            onClick={() =>
                                navigate(
                                    `/wiki/regions/${region.id}`
                                )
                            }
                        >

                            <div className="region-image" />

                            <div className="region-card-content">

                                <div className="region-title">

                                    <h2>
                                        {region.name}
                                    </h2>

                                </div>

                                <p>
                                    {region.country}
                                </p>

                                <div className="region-tags">

                                    <span>
                                        {region.altitude}
                                    </span>

                                    {region.flavorProfile.map(
                                        (note) => (

                                            <span key={note}>
                                                {note}
                                            </span>

                                        )
                                    )}

                                </div>

                            </div>

                        </div>

                    ))}

                </div>

            </div>

        </div>

    );
}

export default Regions;
