import { useEffect, useState } from "react";

import { useNavigate } from "react-router-dom";

import {
    Heart,
    Clock3,
    ChevronRight,
    FileText,
    Globe,
    Lock,
    Trash2
} from "lucide-react";

import {
    getRecipes,
    getMyRecipes,
    getFavoriteRecipes,
    deleteRecipe,
    addFavorite,
    removeFavorite
} from "../../api/recipeApi";
import { getRecipeModerationComment } from "../../utils/recipeModeration";

import "../../styles/recipe/recipeLayout.css";
import "../../styles/recipe/RecipesList.css";

const formatDate = (date) => {

    if (!date) {
        return "Brak daty";
    }

    const parsed = new Date(date);

    if (Number.isNaN(parsed.getTime())) {
        return "Brak daty";
    }

    return parsed.toLocaleDateString("pl-PL");
};

const RecipesList = ({ title }) => {

    const navigate = useNavigate();

    const [recipes, setRecipes] = useState([]);
    const [page, setPage] = useState(1);
    const pageSize = 10;

    useEffect(() => {
        setPage(1);
    }, [title]);
    useEffect(() => {

        const fetchRecipes = async () => {

            try {
                const data = title === "Ulubione receptury"
                    ? await getFavoriteRecipes()
                    : title === "Twoje receptury"
                        ? await getMyRecipes({ page, pageSize })
                        : await getRecipes({ page, pageSize, sortBy: "title", sortOrder: "asc" });

                setRecipes(data);

            } catch (error) {

                console.error(error);
            }
        };

        fetchRecipes();

    }, [title, page]);

    let data = [];

    if (title === "Ulubione receptury") {

        data = recipes;

    } else if (title === "Wszystkie receptury") {

        data = recipes.filter(
            (r) => r.isPublic
        );

    } else {

        data = recipes;
    }

    const handleFavorite = async (recipeId, event) => {
        event.stopPropagation();

        const recipe = recipes.find((item) => item.id === recipeId);
        if (!recipe) {
            return;
        }

        const wasFavorite = recipe.isFavorite ?? false;

        setRecipes((previous) =>
            previous.map((item) =>
                item.id === recipeId
                    ? { ...item, isFavorite: !wasFavorite }
                    : item
            )
        );

        try {
            if (wasFavorite) {
                await removeFavorite(recipeId);
            } else {
                await addFavorite(recipeId);
            }
        } catch (error) {
            setRecipes((previous) =>
                previous.map((item) =>
                    item.id === recipeId
                        ? { ...item, isFavorite: wasFavorite }
                        : item
                )
            );

            console.error(error);
            alert("Nie udało się zaktualizować ulubionych.");
        }
    };

    const handleDelete = async (id) => {

        const confirmed = window.confirm(
            "Czy na pewno chcesz usunąć tę recepturę?"
        );

        if (!confirmed) return;

        try {

            await deleteRecipe(id);

            setRecipes((prev) =>
                prev.filter((r) => r.id !== id)
            );

        } catch (error) {

            console.error(error);

            alert("Nie udało się usunąć receptury.");
        }
    };

    return (
        <div className="recipe-page">
            <div className="recipe-page__header">
                <h1 className="recipe-page__title">{title}</h1>
                <p className="recipe-page__subtitle">
                    {title === "Ulubione receptury"
                        ? "Twoje zapisane i ulubione przepisy."
                        : title === "Wszystkie receptury"
                            ? "Przeglądaj wszystkie publiczne receptury."
                            : "Wszystkie stworzone przez Ciebie receptury."}
                </p>
            </div>

            {data.length === 0 && (
                <div className="recipe-empty">Brak receptur.</div>
            )}

            <div className="recipe-list__items">
                {data.map((r) => (
                    <div key={r.id} className="recipe-card recipe-list__item">
                        <div className="recipe-card__row">
                            <div className="recipe-card__icon">
                                {title === "Ulubione receptury" ? (
                                    <Heart size={22} />
                                ) : (
                                    <FileText size={22} />
                                )}
                            </div>

                            <div>
                                <div className="recipe-list__title-row">
                                    <h2 className="recipe-card__title recipe-card__title--inline">
                                        {r.title}
                                    </h2>

                                    <div className="recipe-badge">
                                        {r.isPublic ? (
                                            <>
                                                <Globe size={12} />
                                                Publiczna
                                            </>
                                        ) : (
                                            <>
                                                <Lock size={12} />
                                                Robocza
                                            </>
                                        )}
                                    </div>
                                </div>

                                <div className="recipe-card__meta">
                                    <Clock3 size={14} />
                                    <span>{formatDate(r.createdAt)}</span>
                                </div>

                                {title === "Twoje receptury" &&
                                    !r.isPublic &&
                                    getRecipeModerationComment(r) && (
                                        <p className="recipe-moderation-note">
                                            Komentarz moderatora:{" "}
                                            {getRecipeModerationComment(r)}
                                        </p>
                                    )}
                            </div>
                        </div>

                        <div className="recipe-list__actions">
                            <button
                                type="button"
                                className="recipe-list__btn recipe-list__btn--icon"
                                onClick={(event) =>
                                    handleFavorite(r.id, event)
                                }
                                aria-label={
                                    r.isFavorite
                                        ? "Usuń z ulubionych"
                                        : "Dodaj do ulubionych"
                                }
                            >
                                <Heart
                                    size={18}
                                    fill={
                                        r.isFavorite
                                            ? "#1f1f1f"
                                            : "none"
                                    }
                                />
                            </button>

                            <button
                                type="button"
                                className="recipe-list__btn recipe-list__btn--primary"
                                onClick={() =>
                                    navigate(`/recipes/${r.id}`)
                                }
                            >
                                Szczegóły
                                <ChevronRight size={18} />
                            </button>

                            {title !== "Ulubione receptury" &&
                                title !== "Wszystkie receptury" && (
                                    <button
                                        type="button"
                                        className="recipe-list__btn recipe-list__btn--danger"
                                        onClick={() =>
                                            handleDelete(r.id)
                                        }
                                    >
                                        <Trash2 size={16} />
                                        Usuń
                                    </button>
                                )}
                        </div>
                    </div>
                ))}
            </div>

            {title !== "Ulubione receptury" && (
                <div className="recipe-list__pagination">
                    <button
                        type="button"
                        className="recipe-list__btn"
                        onClick={() =>
                            setPage((currentPage) =>
                                Math.max(1, currentPage - 1)
                            )
                        }
                        disabled={page === 1}
                    >
                        Poprzednia
                    </button>

                    <span className="recipe-list__page-label">
                        Strona {page}
                    </span>

                    <button
                        type="button"
                        className="recipe-list__btn"
                        onClick={() =>
                            setPage((currentPage) => currentPage + 1)
                        }
                        disabled={recipes.length < pageSize}
                    >
                        Następna
                    </button>
                </div>
            )}
        </div>
    );
};

export default RecipesList;
