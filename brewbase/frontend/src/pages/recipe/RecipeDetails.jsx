
import { useEffect, useState } from "react";

import {
    useNavigate,
    useParams,
    Link
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
} from "../../api/recipeApi";
import { getProfile } from "../../api/profileApi";
import { getRecipeModerationComment } from "../../utils/recipeModeration";

import "../../styles/recipe/recipeLayout.css";
import "../../styles/recipe/RecipeDetails.css";

const RecipeDetails = () => {

    const { id } = useParams();
    const navigate = useNavigate();

    const [recipe, setRecipe] = useState(null);

    const [loading, setLoading] = useState(true);

    const [userRating, setUserRating] = useState(0);

    const [ratingMessage, setRatingMessage] = useState("");

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

                const existingRating = Number(
                    data.userRating ?? data.UserRating ?? 0
                );
                setUserRating(
                    Number.isFinite(existingRating) && existingRating > 0
                        ? existingRating
                        : 0
                );
                setRatingMessage("");

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
        if (userRating > 0) {
            return;
        }

        try {

            await rateRecipe(recipe.id, value);

            const updated = await getRecipeById(id);
            const savedRating = Number(
                updated.userRating ?? updated.UserRating ?? value
            );

            setUserRating(savedRating);
            setRecipe((previous) => ({
                ...previous,
                averageRating: updated.averageRating,
                ratingCount: updated.ratingCount ?? 0,
                userRating: savedRating,
            }));
            setRatingMessage("Twoja ocena została zapisana.");

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
            <div className="recipe-page">
                <div className="recipe-panel">
                    Ładowanie receptury...
                </div>
            </div>
        );
    }

    if (!recipe) {
        return (
            <div className="recipe-page">
                <div className="recipe-panel">
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

    const hasUserRated = userRating > 0;

    const recipeCoffeeId = recipe.coffeeId ?? recipe.CoffeeId;
    const recipeCoffeeName = recipe.coffee ?? recipe.Coffee;

    return (
        <div className="recipe-page recipe-page--centered">
            <div className="recipe-page__container recipe-page__container--lg">
                <div className="recipe-page__header">
                    <h1 className="recipe-page__title recipe-page__title--spaced">
                        {recipe.title}
                    </h1>

                    <div className="recipe-details__header-row">
                        <div>
                            <div className="recipe-details__badges">
                                {recipeCoffeeName && recipeCoffeeId ? (
                                    <Link
                                        to={`/wiki/coffees/${recipeCoffeeId}`}
                                        className="recipe-details__badge recipe-details__badge--link"
                                    >
                                        <Coffee size={14} />
                                        {recipeCoffeeName}
                                    </Link>
                                ) : recipeCoffeeName ? (
                                    <div className="recipe-details__badge">
                                        <Coffee size={14} />
                                        {recipeCoffeeName}
                                    </div>
                                ) : null}

                                {recipe.brewingMethod && (
                                    <div className="recipe-details__badge">
                                        <Droplets size={14} />
                                        {recipe.brewingMethod}
                                    </div>
                                )}

                                <div className="recipe-details__badge">
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
                                    <p className="recipe-moderation-note recipe-moderation-note--header">
                                        Komentarz moderatora:{" "}
                                        {getRecipeModerationComment(recipe)}
                                    </p>
                                )}

                            <div className="recipe-details__meta-row">
                                <button
                                    type="button"
                                    className="recipe-details__favorite-btn"
                                    onClick={handleFavorite}
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

                                {isOwner ? (
                                    <div className="recipe-details__rating">
                                        <span className="recipe-details__rating-text">
                                            Nie możesz oceniać własnej receptury
                                        </span>
                                        <span className="recipe-details__rating-text">
                                            {recipe.averageRating
                                                ? recipe.averageRating.toFixed(1)
                                                : "Brak ocen"}
                                            {" · "}
                                            {recipe.ratingCount || 0} ocen
                                        </span>
                                    </div>
                                ) : (
                                    <div className="recipe-details__rating">
                                        <div className="recipe-details__stars">
                                            {[1, 2, 3, 4, 5].map((star) => (
                                                <Star
                                                    key={star}
                                                    size={20}
                                                    className={
                                                        hasUserRated
                                                            ? "recipe-details__star recipe-details__star--readonly"
                                                            : "recipe-details__star"
                                                    }
                                                    fill={
                                                        star <= userRating
                                                            ? "#1f1f1f"
                                                            : "none"
                                                    }
                                                    onClick={
                                                        hasUserRated
                                                            ? undefined
                                                            : () =>
                                                                  handleRating(
                                                                      star
                                                                  )
                                                    }
                                                />
                                            ))}
                                        </div>

                                        <span className="recipe-details__rating-text">
                                            {recipe.averageRating
                                                ? recipe.averageRating.toFixed(1)
                                                : "Brak ocen"}
                                            {" · "}
                                            {recipe.ratingCount || 0} ocen
                                        </span>

                                        {hasUserRated && (
                                            <span className="recipe-details__rating-success">
                                                Twoja ocena: {userRating}/5
                                            </span>
                                        )}

                                        {ratingMessage && (
                                            <span
                                                role="status"
                                                className="recipe-details__rating-success"
                                            >
                                                {ratingMessage}
                                            </span>
                                        )}
                                    </div>
                                )}
                            </div>
                        </div>

                        <div className="recipe-details__actions">
                            {isOwner && (
                                <button
                                    type="button"
                                    className="recipe-details__btn recipe-details__btn--edit"
                                    onClick={() =>
                                        navigate(`/recipes/edit/${recipe.id}`)
                                    }
                                >
                                    Edytuj
                                </button>
                            )}

                            <button
                                type="button"
                                className="recipe-details__btn recipe-details__btn--export"
                                onClick={exportToTXT}
                            >
                                <Download size={16} />
                                TXT
                            </button>

                            <button
                                type="button"
                                className="recipe-details__btn recipe-details__btn--export"
                                onClick={exportToCSV}
                            >
                                <Download size={16} />
                                CSV
                            </button>

                            <button
                                type="button"
                                className="recipe-details__btn recipe-details__btn--report"
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

                <div className="recipe-details__main-card">
                    <div className="recipe-details__params">
                        <div className="recipe-details__info-card">
                            <Scale size={20} />
                            <div>
                                <p className="recipe-details__info-label">
                                    Ilość kawy
                                </p>
                                <p className="recipe-details__info-value">
                                    {recipe.parameters.coffee}
                                </p>
                            </div>
                        </div>

                        <div className="recipe-details__info-card">
                            <Droplets size={20} />
                            <div>
                                <p className="recipe-details__info-label">
                                    Ilość wody
                                </p>
                                <p className="recipe-details__info-value">
                                    {recipe.parameters.water}
                                </p>
                            </div>
                        </div>

                        <div className="recipe-details__info-card">
                            <Thermometer size={20} />
                            <div>
                                <p className="recipe-details__info-label">
                                    Temperatura
                                </p>
                                <p className="recipe-details__info-value">
                                    {recipe.parameters.temperature}
                                </p>
                            </div>
                        </div>

                        <div className="recipe-details__info-card">
                            <Timer size={20} />
                            <div>
                                <p className="recipe-details__info-label">
                                    Czas parzenia
                                </p>
                                <p className="recipe-details__info-value">
                                    {recipe.parameters.brewTime}
                                </p>
                            </div>
                        </div>
                    </div>

                    <div className="recipe-details__section">
                        <p className="recipe-details__section-title">
                            Opis przygotowania
                        </p>

                        <div className="recipe-details__content-card">
                            <div className="recipe-details__description">
                                <FileText size={18} color="#666" />
                                <div className="recipe-details__description-text">
                                    {recipe.steps}
                                </div>
                            </div>
                        </div>
                    </div>

                    <div>
                        <p className="recipe-details__section-title">
                            Dodatkowe informacje
                        </p>

                        <div className="recipe-details__content-card">
                            <div className="recipe-details__extra">
                                <div>
                                    <span className="recipe-details__extra-label">
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

export default RecipeDetails;
