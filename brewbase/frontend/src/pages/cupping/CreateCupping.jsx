import { useState } from "react";
import { useNavigate } from "react-router-dom";
import "../../styles/CreateCupping.css";
import { createTastingSession } from "../../api/tastingSessionsApi";

const CreateCupping = () => {
    const navigate = useNavigate();

    const [form, setForm] = useState({
        name: "",
        date: "",
        description: "",
    });

    const handleChange = (e) => {
        setForm({
            ...form,
            [e.target.name]: e.target.value,
        });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            const createdSession = await createTastingSession({
                name: form.name,
                description: form.description || null,
                sessionDate: form.date ? `${form.date}T00:00:00` : null,
            });

            navigate(`/cupping/${createdSession.id}`);
        } catch (error) {
            alert(error.message || "Nie udało się utworzyć sesji.");
        }
    };

    return (
        <div className="cupping-wrapper">
            <form className="cupping-box" onSubmit={handleSubmit}>
                <h1>Cupping session</h1>

                <input
                    name="name"
                    placeholder="Nazwa"
                    value={form.name}
                    onChange={handleChange}
                />

                <input
                    name="date"
                    type="date"
                    value={form.date}
                    onChange={handleChange}
                />
                

                <textarea
                    name="description"
                    placeholder="Opis"
                    value={form.description}
                    onChange={handleChange}
                />

                <button type="submit">
                    Rozpocznij sesję
                </button>
            </form>
        </div>
    );
};

export default CreateCupping;