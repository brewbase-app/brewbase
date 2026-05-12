import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { getTastingSessionDetails } from "../../api/tastingSessionsApi";
import "../../styles/CuppingPreview.css";

const CuppingPreview = () => {
    const { id } = useParams();

    const [session, setSession] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadSession = async () => {
            try {
                const data = await getTastingSessionDetails(id);
                setSession(data);
            } catch (error) {
                setError("Nie udało się pobrać sesji.");
            } finally {
                setIsLoading(false);
            }
        };

        loadSession();
    }, [id]);

    const formatDate = (date) => {
        if (!date) {
            return "Brak daty";
        }

        return new Date(date).toLocaleDateString("pl-PL");
    };

    const formatCleanCup = (value) => {
        if (value === true) {
            return "Tak";
        }

        if (value === false) {
            return "Nie";
        }

        return "Brak";
    };

    if (isLoading) {
        return (
            <div className="preview-container">
                <h2>Ładowanie sesji...</h2>
            </div>
        );
    }

    if (error || !session) {
        return (
            <div className="preview-container">
                <h2>{error || "Nie znaleziono sesji"}</h2>
            </div>
        );
    }

    return (
        <div className="preview-container">
            <h1 className="preview-title">
                {session.name}
            </h1>

            <div className="session-info">
                <p>
                    <strong>Data:</strong>{" "}
                    {formatDate(session.sessionDate ?? session.createdAt)}
                </p>

                <p>
                    <strong>Ilość kaw:</strong>{" "}
                    {session.coffees.length}
                </p>
            </div>

            {session.coffees.map((coffee, index) => (
                <div
                    key={coffee.sessionCoffeeId}
                    className="preview-card"
                >
                    <h2>
                        Degustacja {index + 1}
                    </h2>

                    <h3>{coffee.coffeeName}</h3>

                    <div className="preview-grid">
                        <div>
                            <p>
                                <strong>Aroma:</strong>{" "}
                                {coffee.aromaScore ?? "Brak"}
                            </p>

                            <p>
                                <strong>Słodycz:</strong>{" "}
                                {coffee.sweetnessScore ?? "Brak"}
                            </p>

                            <p>
                                <strong>Kwasowość:</strong>{" "}
                                {coffee.acidityScore ?? "Brak"}
                            </p>

                            <p>
                                <strong>Body:</strong>{" "}
                                {coffee.bodyScore ?? "Brak"}
                            </p>

                            <p>
                                <strong>Ogólna ocena:</strong>{" "}
                                {coffee.overallScore ?? "Brak"}
                            </p>
                        </div>

                        <div>
                            <p>
                                <strong>Profile smakowe:</strong>
                            </p>

                            <p>
                                {coffee.flavorProfileNotes || "Brak"}
                            </p>

                            <p>
                                <strong>Komentarz:</strong>
                            </p>

                            <p>
                                {coffee.notes || "Brak"}
                            </p>

                            <p>
                                <strong>Czysta filiżanka:</strong>{" "}
                                {formatCleanCup(coffee.cleanCup)}
                            </p>
                        </div>
                    </div>
                </div>
            ))}
        </div>
    );
};

export default CuppingPreview;