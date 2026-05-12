import { useNavigate } from "react-router-dom";

import "../../styles/wiki/WikiHome.css";

import {
    Coffee,
    Globe,
    FlaskConical,
    Search,
    Plus,
    Flame
} from "lucide-react";

function WikiHome() {

    const navigate = useNavigate();

    const categories = [

        {
            title: "Kawy",
            description:
                "Poznaj odmiany kaw, profile smakowe i pochodzenie.",
            icon: <Coffee size={34} />,
            route: "/wiki/coffees"
        },

        {
            title: "Regiony",
            description:
                "Odkrywaj regiony upraw kawy z całego świata.",
            icon: <Globe size={34} />,
            route: "/wiki/regions"
        },

        {
            title: "Metody Parzenia",
            description:
                "Poznaj techniki parzenia i przepisy.",
            icon: <FlaskConical size={34} />,
            route: "/wiki/methods"
        },

        {
            title: "Palarnie",
            description:
                "Poznaj najlepsze palarnie specialty coffee.",
            icon: <Flame size={34} />,
            route: "/wiki/roasteries"
        }
    ];

    return (

        <div className="wiki-page">

            <div className="wiki-header">

                <div className="wiki-header-top">

                    <div>

                        <h1>
                            Encyklopedia Kawy
                        </h1>

                        <p>
                            Poznaj kawy, regiony
                            i wiedzę o metodach parzenia.
                        </p>

                    </div>

                    <button
                        className="add-article-button"
                        onClick={() => navigate("/wiki/add")}
                    >

                        <Plus size={18} />

                        Nowy artykuł

                    </button>

                </div>

                <div className="wiki-search-container">

                    <Search size={18} />

                    <input
                        type="text"
                        placeholder="Szukaj w encyklopedii..."
                        className="wiki-search"
                    />

                </div>

            </div>

            <div className="wiki-categories">

                {categories.map((category) => (

                    <div
                        key={category.title}
                        className="wiki-card"
                        onClick={() =>
                            navigate(category.route)
                        }
                    >

                        <div className="wiki-card-icon">

                            {category.icon}

                        </div>

                        <h2>
                            {category.title}
                        </h2>

                        <p>
                            {category.description}
                        </p>

                    </div>

                ))}

            </div>

        </div>
    );
}

export default WikiHome;