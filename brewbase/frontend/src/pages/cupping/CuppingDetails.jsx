import { useState } from "react";
import { useNavigate } from "react-router-dom";
import "../../styles/CuppingDetails.css";

const CuppingDetails = () => {
    const navigate = useNavigate();

    const [cuppings, setCuppings] = useState([
        {
            id: 1,
            coffeeName: "",
            aroma: "",
            sweetness: "",
            acidity: "",
            body: "",
            overall: "",
            flavorNotes: "",
            comments: "",
            cleanCup: "",
        },
    ]);

    const addCupping = () => {
        setCuppings([
            ...cuppings,
            {
                id: cuppings.length + 1,
                coffeeName: "",
                aroma: "",
                sweetness: "",
                acidity: "",
                body: "",
                overall: "",
                flavorNotes: "",
                comments: "",
                cleanCup: "",
            },
        ]);
    };

    const handleChange = (id, field, value) => {
        setCuppings((prev) =>
            prev.map((cup) =>
                cup.id === id
                    ? { ...cup, [field]: value }
                    : cup
            )
        );
    };

    const handleSave = () => {
        const existingSessions =
            JSON.parse(
                localStorage.getItem("cuppingSessions")
            ) || [];

        const newSession = {
            id: Date.now(),
            name: `Cupping Session ${existingSessions.length + 1}`,
            date: new Date().toLocaleDateString(),
            cuppings: cuppings,
        };

        const updatedSessions = [
            ...existingSessions,
            newSession,
        ];

        localStorage.setItem(
            "cuppingSessions",
            JSON.stringify(updatedSessions)
        );

        console.log("Zapisano sesję:", newSession);

        navigate("/cupping");
    };

    return (
        <div className="details-container">
            <h1 className="title">
                Cupping session
            </h1>

            {cuppings.map((cup, index) => (
                <div
                    key={cup.id}
                    className="cupping-block"
                >
                    <h2 className="subtitle">
                        Degustacja {index + 1}
                    </h2>

                    <input
                        className="coffee-input"
                        placeholder="wprowadź nazwę kawy"
                        value={cup.coffeeName}
                        onChange={(e) =>
                            handleChange(
                                cup.id,
                                "coffeeName",
                                e.target.value
                            )
                        }
                    />

                    <div className="grid">
                        <div className="box">
                            <h3>Aroma (1-10)</h3>

                            <input
                                type="number"
                                value={cup.aroma}
                                onChange={(e) =>
                                    handleChange(
                                        cup.id,
                                        "aroma",
                                        e.target.value
                                    )
                                }
                            />

                            <textarea
                                placeholder="Notatki"
                            />
                        </div>

                        <div className="box">
                            <h3>Profile smakowe</h3>

                            <textarea
                                placeholder="Notatki"
                                value={
                                    cup.flavorNotes
                                }
                                onChange={(e) =>
                                    handleChange(
                                        cup.id,
                                        "flavorNotes",
                                        e.target.value
                                    )
                                }
                            />
                        </div>

                        <div className="box">
                            <h3>Słodycz (1-10)</h3>

                            <input
                                type="number"
                                value={cup.sweetness}
                                onChange={(e) =>
                                    handleChange(
                                        cup.id,
                                        "sweetness",
                                        e.target.value
                                    )
                                }
                            />

                            <textarea
                                placeholder="Notatki"
                            />
                        </div>

                        <div className="box">
                            <h3>Czysta filiżanka</h3>

                            <div className="checkbox-group">
                                <label>
                                    <input
                                        type="radio"
                                        name={`cleanCup-${cup.id}`}
                                        checked={
                                            cup.cleanCup ===
                                            "tak"
                                        }
                                        onChange={() =>
                                            handleChange(
                                                cup.id,
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
                                        name={`cleanCup-${cup.id}`}
                                        checked={
                                            cup.cleanCup ===
                                            "nie"
                                        }
                                        onChange={() =>
                                            handleChange(
                                                cup.id,
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
                                value={cup.acidity}
                                onChange={(e) =>
                                    handleChange(
                                        cup.id,
                                        "acidity",
                                        e.target.value
                                    )
                                }
                            />

                            <textarea
                                placeholder="Notatki"
                            />
                        </div>

                        <div className="box">
                            <h3>Dodatkowy komentarz</h3>

                            <textarea
                                value={cup.comments}
                                onChange={(e) =>
                                    handleChange(
                                        cup.id,
                                        "comments",
                                        e.target.value
                                    )
                                }
                            />
                        </div>

                        <div className="box">
                            <h3>Body (1-10)</h3>

                            <input
                                type="number"
                                value={cup.body}
                                onChange={(e) =>
                                    handleChange(
                                        cup.id,
                                        "body",
                                        e.target.value
                                    )
                                }
                            />

                            <textarea
                                placeholder="Notatki"
                            />
                        </div>

                        <div className="box">
                            <h3>Ogólna ocena</h3>

                            <input
                                type="number"
                                value={cup.overall}
                                onChange={(e) =>
                                    handleChange(
                                        cup.id,
                                        "overall",
                                        e.target.value
                                    )
                                }
                            />

                            <textarea
                                placeholder="Notatki"
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
            >
                Zapisz sesję
            </button>
        </div>
    );
};

export default CuppingDetails;