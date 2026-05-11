import { useState } from "react";
import { useNavigate } from "react-router-dom";

const RecipesForm = () => {
    const navigate = useNavigate();

    const [formData, setFormData] = useState({
        title: "",
        description: "",
        brewingMethod: "",
        coffee: "",
        water: "",
        temperature: "",
        grindSize: "",
        minutes: "",
        seconds: ""
    });

    const handleChange = (e) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value
        });
    };

    const saveRecipe = (status) => {
        const existingRecipes =
            JSON.parse(localStorage.getItem("recipes")) || [];

        const newRecipe = {
            id: crypto.randomUUID(),

            title: formData.title,

            coffee: "Custom Coffee",

            brewingMethod: formData.brewingMethod,

            steps: formData.description,

            parameters: {
                coffee: `${formData.coffee}g`,
                water: `${formData.water}ml`,
                temperature: `${formData.temperature}°C`,
                grindSize: formData.grindSize,
                brewTime: `${formData.minutes}:${formData.seconds}`
            },

            status,
            createdAt: new Date().toISOString(),
            isFavorite: false
        };

        const updatedRecipes = [...existingRecipes, newRecipe];

        localStorage.setItem(
            "recipes",
            JSON.stringify(updatedRecipes)
        );

        navigate("/recipes/my");
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
            <div style={{ width: "100%", maxWidth: "900px" }}>

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
                        Nowa receptura
                    </h1>

                    <p
                        style={{
                            fontSize: "16px",
                            color: "#6f6f6f"
                        }}
                    >
                        Dodaj nowy przepis parzenia kawy.
                    </p>
                </div>

                {/* FORM */}
                <div
                    style={{
                        backgroundColor: "#fafafa",
                        borderRadius: "28px",
                        border: "1px solid #e6e6e6",
                        padding: "32px",
                        boxShadow: "0 2px 10px rgba(0,0,0,0.03)"
                    }}
                >
                    <div
                        style={{
                            display: "flex",
                            flexDirection: "column",
                            gap: "18px"
                        }}
                    >

                        {/* BASIC INFO */}
                        <div>
                            <p style={sectionTitle}>
                                Podstawowe informacje
                            </p>

                            <div
                                style={{
                                    display: "flex",
                                    flexDirection: "column",
                                    gap: "14px"
                                }}
                            >
                                <input
                                    name="title"
                                    placeholder="Nazwa receptury"
                                    style={inputStyle}
                                    value={formData.title}
                                    onChange={handleChange}
                                />

                                <textarea
                                    name="description"
                                    placeholder="Opis przygotowania"
                                    value={formData.description}
                                    onChange={handleChange}
                                    style={{
                                        ...inputStyle,
                                        height: "110px",
                                        resize: "none",
                                        paddingTop: "14px"
                                    }}
                                />

                                <input
                                    name="brewingMethod"
                                    placeholder="Metoda parzenia"
                                    value={formData.brewingMethod}
                                    onChange={handleChange}
                                    style={inputStyle}
                                />
                            </div>
                        </div>

                        {/* PARAMETERS */}
                        <div style={{ marginTop: "14px" }}>
                            <p style={sectionTitle}>
                                Parametry parzenia
                            </p>

                            <div
                                style={{
                                    display: "grid",
                                    gridTemplateColumns: "1fr 1fr",
                                    gap: "14px"
                                }}
                            >
                                <input
                                    name="coffee"
                                    placeholder="Ilość kawy (g)"
                                    value={formData.coffee}
                                    onChange={handleChange}
                                    style={inputStyle}
                                />

                                <input
                                    name="water"
                                    placeholder="Ilość wody (ml)"
                                    value={formData.water}
                                    onChange={handleChange}
                                    style={inputStyle}
                                />

                                <input
                                    name="temperature"
                                    placeholder="Temperatura wody (°C)"
                                    value={formData.temperature}
                                    onChange={handleChange}
                                    style={inputStyle}
                                />

                                <input
                                    name="grindSize"
                                    placeholder="Stopień mielenia"
                                    value={formData.grindSize}
                                    onChange={handleChange}
                                    style={inputStyle}
                                />
                            </div>
                        </div>

                        {/* TIME */}
                        <div style={{ marginTop: "14px" }}>
                            <p style={sectionTitle}>
                                Czas parzenia
                            </p>

                            <div
                                style={{
                                    display: "grid",
                                    gridTemplateColumns: "1fr 1fr",
                                    gap: "14px"
                                }}
                            >
                                <input
                                    name="minutes"
                                    placeholder="Minuty"
                                    value={formData.minutes}
                                    onChange={handleChange}
                                    style={inputStyle}
                                />

                                <input
                                    name="seconds"
                                    placeholder="Sekundy"
                                    value={formData.seconds}
                                    onChange={handleChange}
                                    style={inputStyle}
                                />
                            </div>
                        </div>

                        {/* BUTTONS */}
                        <div
                            style={{
                                display: "flex",
                                justifyContent: "flex-end",
                                gap: "14px",
                                marginTop: "26px"
                            }}
                        >
                            <button
                                style={draftButtonStyle}
                                onClick={() =>
                                    saveRecipe("DRAFT")
                                }
                            >
                                Zapisz wersję roboczą
                            </button>

                            <button
                                style={publishButtonStyle}
                                onClick={() =>
                                    saveRecipe("PUBLISHED")
                                }
                            >
                                Opublikuj recepturę
                            </button>
                        </div>

                    </div>
                </div>
            </div>
        </div>
    );
};

const sectionTitle = {
    fontSize: "15px",
    fontWeight: "600",
    color: "#4f4f4f",
    marginBottom: "14px"
};

const inputStyle = {
    width: "100%",
    boxSizing: "border-box",
    padding: "16px 18px",
    borderRadius: "18px",
    border: "1px solid #dddddd",
    backgroundColor: "#f5f5f5",
    fontSize: "15px",
    color: "#1f1f1f",
    outline: "none"
};

const draftButtonStyle = {
    backgroundColor: "#ededed",
    color: "#2f2f2f",
    padding: "14px 22px",
    borderRadius: "18px",
    border: "1px solid #d9d9d9",
    fontSize: "15px",
    fontWeight: "600",
    cursor: "pointer"
};

const publishButtonStyle = {
    backgroundColor: "#1f1f1f",
    color: "white",
    padding: "14px 22px",
    borderRadius: "18px",
    border: "none",
    fontSize: "15px",
    fontWeight: "600",
    cursor: "pointer"
};

export default RecipesForm;