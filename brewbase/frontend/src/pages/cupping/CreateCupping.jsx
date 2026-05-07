import { useState } from "react";
import "../../styles/CreateCupping.css";

const CreateCupping = () => {
    const [form, setForm] = useState({
        name: "",
        date: "",
        coffees: "",
        description: "",
    });

    const handleChange = (e) => {
        setForm({
            ...form,
            [e.target.name]: e.target.value,
        });
    };

    return (
        <div className="cupping-wrapper">
            <div className="cupping-box">
                <h1>Cupping session</h1>

                <input
                    name="name"
                    placeholder="Nazwa"
                    onChange={handleChange}
                />

                <input
                    name="date"
                    type="date"
                    onChange={handleChange}
                />

                <input
                    name="coffees"
                    placeholder="Ilość kaw"
                    onChange={handleChange}
                />

                <textarea
                    name="description"
                    placeholder="Opis"
                    onChange={handleChange}
                />

                <button>Rozpocznij sesję</button>
            </div>
        </div>
    );
};

export default CreateCupping;