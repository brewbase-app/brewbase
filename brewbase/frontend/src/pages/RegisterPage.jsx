import { useState } from "react";
import { useNavigate } from "react-router-dom";

import { apiRequest } from "../api/apiClient";

import "../styles/RegisterPage.css";

export default function RegisterPage() {

    const navigate = useNavigate();

    const [nickname, setNickname] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [repeatPassword, setRepeatPassword] = useState("");

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (password !== repeatPassword) {
            alert("Hasła nie są takie same");
            return;
        }

        try {
            await apiRequest("/api/Auth/register", {
                method: "POST",
                body: JSON.stringify({
                    login: nickname,
                    email: email,
                    password: password,
                }),
            });

            alert("Konto zostało utworzone");

            navigate("/login");
        } catch (error) {
            alert(
                error instanceof Error
                    ? error.message
                    : "Nie udało się połączyć z serwerem"
            );
        }
    };

    return (
        <div className="register-wrapper">
            <div className="register-card">

                <h1 className="logo">BrewBase</h1>
                <h2>Załóż konto</h2>

                <form onSubmit={handleSubmit}>

                    <input
                        type="text"
                        placeholder="nickname"
                        value={nickname}
                        onChange={(e) => setNickname(e.target.value)}
                    />

                    <input
                        type="email"
                        placeholder="e-mail"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                    />

                    <input
                        type="password"
                        placeholder="hasło"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                    />

                    <input
                        type="password"
                        placeholder="powtórz hasło"
                        value={repeatPassword}
                        onChange={(e) => setRepeatPassword(e.target.value)}
                    />

                    <button type="submit">
                        Załóż konto
                    </button>

                </form>

                <p className="login-text">
                    Masz już konto?{" "}
                    <span onClick={() => navigate("/login")}>
                        Zaloguj się.
                    </span>
                </p>

            </div>
        </div>
    );
}