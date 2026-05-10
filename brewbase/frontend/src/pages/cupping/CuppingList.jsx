import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getTastingSessions } from "../../api/tastingSessionsApi";
import "../../styles/CuppingList.css";

const CuppingList = () => {
    const navigate = useNavigate();

    const [sessions, setSessions] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadSessions = async () => {
            try {
                const data = await getTastingSessions();
                setSessions(data);
            } catch (error) {
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

    if (isLoading) {
        return (
            <div className="cupping-container">
                <h1 className="cupping-title">Cupping session</h1>
                <p className="empty-text">Ładowanie sesji...</p>
            </div>
        );
    }

    return (
        <div className="cupping-container">
            <h1 className="cupping-title">
                Cupping session
            </h1>

            <button
                className="add-session-btn"
                onClick={() => navigate("/cupping/new")}
            >
                + Dodaj kolejną sesję
            </button>

            {error && (
                <p className="empty-text">
                    {error}
                </p>
            )}

            <div className="cupping-list">
                {sessions.length === 0 && !error ? (
                    <p className="empty-text">
                        Brak zapisanych sesji
                    </p>
                ) : (
                    sessions.map((session) => (
                        <div
                            key={session.id}
                            className="cupping-card"
                        >
                            <div className="card-left">
                                <h3>
                                    {session.name}
                                </h3>

                                <p>
                                    Data:{" "}
                                    {formatDate(session.sessionDate ?? session.createdAt)}
                                </p>

                                <p>
                                    Ilość kaw:{" "}
                                    {session.coffeeCount ?? 0}
                                </p>
                            </div>

                            <div className="card-right">
                                <button
                                    className="details-btn"
                                    onClick={() =>
                                        navigate(`/cupping/preview/${session.id}`)
                                    }
                                >
                                    Szczegóły
                                </button>
                            </div>
                        </div>
                    ))
                )}
            </div>
        </div>
    );
};

export default CuppingList;