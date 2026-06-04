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

                    {title === "Ulubione receptury"
                        ? "Twoje zapisane i ulubione przepisy."

                        : title === "Wszystkie receptury"
                            ? "Przeglądaj wszystkie publiczne receptury."

                            : "Wszystkie stworzone przez Ciebie receptury."}

                </p>

            </div>

            {/* EMPTY STATE */}

            {data.length === 0 && (

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

                                {title === "Ulubione receptury" ? (

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

                                        {r.isPublic ?  (
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

                                        {formatDate(r.createdAt)}

                                    </span>

                                </div>

                                {title === "Twoje receptury" &&
                                    !r.isPublic &&
                                    getRecipeModerationComment(r) && (
                                        <p
                                            style={{
                                                marginTop: "10px",
                                                fontSize: "14px",
                                                color: "#555555",
                                            }}
                                        >
                                            Komentarz moderatora:{" "}
                                            {getRecipeModerationComment(r)}
                                        </p>
                                    )}

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
                                style={favoriteButtonStyle}
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
                                style={detailsButtonStyle}
                                onClick={() =>
                                    navigate(`/recipes/${r.id}`)
                                }
                            >

                                Szczegóły

                                <ChevronRight size={18} />

                            </button>

                            {title !==
                                "Ulubione receptury" &&

                                title !==
                                "Wszystkie receptury" && (

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
            {title !== "Ulubione receptury" && (
                <div
                    style={{
                        display: "flex",
                        gap: "12px",
                        alignItems: "center",
                        marginTop: "26px",
                        maxWidth: "950px"
                    }}
                >
                    <button
                        style={paginationButtonStyle}
                        onClick={() => setPage((currentPage) => Math.max(1, currentPage - 1))}
                        disabled={page === 1}
                    >
                        Poprzednia
                    </button>

                    <span
                        style={{
                            color: "#555",
                            fontSize: "14px",
                            fontWeight: "600"
                        }}
                    >
            Strona {page}
        </span>

                    <button
                        style={paginationButtonStyle}
                        onClick={() => setPage((currentPage) => currentPage + 1)}
                        disabled={recipes.length < pageSize}
                    >
                        Następna
                    </button>
                </div>
            )}
        </div>
    );
};

const paginationButtonStyle = {
    backgroundColor: "#efefef",
    color: "#2f2f2f",
    padding: "12px 18px",
    borderRadius: "18px",
    border: "1px solid #dddddd",
    cursor: "pointer",
    fontSize: "14px",
    fontWeight: "600"
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

const favoriteButtonStyle = {
    backgroundColor: "#efefef",
    color: "#2f2f2f",
    padding: "12px 14px",
    borderRadius: "18px",
    border: "1px solid #dddddd",
    cursor: "pointer",
    display: "flex",
    alignItems: "center",
    justifyContent: "center"
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
