import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
    addCoffeeToTastingSession,
    getTastingSessionDetails,
    updateTastingSessionCoffee,
} from "../../api/tastingSessionsApi";
import "../../styles/CuppingDetails.css";

const createEmptyCupping = () => ({
    rowId: `new-${Date.now()}-${Math.random()}`,
    coffeeId: "",
    coffeeName: "",
    aromaScore: "",
    sweetnessScore: "",
    acidityScore: "",
    bodyScore: "",
    overallScore: "",
    flavorProfileNotes: "",
    notes: "",
    cleanCup: "",
    isNew: true,
});

const CuppingDetails = () => {
    const navigate = useNavigate();
    const { id } = useParams();

    const [session, setSession] = useState(null);
    const [cuppings, setCuppings] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isSaving, setIsSaving] = useState(false);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadSession = async () => {
            try {
                const data = await getTastingSessionDetails(id);

                setSession(data);

                setCuppings(
                    data.coffees.map((coffee) => ({
                        rowId: String(coffee.coffeeId),
                        coffeeId: coffee.coffeeId,
                        coffeeName: coffee.coffeeName,
                        aromaScore: coffee.aromaScore ?? "",
                        sweetnessScore: coffee.sweetnessScore ?? "",
                        acidityScore: coffee.acidityScore ?? "",
                        bodyScore: coffee.bodyScore ?? "",
                        overallScore: coffee.overallScore ?? "",
                        flavorProfileNotes: coffee.flavorProfileNotes ?? "",
                        notes: coffee.notes ?? "",
                        cleanCup:
                            coffee.cleanCup === null || coffee.cleanCup === undefined
                                ? ""
                                : coffee.cleanCup
                                    ? "tak"
                                    : "nie",
                        isNew: false,
                    }))
                );
            } catch (error) {
                setError("Nie udało się pobrać szczegółów sesji.");
            } finally {
                setIsLoading(false);
            }
        };

        loadSession();
    }, [id]);

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

    const toNullableBoolean = (value) => {
        if (value === "tak") {
            return true;
        }

        if (value === "nie") {
            return false;
        }

        return null;
    };

    const validateCup = (cup) => {
        if (!cup.coffeeId) {
            throw new Error("Uzupełnij ID kawy.");
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

    const buildUpdatePayload = (cup) => ({
        notes: toNullableText(cup.notes),
        aromaScore: toNullableNumber(cup.aromaScore),
        sweetnessScore: toNullableNumber(cup.sweetnessScore),
        acidityScore: toNullableNumber(cup.acidityScore),
        bodyScore: toNullableNumber(cup.bodyScore),
        flavorProfileNotes: toNullableText(cup.flavorProfileNotes),
        cleanCup: toNullableBoolean(cup.cleanCup),
        overallScore: toNullableNumber(cup.overallScore),
    });

    const handleSave = async () => {
        setError("");

        try {
            setIsSaving(true);

            for (const cup of cuppings) {
                validateCup(cup);

                const coffeeId = Number(cup.coffeeId);

                if (cup.isNew) {
                    await addCoffeeToTastingSession(id, {
                        coffeeId: coffeeId,
                        notes: toNullableText(cup.notes),
                    });
                }

                await updateTastingSessionCoffee(
                    id,
                    coffeeId,
                    buildUpdatePayload(cup)
                );
            }

            navigate(`/cupping/preview/${id}`);
        } catch (error) {
            setError(error.message || "Nie udało się zapisać sesji.");
        } finally {
            setIsSaving(false);
        }
    };

    if (isLoading) {
        return (
            <div className="details-container">
                <h1 className="title">Cupping session</h1>
                <p>Ładowanie sesji...</p>
            </div>
        );
    }

    return (
        <div className="details-container">
            <h1 className="title">
                {session?.name ?? "Cupping session"}
            </h1>

            {session?.description && (
                <p>{session.description}</p>
            )}

            {error && (
                <p className="error-text">
                    {error}
                </p>
            )}

            {cuppings.map((cup, index) => (
                <div
                    key={cup.rowId}
                    className="cupping-block"
                >
                    <h2 className="subtitle">
                        Degustacja {index + 1}
                    </h2>

                    <input
                        className="coffee-input"
                        type="number"
                        placeholder="wprowadź ID kawy"
                        value={cup.coffeeId}
                        disabled={!cup.isNew}
                        onChange={(e) =>
                            handleChange(
                                cup.rowId,
                                "coffeeId",
                                e.target.value
                            )
                        }
                    />

                    {cup.coffeeName && (
                        <p>
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
                                onChange={(e) =>
                                    handleChange(
                                        cup.rowId,
                                        "aromaScore",
                                        e.target.value
                                    )
                                }
                            />
                        </div>

                        <div className="box">
                            <h3>Profile smakowe</h3>

                            <textarea
                                placeholder="Notatki smakowe"
                                value={cup.flavorProfileNotes}
                                onChange={(e) =>
                                    handleChange(
                                        cup.rowId,
                                        "flavorProfileNotes",
                                        e.target.value
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
                                onChange={(e) =>
                                    handleChange(
                                        cup.rowId,
                                        "sweetnessScore",
                                        e.target.value
                                    )
                                }
                            />
                        </div>

                        <div className="box">
                            <h3>Czysta filiżanka</h3>

                            <div className="checkbox-group">
                                <label>
                                    <input
                                        type="radio"
                                        name={`cleanCup-${cup.rowId}`}
                                        checked={cup.cleanCup === "tak"}
                                        onChange={() =>
                                            handleChange(
                                                cup.rowId,
                                                "cleanCup",
                                                "tak"
                                            )
                                        }
                                    />
                                    Tak
                                </label>

                                <label>
                                    <input
                                        type="radio"
                                        name={`cleanCup-${cup.rowId}`}
                                        checked={cup.cleanCup === "nie"}
                                        onChange={() =>
                                            handleChange(
                                                cup.rowId,
                                                "cleanCup",
                                                "nie"
                                            )
                                        }
                                    />
                                    Nie
                                </label>
                            </div>
                        </div>

                        <div className="box">
                            <h3>Kwasowość (1-10)</h3>

                            <input
                                type="number"
                                min="1"
                                max="10"
                                value={cup.acidityScore}
                                onChange={(e) =>
                                    handleChange(
                                        cup.rowId,
                                        "acidityScore",
                                        e.target.value
                                    )
                                }
                            />
                        </div>

                        <div className="box">
                            <h3>Dodatkowy komentarz</h3>

                            <textarea
                                value={cup.notes}
                                onChange={(e) =>
                                    handleChange(
                                        cup.rowId,
                                        "notes",
                                        e.target.value
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
                                onChange={(e) =>
                                    handleChange(
                                        cup.rowId,
                                        "bodyScore",
                                        e.target.value
                                    )
                                }
                            />
                        </div>

                        <div className="box">
                            <h3>Ogólna ocena</h3>

                            <input
                                type="number"
                                min="1"
                                max="10"
                                value={cup.overallScore}
                                onChange={(e) =>
                                    handleChange(
                                        cup.rowId,
                                        "overallScore",
                                        e.target.value
                                    )
                                }
                            />
                        </div>
                    </div>
                </div>
            ))}

            <button
                className="add-cup-btn"
                onClick={addCupping}
            >
                + Dodaj kolejną degustację
            </button>

            <button
                className="save-btn"
                onClick={handleSave}
                disabled={isSaving}
            >
                {isSaving ? "Zapisywanie..." : "Zapisz sesję"}
            </button>
        </div>
    );
};

export default CuppingDetails;