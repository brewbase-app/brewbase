import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { createCuppingSession } from "../../api/cuppingSessionsApi";
import "../../styles/cupping/CreateCupping.css";

const CreateCupping = () => {
    const navigate = useNavigate();

    const [form, setForm] = useState({
        name: "",
        date: "",
        description: "",
    });
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState("");

    const handleChange = (event) => {
        setForm({
            ...form,
            [event.target.name]: event.target.value,
        });
    };

    const handleSubmit = async (event) => {
        event.preventDefault();

        if (isSubmitting) {
            return;
        }

        setError("");
        setIsSubmitting(true);

        try {
            const createdSession = await createCuppingSession({
                name: form.name,
                description: form.description || null,
                sessionDate: form.date ? `${form.date}T00:00:00` : null,
            });

            navigate(`/cupping/${createdSession.id}`);
        } catch (submitError) {
            setError(submitError.message || "Nie udało się utworzyć sesji.");
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="cupping-wrapper">
            <div className="cupping-inner">
                <div className="cupping-header">
                    <h1>Nowa sesja cupping</h1>
                    <p>Uzupełnij podstawowe informacje, aby rozpocząć degustację.</p>
                </div>

                <form className="cupping-box" onSubmit={handleSubmit}>
                    <div className="cupping-field">
                        <label htmlFor="cupping-name">Nazwa sesji</label>
                        <input
                            id="cupping-name"
                            name="name"
                            placeholder="Np. Porównanie etiopii"
                            value={form.name}
                            onChange={handleChange}
                            required
                        />
                    </div>

                    <div className="cupping-field">
                        <label htmlFor="cupping-date">Data sesji</label>
                        <input
                            id="cupping-date"
                            name="date"
                            type="date"
                            value={form.date}
                            onChange={handleChange}
                        />
                    </div>

                    <div className="cupping-field">
                        <label htmlFor="cupping-description">Opis</label>
                        <textarea
                            id="cupping-description"
                            name="description"
                            placeholder="Krótki opis sesji, cel degustacji..."
                            value={form.description}
                            onChange={handleChange}
                        />
                    </div>

                    {error && (
                        <p className="cupping-error">{error}</p>
                    )}

                    <div className="cupping-form-actions">
                        <button
                            type="button"
                            className="cupping-btn-secondary"
                            onClick={() => navigate("/cupping")}
                            disabled={isSubmitting}
                        >
                            Anuluj
                        </button>

                        <button
                            type="submit"
                            className="cupping-btn-primary"
                            disabled={isSubmitting || !form.name.trim()}
                        >
                            {isSubmitting ? "Tworzenie..." : "Rozpocznij sesję"}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default CreateCupping;
