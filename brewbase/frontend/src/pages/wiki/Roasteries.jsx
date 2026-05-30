import { useState } from "react";

import { useNavigate } from "react-router-dom";

import {
    Search,
    Flame,
    MapPin
} from "lucide-react";

import "../../styles/wiki/Roasteries.css";

import WikiArticlesSection from "../../components/WikiArticlesSection";

function Roasteries() {

    const navigate = useNavigate();

    const [search, setSearch] =
        useState("");

    const roasteries = [

        {
            id: 1,
            name: "Coffee Collective",
            country: "Dania",
            city: "Kopenhaga",
            roastingStyle: "Light Roast",
            specialty: "Nordic Specialty Coffee"
        },

        {
            id: 2,
            name: "Audun Coffee",
            country: "Norwegia",
            city: "Oslo",
            roastingStyle: "Omni Roast",
            specialty: "Competition Coffees"
        },

        {
            id: 3,
            name: "Story Coffee Roasters",
            country: "Polska",
            city: "Warszawa",
            roastingStyle: "Light Roast",
            specialty: "Filter Coffee"
        },

        {
            id: 4,
            name: "Coffee Plant",
            country: "Polska",
            city: "Warszawa",
            roastingStyle: "Espresso Roast",
            specialty: "Espresso Blends"
        }
    ];

    const filteredRoasteries =
        roasteries.filter((roastery) => {

            const query =
                search.toLowerCase();

            return (

                roastery.name
                    .toLowerCase()
                    .includes(query)

                ||

                roastery.country
                    .toLowerCase()
                    .includes(query)

                ||

                roastery.city
                    .toLowerCase()
                    .includes(query)

                ||

                roastery.specialty
                    .toLowerCase()
                    .includes(query)
            );
        });

    return (

        <div className="roasteries-page">

            {/* HEADER */}

            <div className="roasteries-header">

                <h1>
                    Palarnie
                </h1>

                <p>
                    Odkrywaj palarnie specialty coffee
                    z całego świata.
                </p>

                <div className="roasteries-search-container">

                    <Search size={18} />

                    <input
                        type="text"
                        placeholder="Szukaj palarni..."
                        className="roasteries-search"
                        value={search}
                        onChange={(e) =>
                            setSearch(e.target.value)
                        }
                    />

                </div>

            </div>

            {/* GRID */}

            <div className="roasteries-grid">

                {filteredRoasteries.map((roastery) => (

                    <div
                        key={roastery.id}
                        className="roastery-card"
                        onClick={() =>
                            navigate(
                                `/wiki/roasteries/${roastery.id}`
                            )
                        }
                    >

                        <div className="roastery-card-top">

                            <div className="roastery-icon">

                                <Flame size={22} />

                            </div>

                        </div>

                        <div className="roastery-content">

                            <h2>
                                {roastery.name}
                            </h2>

                            <div className="roastery-location">

                                <MapPin size={15} />

                                <span>

                                    {roastery.city},
                                    {" "}
                                    {roastery.country}

                                </span>

                            </div>

                            <div className="roastery-tags">

                                <span>
                                    {roastery.roastingStyle}
                                </span>

                                <span>
                                    {roastery.specialty}
                                </span>

                            </div>

                        </div>

                    </div>

                ))}

            </div>

            <WikiArticlesSection
                module="roastery"
                gridClassName="roasteries-grid"
                cardClassName="roastery-card"
            />

        </div>

    );
}

export default Roasteries;