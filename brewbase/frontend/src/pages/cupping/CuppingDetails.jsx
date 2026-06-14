import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Plus, Trash2 } from "lucide-react";
import {
    addCoffeeToCuppingSession,
    deleteCuppingSession,
    deleteCuppingSessionCoffee,
    getCuppingSessionDetails,
    updateCuppingSession,
    updateCuppingSessionCoffee,
} from "../../api/cuppingSessionsApi";
import { getCoffees } from "../../api/coffeeApi";
import "../../styles/cupping/CuppingDetails.css";
import { sortByName } from "../../utils/sortOptions";

const createEmptyCupping = () => ({
    rowId: `new-${Date.now()}-${Math.random()}`,
    sessionCoffeeId: null,
    selectedCoffeeId: "",
    customCoffeeName: "",
    coffeeName: "",
    aromaScore: "",
    sweetnessScore: "",
    acidityScore: "",
    bodyScore: "",
    overallScore: "",
    flavorProfileNotes: "",
    notes: "",
    isNew: true,
});

const normalizeCoffeeList = (data) => {
    if (Array.isArray(data)) {
        return data;
    }

    if (Array.isArray(data.items)) {
        return data.items;
    }

    if (Array.isArray(data.data)) {
        return data.data;
    }

    return [];
};

const formatDateInput = (date) => {
    if (!date) {
        return "";
    }

    const parsed = new Date(date);

    if (Number.isNaN(parsed.getTime())) {
        return "";
    }

    return parsed.toISOString().slice(0, 10);
};

