import { useNavigate } from "react-router-dom";
import "../../styles/CuppingList.css";

const mockSessions = [
    { id: 1, name: "Testowanie kawki 1", date: "01-02-2026", coffees: 5 },
    { id: 2, name: "Testowanie kawki 2", date: "03-02-2026", coffees: 4 },
    { id: 3, name: "Testowanie kawki 3", date: "02-02-2026", coffees: 8 },
];

const CuppingList = () => {
    const navigate = useNavigate();

    return (
        <div className="cupping-container">
            <h1 className="cupping-title">Cupping session</h1>

            <button
                className="add-session-btn"
                onClick={() => navigate("/cupping/new")}
            >
                + Dodaj kolejną sesję
            </button>

            <div className="cupping-list">
                {mockSessions.map((session) => (
                    <div key={session.id} className="cupping-card">
                        <div className="card-left">
                            <h3>{session.name}</h3>
                            <p>Data: {session.date}</p>
                            <p>Ilość kaw: {session.coffees}</p>
                        </div>

                        <div className="card-right">
                            <button
                                className="details-btn"
                                onClick={() =>
                                    navigate(`/cupping/${session.id}`)
                                }
                            >
                                Szczegóły
                            </button>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
};

export default CuppingList;