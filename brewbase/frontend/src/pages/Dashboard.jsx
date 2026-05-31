import "../styles/Dashboard.css";

import { useState, useEffect } from "react";

import {
    useNavigate
} from "react-router-dom";

import GlobalSearch from "../components/GlobalSearch";
import { getQuickNotes } from "../api/quickNotesApi";

import {
    Bell,
    MessageCircle,
    Coffee,
    NotebookPen,
    Star,
    FileText,
    Bookmark,
    Users,
} from "lucide-react";

function Dashboard() {

    const navigate = useNavigate();

    const [showNotifications, setShowNotifications] =
        useState(false);

    const [quickNotesCount, setQuickNotesCount] = useState(null);

    useEffect(() => {
        const loadQuickNotesCount = async () => {
            try {
                const notes = await getQuickNotes();
                setQuickNotesCount(notes.length);
            } catch {
                setQuickNotesCount(0);
            }
        };

        loadQuickNotesCount();
    }, []);

    const recommendedCoffees = [
        {
            name: "Etiopia Gedeb",
            notes: "Jaśmin, bergamotka, miód",
            rating: 4.8,
        },
        {
            name: "Kenya AA",
            notes: "Porzeczka, cytrusy",
            rating: 4.7,
        },
        {
            name: "Pink Bourbon",
            notes: "Kwiaty, karmel",
            rating: 4.6,
        },
        {
            name: "Brazil Fazenda",
            notes: "Orzechy, czekolada",
            rating: 4.5,
        },
    ];

    const recommendedRecipes = [
        {
            name: "V60 – Jasne palenie",
            author: "CoffeeLover",
            rating: 4.8,
        },
        {
            name: "Chemex Classic",
            author: "AsiaBeans",
            rating: 4.7,
        },
        {
            name: "Espresso Balance",
            author: "BaristaTom",
            rating: 4.6,
        },
        {
            name: "Origami Bloom",
            author: "BrewMaster",
            rating: 4.7,
        },
    ];

    return (

        <div className="dashboard">

            <div className="dashboard-top">

                <div>

                    <h1>Dzień dobry, Maria!</h1>

                    <p>
                        Oto co dzieje się w Twoim kawowym świecie.
                    </p>

                </div>

                <div className="top-actions">

                    <GlobalSearch />

                    <div className="top-icons">

                        <div
                            className="notification-wrapper"
                            onClick={() =>
                                setShowNotifications(
                                    !showNotifications
                                )
                            }
                        >

                            <Bell size={20} />

                            {showNotifications && (

                                <div className="notifications-dropdown">

                                    <div className="notification-item">
                                        CoffeeLover dodał nową recepturę
                                    </div>

                                    <div className="notification-item">
                                        AsiaBeans polubiła Twój cupping
                                    </div>

                                    <div className="notification-item">
                                        Dodano nowy artykuł wiki
                                    </div>

                                    <div className="notification-item">
                                        Twoja receptura została dodana do ulubionych
                                    </div>

                                </div>

                            )}

                        </div>
                        

                    </div>

                </div>

            </div>

            <div className="dashboard-layout">

                {/* LEFT SIDE */}

                <div className="dashboard-main">

                    {/* STATS */}

                    <div className="dashboard-card">

                        <div className="card-header">

                            <h3>Twoje statystyki</h3>

                            <select>
                                <option>Ostatnie 30 dni</option>
                                <option>Ostatnie 7 dni</option>
                                <option>Ostatnie 90 dni</option>
                            </select>

                        </div>

                        <div className="stats-grid">

                            <div className="stat-item">

                                <Coffee size={30} />

                                <h2>12</h2>

                                <p>Dodane receptury</p>

                            </div>

                            <div className="stat-item">

                                <NotebookPen size={30} />

                                <h2>7</h2>

                                <p>Cuppingi</p>

                            </div>

                            <div className="stat-item">

                                <FileText size={30} />

                                <h2>{quickNotesCount ?? "—"}</h2>

                                <p>Szybkie notatki</p>

                            </div>

                            <div className="stat-item">

                                <Star size={30} />

                                <h2>4.6</h2>

                                <p>Średnia ocen</p>

                            </div>

                            <div className="stat-item">

                                <Coffee size={30} />

                                <h2>V60</h2>

                                <p>Najczęściej używana metoda</p>

                                <div className="mini-progress">

                                    <div className="mini-progress-fill"></div>

                                </div>

                            </div>

                        </div>

                    </div>

                    {/* RECOMMENDED COFFEES */}

                    <div className="dashboard-card">

                        <div className="card-header">

                            <h3>Rekomendowane kawy</h3>

                            <span
                                onClick={() =>
                                    navigate("/wiki/coffees")
                                }
                            >
                                Zobacz wszystkie kawy
                            </span>

                        </div>

                        <div className="coffee-grid">

                            {recommendedCoffees.map((coffee, index) => (

                                <div
                                    className="coffee-card"
                                    key={index}
                                >

                                    <div className="coffee-image"></div>

                                    <h4>{coffee.name}</h4>

                                    <p>{coffee.notes}</p>

                                    <div className="rating">

                                        <Star size={14} />

                                        {coffee.rating}

                                    </div>

                                </div>

                            ))}

                        </div>

                    </div>

                    {/* RECOMMENDED RECIPES */}

                    <div className="dashboard-card">

                        <div className="card-header">

                            <h3>Rekomendowane receptury</h3>

                            <span
                                onClick={() =>
                                    navigate("/recipes/all")
                                }
                            >
                                Zobacz wszystkie receptury
                            </span>

                        </div>

                        <div className="coffee-grid">

                            {recommendedRecipes.map((recipe, index) => (

                                <div
                                    className="coffee-card"
                                    key={index}
                                >

                                    <div className="coffee-image"></div>

                                    <h4>{recipe.name}</h4>

                                    <p>by {recipe.author}</p>

                                    <div className="rating">

                                        <Star size={14} />

                                        {recipe.rating}

                                    </div>

                                </div>

                            ))}

                        </div>

                    </div>

                    {/* ACTIVITY */}

                    <div className="dashboard-card">

                        <div className="card-header">

                            <h3>Obserwujesz</h3>

                        </div>

                        <div className="activity-list">

                            <div className="activity-item">

                                <Users size={18} />

                                <p>
                                    <strong>CoffeeLover</strong>
                                    {" "}
                                    dodał nową recepturę
                                </p>

                            </div>

                            <div className="activity-item">

                                <Star size={18} />

                                <p>
                                    <strong>AsiaBeans</strong>
                                    {" "}
                                    oceniła kawę Ethiopia Gedeb
                                </p>

                            </div>

                            <div className="activity-item">

                                <Coffee size={18} />

                                <p>
                                    <strong>BaristaTom</strong>
                                    {" "}
                                    opublikował nowy cupping
                                </p>

                            </div>

                        </div>

                    </div>

                    {/* QUICK ACTIONS */}

                    <div className="quick-actions">

                        <button
                            onClick={() =>
                                navigate("/recipes/new")
                            }
                        >
                            Dodaj recepturę
                        </button>

                        <button
                            onClick={() =>
                                navigate("/cupping/new")
                            }
                        >
                            Dodaj cupping
                        </button>

                        <button
                            onClick={() =>
                                navigate("/quicknotes")
                            }
                        >
                            Szybka notatka
                        </button>

                        <button
                            onClick={() =>
                                navigate("/wiki/add")
                            }
                        >
                            Dodaj treść wiki
                        </button>

                    </div>

                </div>

                {/* RIGHT SIDE */}

                <div className="dashboard-sidebar">

                    {/* FAVORITE COFFEES */}

                    <div className="dashboard-card">

                        <div className="card-header">

                            <h3>Ulubione kawy</h3>

                            <span
                                onClick={() =>
                                    navigate("/favorite-coffees")
                                }
                            >
                                Zobacz wszystkie
                            </span>

                        </div>

                        <div className="favorites-list">

                            <div className="favorite-item">

                                <Bookmark size={18} />

                                <div>

                                    <h4>Etiopia Gedeb</h4>

                                    <p>Jaśmin, bergamotka</p>

                                </div>

                                <span>4.8</span>

                            </div>

                            <div className="favorite-item">

                                <Bookmark size={18} />

                                <div>

                                    <h4>Kenya AA</h4>

                                    <p>Porzeczka, cytrusy</p>

                                </div>

                                <span>4.7</span>

                            </div>

                        </div>

                    </div>

                    {/* FAVORITE RECIPES */}

                    <div className="dashboard-card">

                        <div className="card-header">

                            <h3>Ulubione receptury</h3>

                            <span
                                onClick={() =>
                                    navigate("/recipes/favorites")
                                }
                            >
                                Zobacz wszystkie
                            </span>

                        </div>

                        <div className="favorites-list">

                            <div className="favorite-item">

                                <Bookmark size={18} />

                                <div>

                                    <h4>V60 – Jasne palenie</h4>

                                    <p>by CoffeeLover</p>

                                </div>

                                <span>4.8</span>

                            </div>

                            <div className="favorite-item">

                                <Bookmark size={18} />

                                <div>

                                    <h4>Espresso Balance</h4>

                                    <p>by BaristaTom</p>

                                </div>

                                <span>4.6</span>

                            </div>

                        </div>

                    </div>

                </div>

            </div>

        </div>

    );
}

export default Dashboard;