import { useEffect, useState } from "react";

import { useLocation, useNavigate } from "react-router-dom";

import "../../styles/wiki/WikiHome.css";
import "../../styles/wiki/MyWikiArticles.css";

import {
    Coffee,
    Globe,
    FlaskConical,
    Plus,
    Flame,
    FileText,
    CheckCircle,
    X
} from "lucide-react";

function WikiHome() {

    const navigate = useNavigate();
    const location = useLocation();

    const [showSubmitSuccess, setShowSubmitSuccess] = useState(false);

    useEffect(() => {
        if (location.state?.articleSubmitted) {
            setShowSubmitSuccess(true);
            navigate(location.pathname, { replace: true, state: {} });
        }
    }, [location, navigate]);

    const categories = [

        {
            title: "Kawy",
            description:
                "Poznaj odmiany kaw, profile smakowe i pochodzenie.",
            icon: <Coffee size={34} />,
            route: "/wiki/coffees"
        },

        {
            title: "Kraje",
            description:
                "Poznaj kraje pochodzenia kaw specialty.",
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
                            Poznaj kawy, kraje
                            i wiedzę o metodach parzenia.
                        </p>

                    </div>

                    <div className="wiki-header-actions">

                        <button
                            className="my-articles-button"
                            onClick={() => navigate("/wiki/my-articles")}
                        >

                            <FileText size={18} />

                            Moje artykuły

                        </button>

                        <button
                            className="add-article-button"
                            onClick={() => navigate("/wiki/add")}
                        >

                            <Plus size={18} />

                            Nowy artykuł

                        </button>

                    </div>

                </div>

                {showSubmitSuccess && (

                    <div className="wiki-submit-success">

                        <CheckCircle size={22} />

                        <p>
                            Artykuł został wysłany do moderacji.
                            Po akceptacji pojawi się w encyklopedii.
                            Status możesz sprawdzić w sekcji
                            „Moje artykuły”.
                        </p>

                        <button
                            type="button"
                            className="wiki-submit-success-close"
                            aria-label="Zamknij komunikat"
                            onClick={() => setShowSubmitSuccess(false)}
                        >
                            <X size={18} />
                        </button>

                    </div>

                )}

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