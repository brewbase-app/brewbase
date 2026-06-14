import { useEffect, useState } from "react";

import {
    useNavigate,
    useParams
} from "react-router-dom";

import {
    createRecipe,
    getRecipeById,
    updateRecipe
} from "../../api/recipeApi";
import { getCoffees } from "../../api/coffeeApi";
import { getBrewingMethods } from "../../api/brewingMethodApi";
import { ApiError } from "../../api/apiClient";
import {
    buildRecipeParameters,
    hasValidationErrors,
    mapBackendErrors,
    validateRecipeDraft,
    validateRecipePublish
} from "../../utils/recipeValidation";
import {
    formatBrewingMethodSelectLabel,
    formatCoffeeSelectLabel,
    getBrewingMethodSelectPlaceholder,
    getCoffeeSelectPlaceholder
} from "../../utils/recipeCatalog";
import { getRecipeModerationComment } from "../../utils/recipeModeration";

import "../../styles/recipe/recipeLayout.css";
import "../../styles/recipe/RecipesForm.css";
import { sortByName } from "../../utils/sortOptions";

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
            <p className="recipe-form__error">
                {fieldErrors[fieldName]}
            </p>
        );
    };

    const inputClassName = (fieldName) =>
        fieldErrors[fieldName]
            ? "recipe-form__input recipe-form__input--error"
            : "recipe-form__input";

    const loadCatalog = async () => {
        setCatalogLoading(true);
        setCatalogError("");

        try {
            const [coffeeList, methodList] = await Promise.all([
                getCoffees(),
                getBrewingMethods()
            ]);

            setCoffees(sortByName(Array.isArray(coffeeList) ? coffeeList : []));
            setBrewingMethods(sortByName(Array.isArray(methodList) ? methodList : []));
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

    const submitButtonClass = (variant) =>
        [
            "recipe-form__btn",
            variant === "draft"
                ? "recipe-form__btn--draft"
                : "recipe-form__btn--publish",
            isSubmitting ? "recipe-form__btn--disabled" : ""
        ]
            .filter(Boolean)
            .join(" ");

    return (
        <div className="recipe-page recipe-page--centered">
            <div className="recipe-page__container recipe-page__container--md">
                <div className="recipe-page__header">
                    <h1 className="recipe-page__title">
                        {isEditing
                            ? "Edytuj recepturę"
                            : "Nowa receptura"}
                    </h1>
                    <p className="recipe-page__subtitle">
                        {isEditing
                            ? "Zaktualizuj swoją recepturę."
                            : "Dodaj nowy przepis parzenia kawy."}
                    </p>
                </div>

                {isEditing && !isPublicRecipe && moderationComment && (
                    <div className="recipe-form__moderation-banner">
                        <strong>Komentarz moderatora:</strong> {moderationComment}
                    </div>
                )}

                <div className="recipe-form__card">
                    <div className="recipe-form__fields">
                        <div>
                            <p className="recipe-form__section-title">
                                Podstawowe informacje
                            </p>

                            {fieldErrors.form && (
                                <p className="recipe-form__error">
                                    {fieldErrors.form}
                                </p>
                            )}

                            <div className="recipe-form__group">
                                <input
                                    name="title"
                                    placeholder="Nazwa receptury"
                                    className={inputClassName("title")}
                                    value={formData.title}
                                    onChange={handleChange}
                                />
                                {renderFieldError("title")}

                                <textarea
                                    name="description"
                                    placeholder="Opis przygotowania"
                                    value={formData.description}
                                    onChange={handleChange}
                                    className={`${inputClassName("description")} recipe-form__input--textarea`}
                                />
                                {renderFieldError("description")}

                                <p className="recipe-form__label">
                                    Kawa z katalogu
                                </p>

                                {catalogError && (
                                    <div className="recipe-form__notice recipe-form__notice--error">
                                        <p className="recipe-form__notice-text recipe-form__notice-text--error">
                                            {catalogError}
                                        </p>
                                        <button
                                            type="button"
                                            className="recipe-form__notice-btn"
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
                                    className={inputClassName("coffeeId")}
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

                                <div className="recipe-form__notice recipe-form__notice--wiki">
                                    <p className="recipe-form__notice-text recipe-form__notice-text--wiki">
                                        Nie ma Twojej kawy na liście?
                                    </p>
                                    <button
                                        type="button"
                                        className="recipe-form__notice-btn recipe-form__notice-btn--wiki"
                                        onClick={() => navigate("/wiki/add?module=coffee")}
                                    >
                                        Dodaj artykuł wiki
                                    </button>
                                </div>

                                <p className="recipe-form__label">
                                    Metoda parzenia z katalogu
                                </p>

                                <select
                                    name="brewingMethodId"
                                    value={formData.brewingMethodId}
                                    onChange={handleChange}
                                    disabled={catalogLoading}
                                    className={inputClassName("brewingMethodId")}
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

                                <div className="recipe-form__notice recipe-form__notice--wiki">
                                    <p className="recipe-form__notice-text recipe-form__notice-text--wiki">
                                        Nie ma Twojej metody parzenia na liście?
                                    </p>
                                    <button
                                        type="button"
                                        className="recipe-form__notice-btn recipe-form__notice-btn--wiki"
                                        onClick={() => navigate("/wiki/add?module=brewing_method")}
                                    >
                                        Dodaj artykuł wiki
                                    </button>
                                </div>
                            </div>
                        </div>

                        <div className="recipe-form__section">
                            <p className="recipe-form__section-title">
                                Parametry parzenia
                            </p>

                            <div className="recipe-form__grid">
                                <div>
                                    <input
                                        name="coffee"
                                        placeholder="Ilość kawy (g)"
                                        value={formData.coffee}
                                        onChange={handleChange}
                                        className={inputClassName("coffee")}
                                    />
                                    {renderFieldError("coffee")}
                                </div>

                                <div>
                                    <input
                                        name="water"
                                        placeholder="Ilość wody (ml)"
                                        value={formData.water}
                                        onChange={handleChange}
                                        className={inputClassName("water")}
                                    />
                                    {renderFieldError("water")}
                                </div>

                                <div>
                                    <input
                                        name="temperature"
                                        placeholder="Temperatura wody (°C)"
                                        value={formData.temperature}
                                        onChange={handleChange}
                                        className={inputClassName("temperature")}
                                    />
                                    {renderFieldError("temperature")}
                                </div>

                                <div>
                                    <input
                                        name="grindSize"
                                        placeholder="Stopień mielenia"
                                        value={formData.grindSize}
                                        onChange={handleChange}
                                        className="recipe-form__input"
                                    />
                                </div>
                            </div>
                        </div>

                        <div className="recipe-form__section">
                            <p className="recipe-form__section-title">
                                Czas parzenia
                            </p>

                            <div className="recipe-form__grid">
                                <div>
                                    <input
                                        name="minutes"
                                        placeholder="Minuty"
                                        value={formData.minutes}
                                        onChange={handleChange}
                                        className={
                                            fieldErrors.minutes || fieldErrors.brewTime
                                                ? "recipe-form__input recipe-form__input--error"
                                                : "recipe-form__input"
                                        }
                                    />
                                    {renderFieldError("minutes")}
                                </div>

                                <div>
                                    <input
                                        name="seconds"
                                        placeholder="Sekundy"
                                        value={formData.seconds}
                                        onChange={handleChange}
                                        className={
                                            fieldErrors.seconds || fieldErrors.brewTime
                                                ? "recipe-form__input recipe-form__input--error"
                                                : "recipe-form__input"
                                        }
                                    />
                                    {renderFieldError("seconds")}
                                </div>
                            </div>

                            {renderFieldError("brewTime")}
                        </div>

                        <div className="recipe-form__actions">
                            <button
                                type="button"
                                className={submitButtonClass("draft")}
                                disabled={isSubmitting}
                                onClick={() => saveRecipe("DRAFT")}
                            >
                                {isSubmitting
                                    ? "Zapisywanie..."
                                    : "Zapisz wersję roboczą"}
                            </button>

                            <button
                                type="button"
                                className={submitButtonClass("publish")}
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
    );
};

export default RecipesForm;
