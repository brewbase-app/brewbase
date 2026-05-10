import { useState } from "react";

import { useNavigate } from "react-router-dom";

import {
    Coffee,
    BookOpen,
    Users,
    Star,
    Trophy
} from "lucide-react";

import "../styles/Ranking.css";

function Ranking() {

    const [activeTab, setActiveTab] =
        useState("coffees");

    const navigate = useNavigate();

    /* MOCK DATA */

    const rankingData = {

        coffees: [

            {
                id: 1,
                name: "Geisha Panama",
                rating: 4.9,
                ratingCount: 248,
                subtitle: "Etiopia • Washed"
            },

            {
                id: 2,
                name: "Kenya AA",
                rating: 4.7,
                ratingCount: 192,
                subtitle: "Kenia • SL28"
            },

            {
                id: 3,
                name: "Brazil Cerrado",
                rating: 4.5,
                ratingCount: 140,
                subtitle: "Brazylia • Natural"
            }
        ],

        recipes: [

            {
                id: 1,
                name: "V60 Light Roast",
                rating: 4.8,
                ratingCount: 178,
                subtitle: "by coffeelover"
            },

            {
                id: 2,
                name: "Classic Espresso",
                rating: 4.6,
                ratingCount: 112,
                subtitle: "by brewmaster"
            },

            {
                id: 3,
                name: "Origami Bloom",
                rating: 4.5,
                ratingCount: 96,
                subtitle: "by dripperking"
            }
        ],

        users: [

            {
                id: 1,
                name: "CoffeeLover",
                score: 3210,
                subtitle: "32 receptury"
            },

            {
                id: 2,
                name: "BaristaTom",
                score: 2840,
                subtitle: "28 receptur"
            },

            {
                id: 3,
                name: "AsiaBeans",
                score: 2480,
                subtitle: "21 receptur"
            }
        ]
    };

    const currentData =
        rankingData[activeTab];

    /* NAVIGATION */

    const handleNavigate = (itemId) => {

        if (activeTab === "coffees") {

            navigate(`/wiki/coffees/${itemId}`);
        }

        if (activeTab === "recipes") {

            navigate(`/recipes/${itemId}`);
        }

        if (activeTab === "users") {

            navigate(`/profile/${itemId}`);
        }
    };

    return (

        <div className="ranking-page">

            {/* HEADER */}

            <div className="ranking-header">

                <h1>
                    Rankingi
                </h1>

                <p>
                    Sprawdź najwyżej oceniane
                    kawy, receptury i użytkowników.
                </p>

            </div>

            {/* TABS */}

            <div className="ranking-tabs">

                <button
                    className={
                        activeTab === "coffees"
                            ? "active"
                            : ""
                    }
                    onClick={() =>
                        setActiveTab("coffees")
                    }
                >

                    <Coffee size={18} />

                    Kawy

                </button>

                <button
                    className={
                        activeTab === "recipes"
                            ? "active"
                            : ""
                    }
                    onClick={() =>
                        setActiveTab("recipes")
                    }
                >

                    <BookOpen size={18} />

                    Receptury

                </button>

                <button
                    className={
                        activeTab === "users"
                            ? "active"
                            : ""
                    }
                    onClick={() =>
                        setActiveTab("users")
                    }
                >

                    <Users size={18} />

                    Użytkownicy

                </button>

            </div>

            {/* PODIUM */}

            <div className="top-ranking-podium">

                {/* SECOND */}

                {currentData[1] && (

                    <div
                        className="top-card second-place"
                        onClick={() =>
                            handleNavigate(
                                currentData[1].id
                            )
                        }
                    >

                        <div className="top-badge">
                            #2
                        </div>

                        <h2>
                            {currentData[1].name}
                        </h2>

                        <p>
                            {currentData[1].subtitle}
                        </p>

                        {activeTab !== "users" ? (

                            <div className="top-rating">

                                <Star
                                    size={16}
                                    fill="currentColor"
                                />

                                <span>
                                    {currentData[1].rating}
                                </span>

                                <small>
                                    (
                                    {currentData[1].ratingCount}
                                    {" "}
                                    ocen
                                    )
                                </small>

                            </div>

                        ) : (

                            <div className="top-rating">

                                <span>
                                    {currentData[1].score}
                                    {" "}
                                    pkt
                                </span>

                            </div>

                        )}

                    </div>

                )}

                {/* FIRST */}

                {currentData[0] && (

                    <div
                        className="top-card first-place"
                        onClick={() =>
                            handleNavigate(
                                currentData[0].id
                            )
                        }
                    >

                        <div className="top-badge">

                            <Trophy size={16} />

                            #1

                        </div>

                        <h2>
                            {currentData[0].name}
                        </h2>

                        <p>
                            {currentData[0].subtitle}
                        </p>

                        {activeTab !== "users" ? (

                            <div className="top-rating">

                                <Star
                                    size={16}
                                    fill="currentColor"
                                />

                                <span>
                                    {currentData[0].rating}
                                </span>

                                <small>
                                    (
                                    {currentData[0].ratingCount}
                                    {" "}
                                    ocen
                                    )
                                </small>

                            </div>

                        ) : (

                            <div className="top-rating">

                                <span>
                                    {currentData[0].score}
                                    {" "}
                                    pkt
                                </span>

                            </div>

                        )}

                    </div>

                )}

                {/* THIRD */}

                {currentData[2] && (

                    <div
                        className="top-card third-place"
                        onClick={() =>
                            handleNavigate(
                                currentData[2].id
                            )
                        }
                    >

                        <div className="top-badge">
                            #3
                        </div>

                        <h2>
                            {currentData[2].name}
                        </h2>

                        <p>
                            {currentData[2].subtitle}
                        </p>

                        {activeTab !== "users" ? (

                            <div className="top-rating">

                                <Star
                                    size={16}
                                    fill="currentColor"
                                />

                                <span>
                                    {currentData[2].rating}
                                </span>

                                <small>
                                    (
                                    {currentData[2].ratingCount}
                                    {" "}
                                    ocen
                                    )
                                </small>

                            </div>

                        ) : (

                            <div className="top-rating">

                                <span>
                                    {currentData[2].score}
                                    {" "}
                                    pkt
                                </span>

                            </div>

                        )}

                    </div>

                )}

            </div>

            {/* LEADERBOARD */}

            <div className="leaderboard">

                <div className="leaderboard-header">

                    <h3>
                        Pełny ranking
                    </h3>

                </div>

                <div className="leaderboard-list">

                    {currentData.map((item, index) => (

                        <div
                            className="leaderboard-item"
                            key={item.id}
                            onClick={() =>
                                handleNavigate(item.id)
                            }
                        >

                            <div className="leaderboard-position">

                                #{index + 1}

                            </div>

                            <div className="leaderboard-content">

                                <h4>
                                    {item.name}
                                </h4>

                                <p>
                                    {item.subtitle}
                                </p>

                            </div>

                            <div className="leaderboard-score">

                                {activeTab !== "users" ? (

                                    <>

                                        <Star
                                            size={16}
                                            fill="currentColor"
                                        />

                                        {item.rating}

                                    </>

                                ) : (

                                    <>
                                        {item.score} pkt
                                    </>

                                )}

                            </div>

                        </div>

                    ))}

                </div>

            </div>

        </div>

    );
}

export default Ranking;

