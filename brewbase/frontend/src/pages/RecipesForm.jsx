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
    buildRecipeParameters,
    hasValidationErrors,
    mapBackendErrors,
    validateRecipeDraft,
    validateRecipePublish
} from "../utils/recipeValidation";
import {
    formatBrewingMethodSelectLabel,
    formatCoffeeSelectLabel,
    getBrewingMethodSelectPlaceholder,
    getCoffeeSelectPlaceholder
} from "../utils/recipeCatalog";
import { getRecipeModerationComment } from "../utils/recipeModeration";

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
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [coffees, setCoffees] = useState([]);
    const [brewingMethods, setBrewingMethods] = useState([]);
    const [catalogLoading, setCatalogLoading] = useState(true);
    const [catalogError, setCatalogError] = useState("");
    const [linkedCoffeeLabel, setLinkedCoffeeLabel] = useState("");
    const [linkedBrewingMethodLabel, setLinkedBrewingMethodLabel] = useState("");
    const [moderationComment, setModerationComment] = useState("");
    const [isPublicRecipe, setIsPublicRecipe] = useState(true);

    const handleChange = (e) => {
        const { name, value } = e.target;

        setFormData({
            ...formData,
            [name]: value
        });

        if (fieldErrors[name] || fieldErrors.brewTime) {
            setFieldErrors((currentErrors) => {
                const nextErrors = { ...currentErrors };
                delete nextErrors[name];

                if (name === "minutes" || name === "seconds") {
                    delete nextErrors.brewTime;
                }

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
            parameters: buildRecipeParameters(formData),
            isPublic: isPublishing,
            coffeeId,
            brewingMethodId
        };
    };

    const saveRecipe = async (status) => {
        if (isSubmitting) {
            return;
        }

        const validationErrors = status === "PUBLISHED"
            ? validateRecipePublish(formData)
            : validateRecipeDraft(formData);

        if (hasValidationErrors(validationErrors)) {
            setFieldErrors(validationErrors);
            return;
        }

        setFieldErrors({});

        try {
            setIsSubmitting(true);

            const payload = buildPayload(status);

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
        } finally {
            setIsSubmitting(false);
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

    const loadCatalog = async () => {
        setCatalogLoading(true);
        setCatalogError("");

        try {
            const [coffeeList, methodList] = await Promise.all([
                getCoffees(),
                getBrewingMethods()
            ]);

            setCoffees(Array.isArray(coffeeList) ? coffeeList : []);
            setBrewingMethods(Array.isArray(methodList) ? methodList : []);
        } catch (error) {
            console.error(error);
            setCatalogError("Nie udało się pobrać katalogu kaw i metod parzenia.");
            setCoffees([]);
            setBrewingMethods([]);
        } finally {
            setCatalogLoading(false);
        }
    };

    const coffeeInCatalog = (coffeeId) =>
        coffees.some((coffee) => String(coffee.id) === String(coffeeId));

    const methodInCatalog = (methodId) =>
        brewingMethods.some((method) => String(method.id) === String(methodId));

    const showMissingCoffeeOption =
        formData.coffeeId && !coffeeInCatalog(formData.coffeeId);

    const showMissingMethodOption =
        formData.brewingMethodId && !methodInCatalog(formData.brewingMethodId);

    useEffect(() => {
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

                setLinkedCoffeeLabel(data.coffee || "");
                setLinkedBrewingMethodLabel(data.brewingMethod || "");
                setModerationComment(getRecipeModerationComment(data));
                setIsPublicRecipe(Boolean(data.isPublic ?? data.IsPublic));

                setFormData({
                    title: data.title || "",
                    description: data.steps || "",
                    coffeeId: data.coffeeId ? String(data.coffeeId) : "",
                    brewingMethodId: data.brewingMethodId
                        ? String(data.brewingMethodId)
                        : "",
                    coffee:
                        parameters?.coffee
                            ?.replace("g", "") || "",
                    water:
                        parameters?.water
                            ?.replace("ml", "") || "",
                    temperature:
                        parameters?.temperature
                            ?.replace("°C", "") || "",
                    grindSize:
                        parameters?.grindSize || "",
                    minutes:
                        parameters?.brewTime
                            ?.split(":")[0] || "",
                    seconds:
                        parameters?.brewTime
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

                {isEditing && !isPublicRecipe && moderationComment && (
                    <div
                        style={{
                            marginBottom: "24px",
                            padding: "16px 18px",
                            borderRadius: "16px",
                            backgroundColor: "#fff4e5",
                            border: "1px solid #ffd8a8",
                            color: "#5c4a1f",
                            fontSize: "15px",
                            lineHeight: 1.5,
                        }}
                    >
                        <strong>Komentarz moderatora:</strong> {moderationComment}
                    </div>
                )}

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

                                <p style={fieldLabelStyle}>
                                    Kawa z katalogu
                                </p>

                                {catalogError && (
                                    <div style={catalogNoticeStyle}>
                                        <p style={catalogNoticeTextStyle}>
                                            {catalogError}
                                        </p>
                                        <button
                                            type="button"
                                            style={retryButtonStyle}
                                            onClick={loadCatalog}
                                        >
                                            Spróbuj ponownie
                                        </button>
                                    </div>
                                )}

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
                                        {getCoffeeSelectPlaceholder({
                                            loading: catalogLoading,
                                            error: Boolean(catalogError),
                                            isEmpty: !catalogLoading && !catalogError && coffees.length === 0
                                        })}
                                    </option>
                                    {showMissingCoffeeOption && (
                                        <option value={formData.coffeeId}>
                                            {linkedCoffeeLabel || `Kawa #${formData.coffeeId}`}
                                        </option>
                                    )}
                                    {coffees.map((coffee) => (
                                        <option
                                            key={coffee.id}
                                            value={coffee.id}
                                            title={[coffee.roastery, coffee.region].filter(Boolean).join(", ")}
                                        >
                                            {formatCoffeeSelectLabel(coffee)}
                                        </option>
                                    ))}
                                </select>
                                {renderFieldError("coffeeId")}

                                <div style={wikiHintStyle}>
                                    <p style={wikiHintTextStyle}>
                                        Nie ma Twojej kawy na liście?
                                    </p>
                                    <button
                                        type="button"
                                        style={wikiLinkButtonStyle}
                                        onClick={() => navigate("/wiki/add?module=coffee")}
                                    >
                                        Dodaj artykuł wiki
                                    </button>
                                </div>

                                <p style={fieldLabelStyle}>
                                    Metoda parzenia z katalogu
                                </p>

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
                                        {getBrewingMethodSelectPlaceholder({
                                            loading: catalogLoading,
                                            error: Boolean(catalogError),
                                            isEmpty: !catalogLoading && !catalogError && brewingMethods.length === 0
                                        })}
                                    </option>
                                    {showMissingMethodOption && (
                                        <option value={formData.brewingMethodId}>
                                            {linkedBrewingMethodLabel
                                                || `Metoda #${formData.brewingMethodId}`}
                                        </option>
                                    )}
                                    {brewingMethods.map((method) => (
                                        <option
                                            key={method.id}
                                            value={method.id}
                                            title={method.description || method.name}
                                        >
                                            {formatBrewingMethodSelectLabel(method)}
                                        </option>
                                    ))}
                                </select>
                                {renderFieldError("brewingMethodId")}

                                <div style={wikiHintStyle}>
                                    <p style={wikiHintTextStyle}>
                                        Nie ma Twojej metody parzenia na liście?
                                    </p>
                                    <button
                                        type="button"
                                        style={wikiLinkButtonStyle}
                                        onClick={() => navigate("/wiki/add?module=brewing_method")}
                                    >
                                        Dodaj artykuł wiki
                                    </button>
                                </div>

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
                                <div>
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
                                </div>

                                <div>
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
                                </div>

                                <div>
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
                                </div>

                                <div>
                                    <input
                                        name="grindSize"
                                        placeholder="Stopień mielenia"
                                        value={formData.grindSize}
                                        onChange={handleChange}
                                        style={inputStyle}
                                    />
                                </div>

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
                                <div>
                                    <input
                                        name="minutes"
                                        placeholder="Minuty"
                                        value={formData.minutes}
                                        onChange={handleChange}
                                        style={{
                                            ...inputStyle,
                                            borderColor: (fieldErrors.minutes || fieldErrors.brewTime)
                                                ? "#d14343"
                                                : inputStyle.border
                                        }}
                                    />
                                    {renderFieldError("minutes")}
                                </div>

                                <div>
                                    <input
                                        name="seconds"
                                        placeholder="Sekundy"
                                        value={formData.seconds}
                                        onChange={handleChange}
                                        style={{
                                            ...inputStyle,
                                            borderColor: (fieldErrors.seconds || fieldErrors.brewTime)
                                                ? "#d14343"
                                                : inputStyle.border
                                        }}
                                    />
                                    {renderFieldError("seconds")}
                                </div>
                            </div>

                            {renderFieldError("brewTime")}

                        </div>

                        {/* BUTTONS */}

                        <div style={{ marginTop: "26px" }}>
                            <div
                                style={{
                                    display: "flex",
                                    justifyContent: "flex-end",
                                    gap: "14px"
                                }}
                            >
                                <button
                                    type="button"
                                    style={{
                                        ...draftButtonStyle,
                                        opacity: isSubmitting ? 0.7 : 1,
                                        cursor: isSubmitting ? "not-allowed" : "pointer"
                                    }}
                                    disabled={isSubmitting}
                                    onClick={() => saveRecipe("DRAFT")}
                                >
                                    {isSubmitting
                                        ? "Zapisywanie..."
                                        : "Zapisz wersję roboczą"}
                                </button>

                                <button
                                    type="button"
                                    style={{
                                        ...publishButtonStyle,
                                        opacity: isSubmitting ? 0.7 : 1,
                                        cursor: isSubmitting ? "not-allowed" : "pointer"
                                    }}
                                    disabled={isSubmitting}
                                    onClick={() => saveRecipe("PUBLISHED")}
                                >
                                    {isSubmitting
                                        ? "Zapisywanie..."
                                        : isEditing
                                            ? "Zapisz zmiany"
                                            : "Opublikuj recepturę"}
                                </button>
                            </div>
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

const fieldLabelStyle = {
    margin: "4px 0 0 0",
    fontSize: "13px",
    fontWeight: "600",
    color: "#5a5a5a"
};

const catalogNoticeStyle = {
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    gap: "12px",
    padding: "12px 14px",
    borderRadius: "14px",
    backgroundColor: "#fff4f4",
    border: "1px solid #f0cccc"
};

const catalogNoticeTextStyle = {
    margin: 0,
    fontSize: "13px",
    color: "#9b2c2c"
};

const retryButtonStyle = {
    backgroundColor: "#ffffff",
    color: "#1f1f1f",
    padding: "8px 12px",
    borderRadius: "12px",
    border: "1px solid #dddddd",
    fontSize: "13px",
    fontWeight: "600",
    cursor: "pointer",
    whiteSpace: "nowrap"
};

const wikiHintStyle = {
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    gap: "12px",
    padding: "12px 14px",
    borderRadius: "14px",
    backgroundColor: "#f0f4ff",
    border: "1px solid #d9e3ff"
};

const wikiHintTextStyle = {
    margin: 0,
    fontSize: "13px",
    color: "#4f5d78"
};

const wikiLinkButtonStyle = {
    backgroundColor: "#ffffff",
    color: "#1f1f1f",
    padding: "8px 12px",
    borderRadius: "12px",
    border: "1px solid #c9d7ff",
    fontSize: "13px",
    fontWeight: "600",
    cursor: "pointer",
    whiteSpace: "nowrap"
};

export default RecipesForm;

