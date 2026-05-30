import { useEffect, useState } from "react";

import {
    useNavigate,
    useParams
} from "react-router-dom";

import {
    createRecipe,
    getRecipeById,
    updateRecipe
} from "../api/recipeApi";

const RecipesForm = () => {

    const navigate = useNavigate();

    const { id } = useParams();

    const isEditing = Boolean(id);

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

    useEffect(() => {

        if (!isEditing) return;

        const fetchRecipe = async () => {

            try {

                const data = await getRecipeById(id);

                console.log(data);

                const parameters =
                    typeof data.parameters === "string"
                        ? JSON.parse(data.parameters)
                        : data.parameters;

                setFormData({
                    title: data.title || "",

                    description: data.steps || "",

                    brewingMethod:
                        data.brewingMethod || "",

                    coffee:
                        parameters.coffee
                            ?.replace("g", "") || "",

                    water:
                        parameters.water
                            ?.replace("ml", "") || "",

                    temperature:
                        parameters.temperature
                            ?.replace("°C", "") || "",

                    grindSize:
                        parameters.grindSize || "",

                    minutes:
                        parameters.brewTime
                            ?.split(":")[0] || "",

                    seconds:
                        parameters.brewTime
                            ?.split(":")[1] || ""
                });

            } catch (error) {

                console.error(error);
            }
        };

        fetchRecipe();

    }, [id, isEditing]);

    const saveRecipe = async (status) => {

        try {

            const payload = {

                title: formData.title,

                steps: formData.description,

                parameters: {
                    coffee: `${formData.coffee}g`,
                    water: `${formData.water}ml`,
                    temperature: `${formData.temperature}°C`,
                    grindSize: formData.grindSize,
                    brewTime:
                        `${formData.minutes}:${formData.seconds}`
                },

                isPublic: status === "PUBLISHED",

                coffeeId: 1,

                brewingMethodId: 1
            };

            console.log(payload);

            if (isEditing) {

                await updateRecipe(id, payload);

            } else {

                await createRecipe(payload);
            }

            navigate("/recipes/my");

        } catch (error) {

            console.error(error);

            alert("Nie udało się zapisać receptury.");
        }
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

                        {isEditing
                            ? "Edytuj recepturę"
                            : "Nowa receptura"}

                    </h1>

                    <p
                        style={{
                            fontSize: "16px",
                            color: "#6f6f6f"
                        }}
                    >

                        {isEditing
                            ? "Zaktualizuj swoją recepturę."
                            : "Dodaj nowy przepis parzenia kawy."}

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

                                {isEditing
                                    ? "Zapisz zmiany"
                                    : "Opublikuj recepturę"}

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

