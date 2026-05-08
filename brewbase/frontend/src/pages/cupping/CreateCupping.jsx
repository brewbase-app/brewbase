import { useState } from "react";
import { useNavigate } from "react-router-dom";
import "../../styles/CreateCupping.css";

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

    const handleSubmit = (e) => {
        e.preventDefault();

        console.log("Nowa sesja:", form);

        // później tutaj będzie request do backendu
        navigate("/cupping/1");
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