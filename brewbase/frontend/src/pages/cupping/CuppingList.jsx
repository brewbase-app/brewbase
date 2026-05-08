import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import "../../styles/CuppingList.css";

const CuppingList = () => {
    const navigate = useNavigate();

    const [sessions, setSessions] = useState([]);

    useEffect(() => {
        const savedSessions =
            JSON.parse(
                localStorage.getItem("cuppingSessions")
            ) || [];

        setSessions(savedSessions);
    }, []);

    return (
        <div className="cupping-container">
            <h1 className="cupping-title">
                Cupping session
            </h1>

            <button
                className="add-session-btn"
                onClick={() =>
                    navigate("/cupping/new")
                }
            >
                + Dodaj kolejną sesję
            </button>

            <div className="cupping-list">
                {sessions.length === 0 ? (
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
                                    {session.date}
                                </p>

                                <p>
                                    Ilość kaw:{" "}
                                    {
                                        session
                                            .cuppings
                                            .length
                                    }
                                </p>
                            </div>

                            <div className="card-right">
                                <button
                                    className="details-btn"
                                    onClick={() =>
                                        navigate(
                                            `/cupping/preview/${session.id}`
                                        )
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
    