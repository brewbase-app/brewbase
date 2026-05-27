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

import { getFavoriteRecipes } from "../api/recipeApi";

const RecipesList = ({ title }) => {
    const navigate = useNavigate();
    const isFavoritesPage = title === "Ulubione receptury";
    const isAllRecipesPage = title === "Wszystkie receptury";

    const [localRecipes, setLocalRecipes] = useState(
        () => JSON.parse(localStorage.getItem("recipes")) || []
    );
    const [favoriteRecipes, setFavoriteRecipes] = useState([]);
    const [isLoading, setIsLoading] = useState(isFavoritesPage);
    const [error, setError] = useState("");

    useEffect(() => {
        if (!isFavoritesPage) {
            return;
        }

        const loadFavorites = async () => {
            try {
                setIsLoading(true);
                setError("");

                const data = await getFavoriteRecipes();
                setFavoriteRecipes(Array.isArray(data) ? data : []);
            } catch {
                setError("Nie udało się pobrać ulubionych receptur.");
                setFavoriteRecipes([]);
            } finally {
                setIsLoading(false);
            }
        };

        loadFavorites();
    }, [isFavoritesPage]);

    const handleDelete = (id) => {
        const confirmed = window.confirm(
            "Czy na pewno chcesz usunąć tę recepturę?"
        );

        if (!confirmed) return;

        const updatedRecipes = localRecipes.filter(
            (r) => r.id !== id
        );

        setLocalRecipes(updatedRecipes);

        localStorage.setItem(
            "recipes",
            JSON.stringify(updatedRecipes)
        );
    };

    const isRecipePublic = (recipe) => {
        if (typeof recipe.isPublic === "boolean") {
            return recipe.isPublic;
        }

        return recipe.status === "PUBLISHED";
    };

    const getRecipeSubtitle = (recipe) => {
        if (recipe.createdAt) {
            return new Date(recipe.createdAt).toLocaleDateString();
        }

        return [recipe.coffee, recipe.brewingMethod]
            .filter(Boolean)
            .join(" • ");
    };

    const data = isFavoritesPage
        ? favoriteRecipes
        : isAllRecipesPage
            ? localRecipes.filter((r) => isRecipePublic(r))
            : localRecipes;

    return (
        <div
            style={{
                width: "100%",
                minHeight: "100vh",
                backgroundColor: "#f3f3f3",
                padding: "55px 60px",
                boxSizing: "border-box"
            }}
        >
            {/* HEADER */}
            <div style={{ marginBottom: "38px" }}>
                <h1
                    style={{
                        fontSize: "58px",
                        fontWeight: "700",
                        color: "#1f1f1f",
                        marginBottom: "8px",
                        lineHeight: "1"
                    }}
                >
                    {title}
                </h1>

                <p
                    style={{
                        fontSize: "16px",
                        color: "#6f6f6f"
                    }}
                >
                    {isFavoritesPage
                        ? "Twoje zapisane i ulubione przepisy."
                        : isAllRecipesPage
                            ? "Przeglądaj wszystkie publiczne receptury."
                            : "Wszystkie stworzone przez Ciebie receptury."}
                </p>
            </div>

            {isLoading && (
                <div
                    style={{
                        backgroundColor: "#fafafa",
                        borderRadius: "28px",
                        border: "1px solid #e6e6e6",
                        padding: "50px",
                        maxWidth: "950px",
                        color: "#707070",
                        fontSize: "16px"
                    }}
                >
                    Ładowanie...
                </div>
            )}

            {!isLoading && error && (
                <div
                    style={{
                        backgroundColor: "#fafafa",
                        borderRadius: "28px",
                        border: "1px solid #e6e6e6",
                        padding: "50px",
                        maxWidth: "950px",
                        color: "#707070",
                        fontSize: "16px"
                    }}
                >
                    {error}
                </div>
            )}

            {/* EMPTY STATE */}
            {!isLoading && !error && data.length === 0 && (
                <div
                    style={{
                        backgroundColor: "#fafafa",
                        borderRadius: "28px",
                        border: "1px solid #e6e6e6",
                        padding: "50px",
                        maxWidth: "950px",
                        color: "#707070",
                        fontSize: "16px"
                    }}
                >
                    Brak receptur.
                </div>
            )}

            {/* LIST */}
            {!isLoading && !error && data.length > 0 && (
            <div
                style={{
                    display: "flex",
                    flexDirection: "column",
                    gap: "18px",
                    maxWidth: "950px"
                }}
            >
                {data.map((r) => (
                    <div
                        key={r.id}
                        style={{
                            backgroundColor: "#fafafa",
                            borderRadius: "26px",
                            padding: "24px 28px",
                            border: "1px solid #e6e6e6",
                            display: "flex",
                            justifyContent: "space-between",
                            alignItems: "center",
                            boxShadow:
                                "0 2px 10px rgba(0,0,0,0.03)"
                        }}
                    >
                        {/* LEFT */}
                        <div
                            style={{
                                display: "flex",
                                alignItems: "center",
                                gap: "22px"
                            }}
                        >
                            <div
                                style={{
                                    width: "58px",
                                    height: "58px",
                                    borderRadius: "18px",
                                    backgroundColor: "#efefef",
                                    display: "flex",
                                    alignItems: "center",
                                    justifyContent: "center",
                                    color: "#2a2a2a",
                                    flexShrink: 0
                                }}
                            >
                                {isFavoritesPage ? (
                                    <Heart size={22} />
                                ) : (
                                    <FileText size={22} />
                                )}
                            </div>

                            <div>
                                <div
                                    style={{
                                        display: "flex",
                                        alignItems: "center",
                                        gap: "10px",
                                        marginBottom: "8px",
                                        flexWrap: "wrap"
                                    }}
                                >
                                    <h2
                                        style={{
                                            fontSize: "22px",
                                            fontWeight: "700",
                                            color: "#1f1f1f",
                                            margin: 0
                                        }}
                                    >
                                        {r.title}
                                    </h2>

                                    <div
                                        style={{
                                            display: "flex",
                                            alignItems: "center",
                                            gap: "6px",
                                            backgroundColor:
                                                "#ebebeb",
                                            padding: "6px 10px",
                                            borderRadius: "12px",
                                            fontSize: "12px",
                                            color: "#555"
                                        }}
                                    >
                                        {isRecipePublic(r) ? (
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

                                <div
                                    style={{
                                        display: "flex",
                                        alignItems: "center",
                                        gap: "8px",
                                        color: "#707070",
                                        fontSize: "14px"
                                    }}
                                >
                                    <Clock3 size={14} />

                                    <span>
                                        {getRecipeSubtitle(r) || "—"}
                                    </span>
                                </div>
                            </div>
                        </div>

                        {/* RIGHT */}
                        <div
                            style={{
                                display: "flex",
                                gap: "10px",
                                alignItems: "center"
                            }}
                        >
                            <button
                                style={detailsButtonStyle}
                                onClick={() =>
                                    navigate(`/recipes/${r.id}`)
                                }
                            >
                                Szczegóły
                                <ChevronRight size={18} />
                            </button>

                            {!isFavoritesPage && !isAllRecipesPage && (
                                    <button
                                        style={deleteButtonStyle}
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
            )}
        </div>
    );
};

const detailsButtonStyle = {
    backgroundColor: "#1f1f1f",
    color: "white",
    padding: "12px 18px",
    borderRadius: "18px",
    border: "none",
    cursor: "pointer",
    display: "flex",
    alignItems: "center",
    gap: "8px",
    fontSize: "14px",
    fontWeight: "600"
};

const deleteButtonStyle = {
    backgroundColor: "#efefef",
    color: "#555",
    padding: "12px 16px",
    borderRadius: "18px",
    border: "1px solid #dddddd",
    cursor: "pointer",
    display: "flex",
    alignItems: "center",
    gap: "8px",
    fontSize: "14px",
    fontWeight: "600"
};

export default RecipesList;
