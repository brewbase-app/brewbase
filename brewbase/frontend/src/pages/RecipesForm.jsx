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
import { getCoffees } from "../api/coffeeApi";
import { getBrewingMethods } from "../api/brewingMethodApi";
import { ApiError } from "../api/apiClient";
import {
    hasValidationErrors,
    mapBackendErrors,
    validateRecipeDraft,
    validateRecipePublish
} from "../utils/recipeValidation";

const RecipesForm = () => {

    const navigate = useNavigate();

    const { id } = useParams();

    const isEditing = Boolean(id);

    const [formData, setFormData] = useState({
        title: "",
        description: "",
        coffeeId: "",
        brewingMethodId: "",
        coffee: "",
        water: "",
        temperature: "",
        grindSize: "",
        minutes: "",
        seconds: ""
    });

    const [fieldErrors, setFieldErrors] = useState({});
    const [coffees, setCoffees] = useState([]);
    const [brewingMethods, setBrewingMethods] = useState([]);
    const [catalogLoading, setCatalogLoading] = useState(true);

    const handleChange = (e) => {

        setFormData({
            ...formData,
            [e.target.name]: e.target.value
        });

        if (fieldErrors[e.target.name]) {
            setFieldErrors((currentErrors) => {
                const nextErrors = { ...currentErrors };
                delete nextErrors[e.target.name];
                return nextErrors;
            });
        }
    };

    const buildPayload = (status) => {
        const isPublishing = status === "PUBLISHED";
        const coffeeId = formData.coffeeId ? Number(formData.coffeeId) : null;
        const brewingMethodId = formData.brewingMethodId
            ? Number(formData.brewingMethodId)
            : null;

        return {
            title: formData.title,
            steps: formData.description,
            parameters: {
                coffee: formData.coffee ? `${formData.coffee}g` : "",
                water: formData.water ? `${formData.water}ml` : "",
                temperature: formData.temperature ? `${formData.temperature}°C` : "",
                grindSize: formData.grindSize,
                brewTime: `${formData.minutes || "0"}:${formData.seconds || "0"}`
            },
            isPublic: isPublishing,
            coffeeId,
            brewingMethodId
        };
    };

    const saveRecipe = async (status) => {

        const validationErrors = status === "PUBLISHED"
            ? validateRecipePublish(formData)
            : validateRecipeDraft(formData);

        if (hasValidationErrors(validationErrors)) {
            setFieldErrors(validationErrors);
            return;
        }

        setFieldErrors({});

        try {

            const payload = buildPayload(status);

            console.log(payload);

            if (isEditing) {

                await updateRecipe(id, payload);

            } else {

                await createRecipe(payload);
            }

            navigate("/recipes/my");

        } catch (error) {

            console.error(error);

            if (error instanceof ApiError && Object.keys(error.errors).length > 0) {
                setFieldErrors(mapBackendErrors(error.errors));
                return;
            }

            alert("Nie udało się zapisać receptury.");
        }
    };

    const renderFieldError = (fieldName) => {
        if (!fieldErrors[fieldName]) {
            return null;
        }

        return (
            <p style={errorTextStyle}>
                {fieldErrors[fieldName]}
            </p>
        );
    };

    useEffect(() => {

        const loadCatalog = async () => {
            try {
                const [coffeeList, methodList] = await Promise.all([
                    getCoffees(),
                    getBrewingMethods()
                ]);

                setCoffees(Array.isArray(coffeeList) ? coffeeList : []);
                setBrewingMethods(Array.isArray(methodList) ? methodList : []);
            } catch (error) {
                console.error(error);
            } finally {
                setCatalogLoading(false);
            }
        };

        loadCatalog();

    }, []);

    useEffect(() => {

        if (!isEditing) return;

        const fetchRecipe = async () => {

            try {

                const data = await getRecipeById(id);

                const parameters =
                    typeof data.parameters === "string"
                        ? JSON.parse(data.parameters)
                        : data.parameters;

                setFormData({
                    title: data.title || "",
                    description: data.steps || "",
                    coffeeId: data.coffeeId ? String(data.coffeeId) : "",
                    brewingMethodId: data.brewingMethodId
                        ? String(data.brewingMethodId)
                        : "",
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

                            {fieldErrors.form && (
                                <p style={errorTextStyle}>
                                    {fieldErrors.form}
                                </p>
                            )}

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
                                    style={{
                                        ...inputStyle,
                                        borderColor: fieldErrors.title ? "#d14343" : inputStyle.border
                                    }}
                                    value={formData.title}
                                    onChange={handleChange}
                                />
                                {renderFieldError("title")}

                                <textarea
                                    name="description"
                                    placeholder="Opis przygotowania"
                                    value={formData.description}
                                    onChange={handleChange}
                                    style={{
                                        ...inputStyle,
                                        height: "110px",
                                        resize: "none",
                                        paddingTop: "14px",
                                        borderColor: fieldErrors.description ? "#d14343" : inputStyle.border
                                    }}
                                />
                                {renderFieldError("description")}

                                <select
                                    name="coffeeId"
                                    value={formData.coffeeId}
                                    onChange={handleChange}
                                    disabled={catalogLoading}
                                    style={{
                                        ...inputStyle,
                                        borderColor: fieldErrors.coffeeId ? "#d14343" : inputStyle.border
                                    }}
                                >
                                    <option value="">
                                        {catalogLoading ? "Ładowanie kaw..." : "Wybierz kawę (opcjonalnie w roboczej)"}
                                    </option>
                                    {coffees.map((coffee) => (
                                        <option key={coffee.id} value={coffee.id}>
                                            {coffee.name}
                                        </option>
                                    ))}
                                </select>
                                {renderFieldError("coffeeId")}

                                <select
                                    name="brewingMethodId"
                                    value={formData.brewingMethodId}
                                    onChange={handleChange}
                                    disabled={catalogLoading}
                                    style={{
                                        ...inputStyle,
                                        borderColor: fieldErrors.brewingMethodId ? "#d14343" : inputStyle.border
                                    }}
                                >
                                    <option value="">
                                        {catalogLoading
                                            ? "Ładowanie metod..."
                                            : "Wybierz metodę parzenia (opcjonalnie w roboczej)"}
                                    </option>
                                    {brewingMethods.map((method) => (
                                        <option key={method.id} value={method.id}>
                                            {method.name}
                                        </option>
                                    ))}
                                </select>
                                {renderFieldError("brewingMethodId")}

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
                                    style={{
                                        ...inputStyle,
                                        borderColor: fieldErrors.coffee ? "#d14343" : inputStyle.border
                                    }}
                                />
                                {renderFieldError("coffee")}

                                <input
                                    name="water"
                                    placeholder="Ilość wody (ml)"
                                    value={formData.water}
                                    onChange={handleChange}
                                    style={{
                                        ...inputStyle,
                                        borderColor: fieldErrors.water ? "#d14343" : inputStyle.border
                                    }}
                                />
                                {renderFieldError("water")}

                                <input
                                    name="temperature"
                                    placeholder="Temperatura wody (°C)"
                                    value={formData.temperature}
                                    onChange={handleChange}
                                    style={{
                                        ...inputStyle,
                                        borderColor: fieldErrors.temperature ? "#d14343" : inputStyle.border
                                    }}
                                />
                                {renderFieldError("temperature")}

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
                                    style={{
                                        ...inputStyle,
                                        borderColor: fieldErrors.minutes ? "#d14343" : inputStyle.border
                                    }}
                                />
                                {renderFieldError("minutes")}

                                <input
                                    name="seconds"
                                    placeholder="Sekundy"
                                    value={formData.seconds}
                                    onChange={handleChange}
                                    style={{
                                        ...inputStyle,
                                        borderColor: fieldErrors.seconds ? "#d14343" : inputStyle.border
                                    }}
                                />
                                {renderFieldError("seconds")}

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

const errorTextStyle = {
    margin: "6px 0 0 0",
    fontSize: "13px",
    color: "#d14343"
};

export default RecipesForm;

