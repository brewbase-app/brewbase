
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
    Download
} from "lucide-react";

import { getRecipeById } from "../api/recipeApi";

function parseRecipeParameters(rawParameters) {
    if (!rawParameters) {
        return {};
    }

    if (typeof rawParameters === "string") {
        try {
            const parsed = JSON.parse(rawParameters);
            return parsed && typeof parsed === "object" ? parsed : {};
        } catch {
            return {};
        }
    }

    if (typeof rawParameters === "object") {
        return rawParameters;
    }

    return {};
}

const RecipeDetails = () => {
    

    const navigate = useNavigate();
    const { id } = useParams();

    const [recipe, setRecipe] = useState(null);

    const [loading, setLoading] = useState(true);

    useEffect(() => {

        const fetchRecipe = async () => {

            try {

                const data = await getRecipeById(id);

                const parsedRecipe = {
                    ...data,
                    parameters: parseRecipeParameters(data.parameters)
                };

                setRecipe(parsedRecipe);

            } catch (error) {

                console.error(error);

            } finally {

                setLoading(false);
            }
        };

        fetchRecipe();

    }, [id]);

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

    const parameters = recipe.parameters || {};
    const displayValue = (value, fallback = "—") => value || fallback;

    const exportToTXT = () => {

        const content = `
${recipe.title}

Kawa (katalog): ${displayValue(recipe.coffee, "Nie wybrano")}
Metoda: ${displayValue(recipe.brewingMethod, "Nie wybrano")}
Status: ${recipe.isPublic ? "Publiczna" : "Robocza"}

PARAMETRY
- Dawka kawy: ${displayValue(parameters.coffee)}
- Woda: ${displayValue(parameters.water)}
- Temperatura: ${displayValue(parameters.temperature)}
- Czas: ${displayValue(parameters.brewTime)}
- Mielenie: ${displayValue(parameters.grindSize)}

OPIS
${recipe.steps || ""}
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
Title,Brewing Method,Status,Catalog Coffee,Coffee Dose,Water,Temperature,Brew Time,Grind Size,Steps
"${recipe.title}","${displayValue(recipe.brewingMethod, "Nie wybrano")}","${recipe.isPublic ? "Publiczna" : "Robocza"}","${displayValue(recipe.coffee, "Nie wybrano")}","${displayValue(parameters.coffee)}","${displayValue(parameters.water)}","${displayValue(parameters.temperature)}","${displayValue(parameters.brewTime)}","${displayValue(parameters.grindSize)}","${(recipe.steps || "").replace(/\n/g, " ")}"
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

                    {recipe.coffee && (
                        <p
                            style={{
                                fontSize: "18px",
                                color: "#6f6f6f",
                                margin: "0 0 14px 0"
                            }}
                        >
                            {recipe.coffee}
                        </p>
                    )}

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
                                    {displayValue(recipe.brewingMethod, "Metoda nie wybrana")}
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

                        </div>

                        {/* ACTIONS */}

                        <div
                            style={{
                                display: "flex",
                                gap: "10px"
                            }}
                        >

                            <button
                                style={editButtonStyle}
                                onClick={() =>
                                    navigate(`/recipes/edit/${recipe.id}`)
                                }
                            >
                                Edytuj
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
                                    {displayValue(parameters.coffee)}
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
                                    {displayValue(parameters.water)}
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
                                    {displayValue(parameters.temperature)}
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
                                    {displayValue(parameters.brewTime)}
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
                                    {displayValue(recipe.steps, "Brak opisu")}
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
                                    {displayValue(parameters.grindSize)}
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
    border: "none",
    cursor: "pointer",
    fontSize: "14px",
    fontWeight: "600"
};

const exportButtonStyle = {
    display: "flex",
    alignItems: "center",
    gap: "8px",
    backgroundColor: "#ededed",
    color: "#2f2f2f",
    padding: "10px 16px",
    borderRadius: "16px",
    border: "1px solid #d9d9d9",
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

