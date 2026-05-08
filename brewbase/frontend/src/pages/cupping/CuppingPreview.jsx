import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import "../../styles/CuppingPreview.css";

const CuppingPreview = () => {
    const { id } = useParams();

    const [session, setSession] = useState(null);

    useEffect(() => {
        const savedSessions =
            JSON.parse(
                localStorage.getItem("cuppingSessions")
            ) || [];

        const foundSession = savedSessions.find(
            (s) => s.id.toString() === id
        );

        setSession(foundSession);
    }, [id]);

    if (!session) {
        return (
            <div className="preview-container">
                <h2>Nie znaleziono sesji</h2>
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
                    {session.date}
                </p>

                <p>
                    <strong>Ilość kaw:</strong>{" "}
                    {session.cuppings.length}
                </p>
            </div>

            {session.cuppings.map((cup, index) => (
                <div
                    key={cup.id}
                    className="preview-card"
                >
                    <h2>
                        Degustacja {index + 1}
                    </h2>

                    <h3>{cup.coffeeName}</h3>

                    <div className="preview-grid">
                        <div>
                            <p>
                                <strong>Aroma:</strong>{" "}
                                {cup.aroma}
                            </p>

                            <p>
                                <strong>Słodycz:</strong>{" "}
                                {cup.sweetness}
                            </p>

                            <p>
                                <strong>Kwasowość:</strong>{" "}
                                {cup.acidity}
                            </p>

                            <p>
                                <strong>Body:</strong>{" "}
                                {cup.body}
                            </p>

                            <p>
                                <strong>
                                    Ogólna ocena:
                                </strong>{" "}
                                {cup.overall}
                            </p>
                        </div>

                        <div>
                            <p>
                                <strong>
                                    Profile smakowe:
                                </strong>
                            </p>

                            <p>
                                {cup.flavorNotes}
                            </p>

                            <p>
                                <strong>
                                    Komentarz:
                                </strong>
                            </p>

                            <p>
                                {cup.comments}
                            </p>

                            <p>
                                <strong>
                                    Czysta filiżanka:
                                </strong>{" "}
                                {cup.cleanCup}
                            </p>
                        </div>
                    </div>
                </div>
            ))}
        </div>
    );
};

export default CuppingPreview;