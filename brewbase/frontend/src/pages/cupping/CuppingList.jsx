import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
    Coffee,
    Clock3,
    ChevronRight,
    Plus,
    Trash2
} from "lucide-react";
import {
    deleteCuppingSession,
    getCuppingSessions
} from "../../api/cuppingSessionsApi";
import "../../styles/CuppingList.css";

const CuppingList = () => {
    const navigate = useNavigate();

    const [sessions, setSessions] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadSessions = async () => {
            try {
                const data = await getCuppingSessions();
                setSessions(data);
            } catch {
                setError("Nie udało się pobrać sesji cuppingowych.");
            } finally {
                setIsLoading(false);
            }
        };

        loadSessions();
    }, []);

    const formatDate = (date) => {
        if (!date) {
            return "Brak daty";
        }

        return new Date(date).toLocaleDateString("pl-PL");
    };

    const handleDeleteSession = async (sessionId, sessionName) => {
        const confirmed = window.confirm(
            `Czy na pewno chcesz usunąć sesję „${sessionName}”?`
        );

        if (!confirmed) {
            return;
        }

        try {
            await deleteCuppingSession(sessionId);
            setSessions((current) =>
                current.filter((session) => session.id !== sessionId)
            );
        } catch {
            setError("Nie udało się usunąć sesji.");
        }
    };

    if (isLoading) {
        return (
            <div className="cupping-container">
                <div className="cupping-inner">
                    <div className="cupping-header">
                        <h1 className="cupping-title">Cupping session</h1>
                        <p className="cupping-subtitle">Twoje sesje degustacji kaw.</p>
                    </div>
                    <p className="empty-text">Ładowanie sesji...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="cupping-container">
            <div className="cupping-inner">
                <div className="cupping-header">
                    <h1 className="cupping-title">Cupping session</h1>
                    <p className="cupping-subtitle">
                        Twórz sesje degustacji i zapisuj oceny kaw.
                    </p>
                </div>

                <div className="cupping-actions">
                    <button
                        type="button"
                        className="add-session-btn"
                        onClick={() => navigate("/cupping/new")}
                    >
                        <Plus size={18} />
                        Dodaj kolejną sesję
                    </button>
                </div>

                {error && (
                    <p className="error-text">{error}</p>
                )}

                <div className="cupping-list">
                    {sessions.length === 0 && !error ? (
                        <p className="empty-text">
                            Brak zapisanych sesji. Kliknij „Dodaj kolejną sesję”, aby rozpocząć pierwszą degustację.
                        </p>
                    ) : (
                        sessions.map((session) => (
                            <div
                                key={session.id}
                                className="cupping-card"
                            >
                                <div className="card-left">
                                    <div className="card-icon">
                                        <Coffee size={22} />
                                    </div>

                                    <div className="card-content">
                                        <h3>{session.name}</h3>

                                        <div className="card-meta">
                                            <span>
                                                <Clock3 size={14} />
                                                {formatDate(session.sessionDate ?? session.createdAt)}
                                            </span>

                                            <span>
                                                Ilość kaw: {session.coffeeCount ?? 0}
                                            </span>
                                        </div>
                                    </div>
                                </div>

                                <div className="card-actions">
                                    <button
                                        type="button"
                                        className="details-btn"
                                        onClick={() =>
                                            navigate(`/cupping/preview/${session.id}`)
                                        }
                                    >
                                        Szczegóły
                                        <ChevronRight size={18} />
                                    </button>

                                    <button
                                        type="button"
                                        className="delete-btn"
                                        onClick={() =>
                                            handleDeleteSession(session.id, session.name)
                                        }
                                    >
                                        <Trash2 size={16} />
                                        Usuń
                                    </button>
                                </div>
                            </div>
                        ))
                    )}
                </div>
            </div>
        </div>
    );
};

export default CuppingList;
