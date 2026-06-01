
import { useEffect, useState } from "react";

import {
    useNavigate,
    useParams
} from "react-router-dom";

import {
    Globe,
    Lock,
    Coffee,
    Thermometer,
    Droplets,
    Scale,
    Timer,
    FileText,
    Download,
    Heart,
    Star,
    Flag
} from "lucide-react";

import {
    getRecipeById,
    addFavorite,
    removeFavorite,
    rateRecipe
} from "../api/recipeApi";
import { getProfile } from "../api/profileApi";
import { getRecipeModerationComment } from "../utils/recipeModeration";

const RecipeDetails = () => {
    

    const { id } = useParams();
    const navigate = useNavigate();

    const [recipe, setRecipe] = useState(null);

    const [loading, setLoading] = useState(true);

    const [userRating, setUserRating] = useState(0);

    const [isFavorite, setIsFavorite] = useState(false);

    const [currentUserId, setCurrentUserId] = useState(null);

    useEffect(() => {

        const fetchRecipe = async () => {

            try {

                const [data, profile] = await Promise.all([
                    getRecipeById(id),
                    getProfile().catch(() => null),
                ]);

                const parsedRecipe = {
                    ...data,
                    parameters:
                        typeof data.parameters === "string"
                            ? JSON.parse(data.parameters)
                            : data.parameters
                };

                setRecipe(parsedRecipe);

                setIsFavorite(data.isFavorite || false);

                const profileUserId = profile?.userId ?? profile?.UserId;
                setCurrentUserId(
                    profileUserId != null ? Number(profileUserId) : null
                );

            } catch (error) {

                console.error(error);

            } finally {

                setLoading(false);
            }
        };

        fetchRecipe();

    }, [id]);

    const handleFavorite = async () => {

        const wasFavorite = isFavorite;

        setIsFavorite(!wasFavorite);

        try {

            if (wasFavorite) {

                await removeFavorite(recipe.id);

            } else {

                await addFavorite(recipe.id);
            }

        } catch (error) {

            setIsFavorite(wasFavorite);

            console.error(error);

            alert(
                "Nie udało się zaktualizować ulubionych."
            );
        }
    };

    const handleRating = async (value) => {

        try {

            await rateRecipe(recipe.id, value);

            setUserRating(value);

            const updated = await getRecipeById(id);
            setRecipe((previous) => ({
                ...previous,
                averageRating: updated.averageRating,
                ratingCount: updated.ratingCount ?? 0,
            }));

        } catch (error) {

            console.error(error);

            const message =
                error?.status === 403
                    ? "Nie możesz oceniać własnej receptury."
                    : error?.status === 401
                      ? "Zaloguj się, aby wystawić ocenę."
                      : error?.message &&
                          typeof error.message === "string" &&
                          error.message.trim().length > 0
                        ? error.message
                        : "Nie udało się zapisać oceny.";

            alert(message);
        }
    };

    if (loading) {

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

                <div
                    style={{
                        backgroundColor: "#fafafa",
                        borderRadius: "28px",
                        border: "1px solid #e6e6e6",
                        padding: "40px",
                        maxWidth: "900px",
                        color: "#2f2f2f",
                        fontSize: "20px",
                        fontWeight: "600"
                    }}
                >
                    Ładowanie receptury...
                </div>

            </div>
        );
    }

    if (!recipe) {

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

                <div
                    style={{
                        backgroundColor: "#fafafa",
                        borderRadius: "28px",
                        border: "1px solid #e6e6e6",
                        padding: "40px",
                        maxWidth: "900px",
                        color: "#2f2f2f",
                        fontSize: "20px",
                        fontWeight: "600"
                    }}
                >
                    Nie znaleziono receptury.
                </div>

            </div>
        );
    }

    const exportToTXT = () => {

        const content = `
${recipe.title}

Metoda: ${recipe.brewingMethod}
Status: ${recipe.isPublic ? "Publiczna" : "Robocza"}

PARAMETRY
- Kawa: ${recipe.parameters.coffee}
- Woda: ${recipe.parameters.water}
- Temperatura: ${recipe.parameters.temperature}
- Czas: ${recipe.parameters.brewTime}
- Mielenie: ${recipe.parameters.grindSize}

OPIS
${recipe.steps}
`;

        const blob = new Blob([content], {
            type: "text/plain"
        });

        const url = URL.createObjectURL(blob);

        const a = document.createElement("a");

        a.href = url;

        a.download = `${recipe.title}.txt`;

        a.click();

        URL.revokeObjectURL(url);
    };

    const exportToCSV = () => {

        const csvContent = `
Title,Brewing Method,Status,Coffee,Water,Temperature,Brew Time,Grind Size,Steps
"${recipe.title}","${recipe.brewingMethod}","${recipe.isPublic ? "Publiczna" : "Robocza"}","${recipe.parameters.coffee}","${recipe.parameters.water}","${recipe.parameters.temperature}","${recipe.parameters.brewTime}","${recipe.parameters.grindSize}","${recipe.steps.replace(/\n/g, " ")}"
    `;

        const blob = new Blob([csvContent], {
            type: "text/csv;charset=utf-8;"
        });

        const url = URL.createObjectURL(blob);

        const a = document.createElement("a");

        a.href = url;

        a.download = `${recipe.title}.csv`;

        a.click();

        URL.revokeObjectURL(url);
    };

    const recipeOwnerId = Number(recipe.userId ?? recipe.UserId);
    const isOwner =
        currentUserId != null &&
        Number.isFinite(recipeOwnerId) &&
        currentUserId === recipeOwnerId;

    return (

        <div
            style={{
                width: "100%",
                minHeight: "100vh",
                backgroundColor: "#f3f3f3",
                padding: "55px 60px",
                boxSizing: "border-box",
                display: "flex",
                justifyContent: "center"
            }}
        >

            <div style={{ width: "100%", maxWidth: "950px" }}>

                {/* HEADER */}

                <div style={{ marginBottom: "38px" }}>

                    <h1
                        style={{
                            fontSize: "58px",
                            fontWeight: "700",
                            color: "#1f1f1f",
                            marginBottom: "14px",
                            lineHeight: "1"
                        }}
                    >
                        {recipe.title}
                    </h1>

                    <div
                        style={{
                            display: "flex",
                            alignItems: "center",
                            justifyContent: "space-between",
                            gap: "20px",
                            flexWrap: "wrap"
                        }}
                    >

                        <div>

                            <div
                                style={{
                                    display: "flex",
                                    alignItems: "center",
                                    gap: "12px",
                                    flexWrap: "wrap"
                                }}
                            >

                                <div style={badgeStyle}>
                                    <Coffee size={14} />
                                    {recipe.brewingMethod}
                                </div>

                                <div style={badgeStyle}>

                                    {recipe.isPublic ? (

                                        <>
                                            <Globe size={14} />
                                            Publiczna
                                        </>

                                    ) : (

                                        <>
                                            <Lock size={14} />
                                            Wersja robocza
                                        </>

                                    )}

                                </div>

                            </div>

                            {isOwner &&
                                !recipe.isPublic &&
                                getRecipeModerationComment(recipe) && (
                                    <p
                                        style={{
                                            marginTop: "16px",
                                            fontSize: "15px",
                                            color: "#555555",
                                            maxWidth: "720px",
                                        }}
                                    >
                                        Komentarz moderatora:{" "}
                                        {getRecipeModerationComment(recipe)}
                                    </p>
                                )}

                            {/* FAVORITES + RATING */}

                            <div
                                style={{
                                    display: "flex",
                                    alignItems: "center",
                                    gap: "20px",
                                    marginTop: "18px",
                                    flexWrap: "wrap"
                                }}
                            >

                                {/* FAVORITE */}

                                <button
                                    onClick={handleFavorite}
                                    style={{
                                        background: "none",
                                        border: "none",
                                        cursor: "pointer",
                                        display: "flex",
                                        alignItems: "center",
                                        gap: "10px",
                                        fontSize: "15px",
                                        fontWeight: "600",
                                        color: "#2f2f2f",
                                        padding: 0
                                    }}
                                >

                                    <Heart
                                        size={20}
                                        fill={
                                            isFavorite
                                                ? "#1f1f1f"
                                                : "none"
                                        }
                                    />

                                    {isFavorite
                                        ? "Dodano do ulubionych"
                                        : "Dodaj do ulubionych"}

                                </button>

                                {/* RATING */}

                                {isOwner ? (

                                    <div
                                        style={{
                                            display: "flex",
                                            alignItems: "center",
                                            gap: "10px",
                                            flexWrap: "wrap"
                                        }}
                                    >
                                        <span
                                            style={{
                                                fontSize: "14px",
                                                color: "#666"
                                            }}
                                        >
                                            Nie możesz oceniać własnej receptury
                                        </span>

                                        <span
                                            style={{
                                                fontSize: "14px",
                                                color: "#666"
                                            }}
                                        >
                                            {recipe.averageRating
                                                ? recipe.averageRating.toFixed(1)
                                                : "Brak ocen"}

                                            {" · "}

                                            {recipe.ratingCount || 0} ocen
                                        </span>
                                    </div>

                                ) : (

                                    <div
                                        style={{
                                            display: "flex",
                                            alignItems: "center",
                                            gap: "10px"
                                        }}
                                    >

                                        <div
                                            style={{
                                                display: "flex",
                                                gap: "4px"
                                            }}
                                        >

                                            {[1, 2, 3, 4, 5].map((star) => (

                                                <Star
                                                    key={star}
                                                    size={20}
                                                    style={{
                                                        cursor: "pointer"
                                                    }}
                                                    fill={
                                                        star <= userRating
                                                            ? "#1f1f1f"
                                                            : "none"
                                                    }
                                                    onClick={() =>
                                                        handleRating(star)
                                                    }
                                                />

                                            ))}

                                        </div>

                                        <span
                                            style={{
                                                fontSize: "14px",
                                                color: "#666"
                                            }}
                                        >

                                            {recipe.averageRating
                                                ? recipe.averageRating.toFixed(1)
                                                : "Brak ocen"}

                                            {" · "}

                                            {recipe.ratingCount || 0} ocen

                                        </span>

                                    </div>

                                )}

                            </div>

                        </div>

                        {/* ACTIONS */}

                        <div
                            style={{
                                display: "flex",
                                gap: "10px"
                            }}
                        >

                            {isOwner && (
                                <button
                                    style={editButtonStyle}
                                    onClick={() =>
                                        navigate(`/recipes/edit/${recipe.id}`)
                                    }
                                >
                                    Edytuj
                                </button>
                            )}

                            <button
                                style={exportButtonStyle}
                                onClick={exportToTXT}
                            >
                                <Download size={16} />
                                TXT
                            </button>

                            <button
                                style={exportButtonStyle}
                                onClick={exportToCSV}
                            >
                                <Download size={16} />
                                CSV
                            </button>

                            <button
                                style={reportButtonStyle}
                                onClick={() =>
                                    navigate("/report", {
                                        state: {
                                            contentType: "recipe",
                                            contentId: recipe.id,
                                            contentTitle: recipe.title,
                                            returnPath: `/recipes/${recipe.id}`,
                                        },
                                    })
                                }
                            >
                                <Flag size={16} />
                                Zgłoś treść
                            </button>

                        </div>

                    </div>

                </div>

                {/* MAIN CARD */}

                <div
                    style={{
                        backgroundColor: "#fafafa",
                        borderRadius: "28px",
                        border: "1px solid #e6e6e6",
                        padding: "34px",
                        boxShadow:
                            "0 2px 10px rgba(0,0,0,0.03)"
                    }}
                >

                    {/* PARAMETERS */}

                    <div
                        style={{
                            display: "grid",
                            gridTemplateColumns:
                                "repeat(auto-fit, minmax(220px, 1fr))",
                            gap: "18px",
                            marginBottom: "34px"
                        }}
                    >

                        <div style={infoCardStyle}>
                            <Scale size={20} />

                            <div>
                                <p style={infoLabel}>
                                    Ilość kawy
                                </p>

                                <p style={infoValue}>
                                    {recipe.parameters.coffee}
                                </p>
                            </div>

                        </div>

                        <div style={infoCardStyle}>
                            <Droplets size={20} />

                            <div>
                                <p style={infoLabel}>
                                    Ilość wody
                                </p>

                                <p style={infoValue}>
                                    {recipe.parameters.water}
                                </p>
                            </div>

                        </div>

                        <div style={infoCardStyle}>
                            <Thermometer size={20} />

                            <div>
                                <p style={infoLabel}>
                                    Temperatura
                                </p>

                                <p style={infoValue}>
                                    {recipe.parameters.temperature}
                                </p>
                            </div>

                        </div>

                        <div style={infoCardStyle}>
                            <Timer size={20} />

                            <div>
                                <p style={infoLabel}>
                                    Czas parzenia
                                </p>

                                <p style={infoValue}>
                                    {recipe.parameters.brewTime}
                                </p>
                            </div>

                        </div>

                    </div>

                    {/* DESCRIPTION */}

                    <div style={{ marginBottom: "28px" }}>

                        <p style={sectionTitle}>
                            Opis przygotowania
                        </p>

                        <div style={contentCardStyle}>

                            <div
                                style={{
                                    display: "flex",
                                    alignItems: "flex-start",
                                    gap: "12px"
                                }}
                            >

                                <FileText
                                    size={18}
                                    color="#666"
                                />

                                <div
                                    style={{
                                        lineHeight: "1.8",
                                        color: "#2f2f2f",
                                        whiteSpace: "pre-line"
                                    }}
                                >
                                    {recipe.steps}
                                </div>

                            </div>

                        </div>

                    </div>

                    {/* ADDITIONAL */}

                    <div>

                        <p style={sectionTitle}>
                            Dodatkowe informacje
                        </p>

                        <div style={contentCardStyle}>

                            <div
                                style={{
                                    display: "flex",
                                    flexDirection: "column",
                                    gap: "12px"
                                }}
                            >

                                <div>
                                    <span style={labelStyle}>
                                        Stopień mielenia:
                                    </span>{" "}
                                    {recipe.parameters.grindSize}
                                </div>

                            </div>

                        </div>

                    </div>

                </div>

            </div>

        </div>
    );
};

