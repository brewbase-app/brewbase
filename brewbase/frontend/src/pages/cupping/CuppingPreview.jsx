import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Coffee, Clock3, ChevronRight, Trash2 } from "lucide-react";
import {
    deleteCuppingSession,
    getCuppingSessionDetails
} from "../../api/cuppingSessionsApi";
import "../../styles/cupping/CuppingPreview.css";

const CuppingPreview = () => {
    const { id } = useParams();
    const navigate = useNavigate();

    const [session, setSession] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [isDeleting, setIsDeleting] = useState(false);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadSession = async () => {
            try {
                const data = await getCuppingSessionDetails(id);
                setSession(data);
            } catch {
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

    if (isLoading) {
        return (
            <div className="preview-container">
                <div className="preview-inner">
                    <div className="preview-header">
                        <h1 className="preview-title">Cupping session</h1>
                        <p className="preview-subtitle">Podgląd zapisanej sesji degustacji.</p>
                    </div>
                    <p className="loading-text">Ładowanie sesji...</p>
                </div>
            </div>
        );
    }

    if (error || !session) {
        return (
            <div className="preview-container">
                <div className="preview-inner">
                    <div className="preview-header">
                        <h1 className="preview-title">Cupping session</h1>
                        <p className="preview-subtitle">Podgląd zapisanej sesji degustacji.</p>
                    </div>
                    <p className="empty-text">{error || "Nie znaleziono sesji"}</p>
                </div>
            </div>
        );
    }

    return (
        <div className="preview-container">
            <div className="preview-inner">
                <div className="preview-header">
                    <h1 className="preview-title">{session.name}</h1>
                    <p className="preview-subtitle">Podsumowanie zapisanej sesji cupping.</p>
                </div>

                <div className="session-info">
                    <p>
                        <strong>Data:</strong>{" "}
                        {formatDate(session.sessionDate ?? session.createdAt)}
                    </p>

                    <p>
                        <strong>Ilość kaw:</strong>{" "}
                        {session.coffees.length}
                    </p>

                    {session.description && (
                        <p>
                            <strong>Opis:</strong>{" "}
                            {session.description}
                        </p>
                    )}
                </div>

                <div className="preview-list">
                    {session.coffees.map((coffee, index) => (
                        <div
                            key={coffee.sessionCoffeeId}
                            className="preview-card"
                        >
                            <div className="preview-card-header">
                                <div className="preview-card-icon">
                                    <Coffee size={22} />
                                </div>

                                <div>
                                    <h2>Degustacja {index + 1}</h2>
                                    <h3>{coffee.coffeeName}</h3>
                                </div>
                            </div>

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
                                        <strong>Profile smakowe:</strong>{" "}
                                        {coffee.flavorProfileNotes || "Brak"}
                                    </p>

                                    <p>
                                        <strong>Komentarz:</strong>{" "}
                                        {coffee.notes || "Brak"}
                                    </p>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>

                <div className="preview-actions">
                    <button
                        type="button"
                        className="preview-btn-secondary"
                        onClick={() => navigate("/cupping")}
                    >
                        <Clock3 size={18} />
                        Lista sesji
                    </button>

                    <button
                        type="button"
                        className="preview-btn-secondary"
                        onClick={handleDeleteSession}
                        disabled={isDeleting}
                    >
                        <Trash2 size={18} />
                        {isDeleting ? "Usuwanie..." : "Usuń sesję"}
                    </button>

                    <button
                        type="button"
                        className="preview-btn-primary"
                        onClick={() => navigate(`/cupping/${id}`)}
                    >
                        Edytuj sesję
                        <ChevronRight size={18} />
                    </button>
                </div>
            </div>
        </div>
    );
};

export default CuppingPreview;
