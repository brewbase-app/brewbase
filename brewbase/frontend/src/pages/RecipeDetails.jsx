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
    Download
} from "lucide-react";

const RecipeDetails = () => {
    const { id } = useParams();

    const recipes =
        JSON.parse(localStorage.getItem("recipes")) || [];

    const recipe = recipes.find((r) => r.id === id);

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
Status: ${recipe.status}

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
"${recipe.title}","${recipe.brewingMethod}","${recipe.status}","${recipe.parameters.coffee}","${recipe.parameters.water}","${recipe.parameters.temperature}","${recipe.parameters.brewTime}","${recipe.parameters.grindSize}","${recipe.steps.replace(/\n/g, " ")}"
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
                                {recipe.brewingMethod}
                            </div>

                            <div style={badgeStyle}>
                                {recipe.status ===
                                "PUBLISHED" ? (
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

                        {/* EXPORT BUTTONS */}
                        <div
                            style={{
                                display: "flex",
                                gap: "10px"
                            }}
                        >
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

                                <div>
                                    <span style={labelStyle}>
                                        Utworzono:
                                    </span>{" "}
                                    {new Date(
                                        recipe.createdAt
                                    ).toLocaleDateString()}
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