const sectionTitle = {
    fontSize: "16px",
    fontWeight: "600",
    color: "#4f4f4f",
    marginBottom: "14px"
};

const badgeStyle = {
    display: "flex",
    alignItems: "center",
    gap: "8px",
    backgroundColor: "#ebebeb",
    color: "#2f2f2f",
    padding: "10px 14px",
    borderRadius: "16px",
    fontSize: "14px",
    fontWeight: "600"
};


const editButtonStyle = {
    backgroundColor: "#1f1f1f",
    color: "white",
    padding: "10px 16px",
    borderRadius: "16px",
    border: "1px solid #d9d9d9",
    cursor: "pointer",
    fontSize: "14px",
    fontWeight: "600"
};

const exportButtonStyle = {
    display: "flex",
    alignItems: "center",
    gap: "8px",
    backgroundColor: "white",
    color: "#2f2f2f",
    padding: "10px 16px",
    borderRadius: "16px",
    border: "1px solid #d9d9d9",
    cursor: "pointer",
    fontSize: "14px",
    fontWeight: "600"
};

const reportButtonStyle = {
    display: "flex",
    alignItems: "center",
    gap: "8px",
    backgroundColor: "transparent",
    color: "#6b6b6b",
    padding: "10px 16px",
    borderRadius: "16px",
    border: "1px solid #d4d4d4",
    cursor: "pointer",
    fontSize: "14px",
    fontWeight: "600"
};

const infoCardStyle = {
    backgroundColor: "#f3f3f3",
    borderRadius: "20px",
    padding: "18px",
    display: "flex",
    alignItems: "center",
    gap: "14px",
    color: "#2f2f2f",
    border: "1px solid #e2e2e2"
};

const infoLabel = {
    fontSize: "13px",
    color: "#707070",
    marginBottom: "4px"
};

const infoValue = {
    fontSize: "17px",
    fontWeight: "600",
    color: "#1f1f1f"
};

const contentCardStyle = {
    backgroundColor: "#f3f3f3",
    borderRadius: "22px",
    padding: "24px",
    border: "1px solid #e2e2e2",
    color: "#1f1f1f",
    fontSize: "15px"
};

const labelStyle = {
    fontWeight: "600",
    color: "#4f4f4f"
};

export default RecipeDetails;

