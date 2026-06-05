import { useNavigate } from "react-router-dom";
import {
    Heart,
    Plus,
    BookOpen,
    ChevronRight
} from "lucide-react";

import "../../styles/recipe/recipeLayout.css";
import "../../styles/recipe/Recipes.css";

const Recipes = () => {
    const navigate = useNavigate();

    const cards = [
        {
            title: "Wszystkie receptury",
            description: "Przeglądaj wszystkie dostępne receptury.",
            icon: <BookOpen size={22} />,
            path: "/recipes/all"
        },
        {
            title: "Nowa receptura",
            description: "Stwórz własny przepis parzenia kawy.",
            icon: <Plus size={22} />,
            path: "/recipes/new"
        },
        {
            title: "Polubione receptury",
            description: "Zapisane i najczęściej używane przepisy.",
            icon: <Heart size={22} />,
            path: "/recipes/favorites"
        },
        {
            title: "Twoje receptury",
            description: "Wszystkie stworzone przez Ciebie receptury.",
            icon: <BookOpen size={22} />,
            path: "/recipes/my"
        }
    ];

    return (
        <div className="recipe-page">
            <div className="recipe-page__header">
                <h1 className="recipe-page__title">Receptury</h1>
                <p className="recipe-page__subtitle">
                    Twórz, zapisuj i organizuj swoje przepisy parzenia.
                </p>
            </div>

            <div className="recipe-hub__cards">
                {cards.map((card) => (
                    <div
                        key={card.title}
                        className="recipe-hub__card"
                        onClick={() => navigate(card.path)}
                        role="button"
                        tabIndex={0}
                        onKeyDown={(event) => {
                            if (event.key === "Enter" || event.key === " ") {
                                navigate(card.path);
                            }
                        }}
                    >
                        <div className="recipe-hub__card-body">
                            <div className="recipe-card__icon">
                                {card.icon}
                            </div>

                            <div>
                                <h2 className="recipe-card__title">
                                    {card.title}
                                </h2>
                                <p className="recipe-card__text">
                                    {card.description}
                                </p>
                            </div>
                        </div>

                        <ChevronRight size={26} color="#666" />
                    </div>
                ))}
            </div>
        </div>
    );
};

export default Recipes;