const CuppingDetails = () => {
    const navigate = useNavigate();
    const { id } = useParams();

    const [sessionForm, setSessionForm] = useState({
        name: "",
        date: "",
        description: "",
    });
    const [availableCoffees, setAvailableCoffees] = useState([]);
    const [cuppings, setCuppings] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isSaving, setIsSaving] = useState(false);
    const [isDeleting, setIsDeleting] = useState(false);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadSession = async () => {
            try {
                const [sessionData, coffeesData] = await Promise.all([
                    getCuppingSessionDetails(id),
                    getCoffees(),
                ]);

                setSessionForm({
                    name: sessionData.name ?? "",
                    date: formatDateInput(sessionData.sessionDate ?? sessionData.createdAt),
                    description: sessionData.description ?? "",
                });
                setAvailableCoffees(sortByName(normalizeCoffeeList(coffeesData)));
                
                setCuppings(
                    sessionData.coffees.length > 0
                        ? sessionData.coffees.map((coffee) => ({
                            rowId: String(coffee.sessionCoffeeId),
                            sessionCoffeeId: coffee.sessionCoffeeId,
                            selectedCoffeeId: coffee.coffeeId ?? "",
                            customCoffeeName: coffee.coffeeId ? "" : coffee.coffeeName,
                            coffeeName: coffee.coffeeName,
                            aromaScore: coffee.aromaScore ?? "",
                            sweetnessScore: coffee.sweetnessScore ?? "",
                            acidityScore: coffee.acidityScore ?? "",
                            bodyScore: coffee.bodyScore ?? "",
                            overallScore: coffee.overallScore ?? "",
                            flavorProfileNotes: coffee.flavorProfileNotes ?? "",
                            notes: coffee.notes ?? "",
                            isNew: false,
                        }))
                        : [createEmptyCupping()]
                );
            } catch {
                setError("Nie udało się pobrać szczegółów sesji.");
            } finally {
                setIsLoading(false);
            }
        };

        loadSession();
    }, [id]);

    const handleSessionChange = (event) => {
        setSessionForm((current) => ({
            ...current,
            [event.target.name]: event.target.value,
        }));
    };

    const addCupping = () => {
        setCuppings((prev) => [...prev, createEmptyCupping()]);
    };

    const handleChange = (rowId, field, value) => {
        setCuppings((prev) =>
            prev.map((cup) =>
                cup.rowId === rowId
                    ? { ...cup, [field]: value }
                    : cup
            )
        );
    };

    const handleSelectedCoffeeChange = (rowId, value) => {
        setCuppings((prev) =>
            prev.map((cup) =>
                cup.rowId === rowId
                    ? {
                        ...cup,
                        selectedCoffeeId: value,
                        customCoffeeName: value ? "" : cup.customCoffeeName,
                    }
                    : cup
            )
        );
    };

    const handleCustomCoffeeNameChange = (rowId, value) => {
        setCuppings((prev) =>
            prev.map((cup) =>
                cup.rowId === rowId
                    ? {
                        ...cup,
                        customCoffeeName: value,
                        selectedCoffeeId: value.trim() ? "" : cup.selectedCoffeeId,
                    }
                    : cup
            )
        );
    };

    const handleRemoveCupping = async (cup) => {
        setError("");

        if (cup.isNew) {
            setCuppings((prev) => {
                const next = prev.filter((item) => item.rowId !== cup.rowId);
                return next.length > 0 ? next : [createEmptyCupping()];
            });
            return;
        }

        const confirmed = window.confirm("Czy na pewno chcesz usunąć tę degustację?");

        if (!confirmed) {
            return;
        }

        try {
            await deleteCuppingSessionCoffee(id, cup.sessionCoffeeId);
            setCuppings((prev) => {
                const next = prev.filter((item) => item.rowId !== cup.rowId);
                return next.length > 0 ? next : [createEmptyCupping()];
            });
        } catch {
            setError("Nie udało się usunąć degustacji.");
        }
    };

    const handleDeleteSession = async () => {
        const confirmed = window.confirm("Czy na pewno chcesz usunąć całą sesję cupping?");

        if (!confirmed || isDeleting) {
            return;
        }

        setError("");
        setIsDeleting(true);

        try {
            await deleteCuppingSession(id);
            navigate("/cupping");
        } catch {
            setError("Nie udało się usunąć sesji.");
        } finally {
            setIsDeleting(false);
        }
    };

    const toNullableNumber = (value) => {
        if (value === "" || value === null || value === undefined) {
            return null;
        }

        return Number(value);
    };

    const toNullableText = (value) => {
        if (!value || !value.trim()) {
            return null;
        }

        return value.trim();
    };

    const validateCup = (cup) => {
        if (cup.isNew) {
            const hasSelectedCoffee = cup.selectedCoffeeId !== "";
            const hasCustomCoffee = cup.customCoffeeName && cup.customCoffeeName.trim();

            if (!hasSelectedCoffee && !hasCustomCoffee) {
                throw new Error("Wybierz kawę z listy albo wpisz własną nazwę kawy.");
            }
        }

        const scoreFields = [
            cup.aromaScore,
            cup.sweetnessScore,
            cup.acidityScore,
            cup.bodyScore,
            cup.overallScore,
        ];

        const hasInvalidScore = scoreFields.some((value) => {
            if (value === "" || value === null || value === undefined) {
                return false;
            }

            const numberValue = Number(value);

            return numberValue < 1 || numberValue > 10;
        });

        if (hasInvalidScore) {
            throw new Error("Oceny muszą być w zakresie od 1 do 10.");
        }
    };

    const buildAddPayload = (cup) => {
        if (cup.selectedCoffeeId) {
            return {
                coffeeId: Number(cup.selectedCoffeeId),
                notes: toNullableText(cup.notes),
            };
        }

        return {
            coffeeName: cup.customCoffeeName.trim(),
            notes: toNullableText(cup.notes),
        };
    };

    const buildUpdatePayload = (cup) => ({
        notes: toNullableText(cup.notes),
        aromaScore: toNullableNumber(cup.aromaScore),
        sweetnessScore: toNullableNumber(cup.sweetnessScore),
        acidityScore: toNullableNumber(cup.acidityScore),
        bodyScore: toNullableNumber(cup.bodyScore),
        flavorProfileNotes: toNullableText(cup.flavorProfileNotes),
        overallScore: toNullableNumber(cup.overallScore),
    });

    const handleSave = async () => {
        setError("");

        if (!sessionForm.name.trim()) {
            setError("Nazwa sesji jest wymagana.");
            return;
        }

        try {
            setIsSaving(true);

            await updateCuppingSession(id, {
                name: sessionForm.name.trim(),
                description: sessionForm.description || null,
                sessionDate: sessionForm.date ? `${sessionForm.date}T00:00:00` : null,
            });

            for (const cup of cuppings) {
                validateCup(cup);

                let sessionCoffeeId = cup.sessionCoffeeId;

                if (cup.isNew) {
                    const createdCoffee = await addCoffeeToCuppingSession(
                        id,
                        buildAddPayload(cup)
                    );

                    sessionCoffeeId = createdCoffee.sessionCoffeeId;

                    setCuppings((prev) =>
                        prev.map((item) =>
                            item.rowId === cup.rowId
                                ? {
                                    ...item,
                                    rowId: String(createdCoffee.sessionCoffeeId),
                                    sessionCoffeeId: createdCoffee.sessionCoffeeId,
                                    coffeeId: createdCoffee.coffeeId ?? "",
                                    coffeeName: createdCoffee.coffeeName,
                                    isNew: false,
                                }
                                : item
                        )
                    );
                }

                await updateCuppingSessionCoffee(
                    id,
                    sessionCoffeeId,
                    buildUpdatePayload(cup)
                );
            }

            navigate(`/cupping/preview/${id}`);
        } catch (saveError) {
            setError(saveError.message || "Nie udało się zapisać sesji.");
        } finally {
            setIsSaving(false);
        }
    };

    if (isLoading) {
        return (
            <div className="details-container">
                <div className="details-inner">
                    <div className="details-header">
                        <h1 className="title">Sesja Cupping</h1>
                        <p className="details-subtitle">Dodaj kawy i uzupełnij oceny degustacji.</p>
                    </div>
                    <p className="loading-text">Ładowanie sesji...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="details-container">
            <div className="details-inner">
                <div className="details-header">
                    <h1 className="title">Sesja Cupping</h1>
                    <p className="details-subtitle">
                        Dodaj kawy do sesji i uzupełnij oceny degustacji.
                    </p>
                </div>

                {error && (
                    <p className="error-text">{error}</p>
                )}

                <div className="session-form-card">
                    <div className="coffee-inputs">
                        <div className="coffee-field">
                            <label htmlFor="session-name">Nazwa sesji</label>
                            <input
                                id="session-name"
                                name="name"
                                className="coffee-input"
                                value={sessionForm.name}
                                onChange={handleSessionChange}
                            />
                        </div>

                        <div className="coffee-field">
                            <label htmlFor="session-date">Data sesji</label>
                            <input
                                id="session-date"
                                name="date"
                                type="date"
                                className="coffee-input"
                                value={sessionForm.date}
                                onChange={handleSessionChange}
                            />
                        </div>

                        <div className="coffee-field coffee-field-full">
                            <label htmlFor="session-description">Opis</label>
                            <textarea
                                id="session-description"
                                name="description"
                                className="coffee-input coffee-textarea"
                                value={sessionForm.description}
                                onChange={handleSessionChange}
                            />
                        </div>
                    </div>
                </div>

                {cuppings.map((cup, index) => (
                    <div
                        key={cup.rowId}
                        className="cupping-block"
                    >
                        <div className="block-header">
                            <h2 className="subtitle">
                                Degustacja {index + 1}
                            </h2>

                            <button
                                type="button"
                                className="remove-cup-btn"
                                onClick={() => handleRemoveCupping(cup)}
                            >
                                <Trash2 size={16} />
                                Usuń
                            </button>
                        </div>

                        {cup.isNew ? (
                            <div className="coffee-inputs">
                                <div className="coffee-field">
                                    <label htmlFor={`coffee-select-${cup.rowId}`}>
                                        Kawa z bazy
                                    </label>
                                    <select
                                        id={`coffee-select-${cup.rowId}`}
                                        className="coffee-input"
                                        value={cup.selectedCoffeeId}
                                        onChange={(event) =>
                                            handleSelectedCoffeeChange(
                                                cup.rowId,
                                                event.target.value
                                            )
                                        }
                                    >
                                        <option value="">
                                            Wybierz kawę z bazy
                                        </option>

                                        {availableCoffees.map((coffee) => (
                                            <option
                                                key={coffee.id}
                                                value={coffee.id}
                                            >
                                                {coffee.name}
                                            </option>
                                        ))}
                                    </select>
                                </div>

                                <div className="coffee-field">
                                    <label htmlFor={`coffee-custom-${cup.rowId}`}>
                                        Własna nazwa kawy
                                    </label>
                                    <input
                                        id={`coffee-custom-${cup.rowId}`}
                                        className="coffee-input"
                                        type="text"
                                        placeholder="albo wpisz własną kawę"
                                        value={cup.customCoffeeName}
                                        disabled={!!cup.selectedCoffeeId}
                                        onChange={(event) =>
                                            handleCustomCoffeeNameChange(
                                                cup.rowId,
                                                event.target.value
                                            )
                                        }
                                    />
                                </div>
                            </div>
                        ) : (
                            <p className="coffee-label">
                                Kawa: {cup.coffeeName}
                            </p>
                        )}

                        <div className="grid">
                            <div className="box">
                                <h3>Aroma (1-10)</h3>
                                <input
                                    type="number"
                                    min="1"
                                    max="10"
                                    value={cup.aromaScore}
                                    onChange={(event) =>
                                        handleChange(
                                            cup.rowId,
                                            "aromaScore",
                                            event.target.value
                                        )
                                    }
                                />
                            </div>

                            <div className="box">
                                <h3>Słodycz (1-10)</h3>
                                <input
                                    type="number"
                                    min="1"
                                    max="10"
                                    value={cup.sweetnessScore}
                                    onChange={(event) =>
                                        handleChange(
                                            cup.rowId,
                                            "sweetnessScore",
                                            event.target.value
                                        )
                                    }
                                />
                            </div>

                            <div className="box">
                                <h3>Kwasowość (1-10)</h3>
                                <input
                                    type="number"
                                    min="1"
                                    max="10"
                                    value={cup.acidityScore}
                                    onChange={(event) =>
                                        handleChange(
                                            cup.rowId,
                                            "acidityScore",
                                            event.target.value
                                        )
                                    }
                                />
                            </div>

                            <div className="box">
                                <h3>Body (1-10)</h3>
                                <input
                                    type="number"
                                    min="1"
                                    max="10"
                                    value={cup.bodyScore}
                                    onChange={(event) =>
                                        handleChange(
                                            cup.rowId,
                                            "bodyScore",
                                            event.target.value
                                        )
                                    }
                                />
                            </div>

                            <div className="box">
                                <h3>Ogólna ocena (1-10)</h3>
                                <input
                                    type="number"
                                    min="1"
                                    max="10"
                                    value={cup.overallScore}
                                    onChange={(event) =>
                                        handleChange(
                                            cup.rowId,
                                            "overallScore",
                                            event.target.value
                                        )
                                    }
                                />
                            </div>

                            <div className="box box-full">
                                <h3>Profile smakowe</h3>
                                <textarea
                                    placeholder="Notatki smakowe"
                                    value={cup.flavorProfileNotes}
                                    onChange={(event) =>
                                        handleChange(
                                            cup.rowId,
                                            "flavorProfileNotes",
                                            event.target.value
                                        )
                                    }
                                />
                            </div>

                            <div className="box box-full">
                                <h3>Dodatkowy komentarz</h3>
                                <textarea
                                    value={cup.notes}
                                    onChange={(event) =>
                                        handleChange(
                                            cup.rowId,
                                            "notes",
                                            event.target.value
                                        )
                                    }
                                />
                            </div>
                        </div>
                    </div>
                ))}

                <div className="details-actions">
                    <button
                        type="button"
                        className="add-cup-btn"
                        onClick={addCupping}
                    >
                        <Plus size={18} />
                        Dodaj kolejną degustację
                    </button>

                    <button
                        type="button"
                        className="delete-session-btn"
                        onClick={handleDeleteSession}
                        disabled={isDeleting || isSaving}
                    >
                        <Trash2 size={16} />
                        {isDeleting ? "Usuwanie..." : "Usuń sesję"}
                    </button>

                    <button
                        type="button"
                        className="save-btn"
                        onClick={handleSave}
                        disabled={isSaving || isDeleting}
                    >
                        {isSaving ? "Zapisywanie..." : "Zapisz sesję"}
                    </button>
                </div>
            </div>
        </div>
    );
};

export default CuppingDetails;
