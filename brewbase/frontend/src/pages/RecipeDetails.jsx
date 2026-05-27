import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
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
    Heart
} from "lucide-react";

import {
    addRecipeFavorite,
    getRecipeById,
    removeRecipeFavorite
} from "../api/recipeApi";

function parseParameters(parameters) {
    if (!parameters) {
        return {};
    }

    if (typeof parameters === "object") {
        return parameters;
    }

    try {
        return JSON.parse(parameters);
    } catch {
        return {};
    }
}

function formatParameter(params, keys, suffix = "") {
    for (const key of keys) {
        if (params[key] !== undefined && params[key] !== null && params[key] !== "") {
            const value = params[key];
            return suffix && typeof value === "number"
                ? `${value}${suffix}`
                : String(value);
        }
    }

    return "—";
}

const RecipeDetails = () => {
    const { id } = useParams();

    const [recipe, setRecipe] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState("");

    const loadRecipe = async () => {
        try {
            setIsLoading(true);
            setError("");

            const data = await getRecipeById(id);
            setRecipe(data);
        } catch {
            setError("Nie udało się pobrać receptury.");
            setRecipe(null);
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        loadRecipe();
    }, [id]);

    const handleFavorite = async () => {
        const wasFavorite = recipe.isFavorite ?? false;

        setRecipe((previous) => ({
            ...previous,
            isFavorite: !wasFavorite
        }));

        try {
            if (wasFavorite) {
                await removeRecipeFavorite(recipe.id);
            } else {
                await addRecipeFavorite(recipe.id);
            }
        } catch {
            setRecipe((previous) => ({
                ...previous,
                isFavorite: wasFavorite
            }));
        }
    };

    if (isLoading) {
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
                        color: "#707070",
                        fontSize: "16px"
                    }}
                >
                    Ładowanie...
                </div>
            </div>
        );
    }

    if (error || !recipe) {
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
                    {error || "Nie znaleziono receptury."}
                </div>
            </div>
        );
    }

    const parameters = parseParameters(recipe.parameters);
    const coffeeAmount = formatParameter(
        parameters,
        ["coffee", "coffee_grams"],
        "g"
    );
    const waterAmount = formatParameter(
        parameters,
        ["water", "water_ml"],
        "ml"
    );
    const temperature = formatParameter(
        parameters,
        ["temperature"],
        "°C"
    );
    const brewTime = formatParameter(
        parameters,
        ["brewTime", "brew_time"]
    );
    const grindSize = formatParameter(
        parameters,
        ["grindSize", "grind_size"]
    );
    const statusLabel = recipe.isPublic ? "PUBLISHED" : "DRAFT";

    const exportToTXT = () => {
        const content = `
${recipe.title}

Metoda: ${recipe.brewingMethod ?? "—"}
Status: ${statusLabel}

PARAMETRY
- Kawa: ${coffeeAmount}
- Woda: ${waterAmount}
- Temperatura: ${temperature}
- Czas: ${brewTime}
- Mielenie: ${grindSize}

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
"${recipe.title}","${recipe.brewingMethod ?? ""}","${statusLabel}","${coffeeAmount}","${waterAmount}","${temperature}","${brewTime}","${grindSize}","${recipe.steps.replace(/\n/g, " ")}"
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
                                {recipe.brewingMethod ?? "—"}
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

                        <div
                            style={{
                                display: "flex",
                                gap: "10px",
                                alignItems: "center"
                            }}
                        >
                            <button
                                style={favoriteButtonStyle}
                                onClick={handleFavorite}
                                aria-label={
                                    recipe.isFavorite
                                        ? "Usuń z ulubionych"
                                        : "Dodaj do ulubionych"
                                }
                            >
                                <Heart
                                    size={18}
                                    fill={
                                        recipe.isFavorite
                                            ? "currentColor"
                                            : "none"
                                    }
                                />
                            </button>

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
                                    {coffeeAmount}
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
                                    {waterAmount}
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
                                    {temperature}
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
                                    {brewTime}
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
                                {recipe.coffee && (
                                    <div>
                                        <span style={labelStyle}>
                                            Kawa:
                                        </span>{" "}
                                        {recipe.coffee}
                                    </div>
                                )}

                                <div>
                                    <span style={labelStyle}>
                                        Stopień mielenia:
                                    </span>{" "}
                                    {grindSize}
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

const favoriteButtonStyle = {
    backgroundColor: "#efefef",
    color: "#2f2f2f",
    padding: "10px 14px",
    borderRadius: "16px",
    border: "1px solid #dddddd",
    cursor: "pointer",
    display: "flex",
    alignItems: "center",
    justifyContent: "center"
};

const exportButtonStyle = {
    backgroundColor: "#efefef",
    color: "#2f2f2f",
    padding: "10px 14px",
    borderRadius: "16px",
    border: "1px solid #dddddd",
    cursor: "pointer",
    display: "flex",
    alignItems: "center",
    gap: "8px",
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
