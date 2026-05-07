import { useState } from "react";
import "../styles/RegisterPage.css";

export default function RegisterPage() {
    const [nickname, setNickname] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [repeatPassword, setRepeatPassword] = useState("");

    const handleSubmit = (e) => {
        e.preventDefault();
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

                    <button type="submit">Załóż konto</button>
                </form>

                <p className="login-text">
                    Masz już konto? <span>Zaloguj się.</span>
                </p>
            </div>
        </div>
    );
}