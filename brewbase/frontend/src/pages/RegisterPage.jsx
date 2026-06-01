import { useState } from "react";
import { useNavigate } from "react-router-dom";

import { apiRequest } from "../api/apiClient";
import { establishAuthSession } from "../api/authSession";

import "../styles/RegisterPage.css";

export default function RegisterPage() {

    const navigate = useNavigate();

    const [nickname, setNickname] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [repeatPassword, setRepeatPassword] = useState("");
    const [passwordHint, setPasswordHint] = useState("");

    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);

    const passwordsMatch =
        password === repeatPassword || repeatPassword === "";

    const isFormValid =
        nickname.trim().length >= 3 &&
        email.includes("@") &&
        password.length >= 6 &&
        passwordHint.trim().length >= 3 &&
        passwordsMatch;

    const handleSubmit = async (e) => {

        e.preventDefault();

        setError("");

        if (!passwordsMatch) {
            setError("Hasła nie są takie same");
            return;
        }

        try {

            setLoading(true);

            const data = await apiRequest("/api/Auth/register", {
                method: "POST",
                body: JSON.stringify({
                    login: nickname,
                    email,
                    password,
                    passwordHint,
                }),
            });

            await establishAuthSession(data.token);

            navigate("/onboarding");

        } catch (error) {

            setError(
                error instanceof Error
                    ? error.message
                    : "Nie udało się utworzyć konta"
            );

        } finally {

            setLoading(false);
        }
    };

    return (
        <div className="register-wrapper">

            <div className="register-card">

                <h1 className="logo">
                    BrewBase
                </h1>

                <h2>
                    Załóż konto
                </h2>

                <p className="register-subtitle">
                    Stwórz swój profil smakowy kawy
                </p>

                <form onSubmit={handleSubmit}>

                    <input
                        type="text"
                        placeholder="Nazwa użytkownika"
                        value={nickname}
                        onChange={(e) =>
                            setNickname(e.target.value)
                        }
                    />

                    <input
                        type="email"
                        placeholder="Adres e-mail"
                        value={email}
                        onChange={(e) =>
                            setEmail(e.target.value)
                        }
                    />

                    <input
                        type="password"
                        placeholder="Hasło"
                        value={password}
                        onChange={(e) =>
                            setPassword(e.target.value)
                        }
                    />

                    <input
                        type="password"
                        placeholder="Powtórz hasło"
                        value={repeatPassword}
                        onChange={(e) =>
                            setRepeatPassword(e.target.value)
                        }
                    />

                    <input
                        type="text"
                        placeholder="Podpowiedź do hasła"
                        value={passwordHint}
                        onChange={(e) =>
                            setPasswordHint(e.target.value)
                        }
                    />

                    <p className="hint-description">
                        Dodaj prywatną podpowiedź,
                        która pomoże Ci przypomnieć hasło
                    </p>

                    {!passwordsMatch && (
                        <p className="field-error">
                            Hasła nie są takie same
                        </p>
                    )}

                    {error && (
                        <div className="register-error">
                            {error}
                        </div>
                    )}

                    <button
                        type="submit"
                        disabled={!isFormValid || loading}
                    >
                        {loading
                            ? "Tworzenie konta..."
                            : "Kontynuuj"}
                    </button>

                </form>

                <p className="login-text">

                    Masz już konto?{" "}

                    <span
                        onClick={() => navigate("/login")}
                    >
                        Zaloguj się
                    </span>

                </p>

            </div>

        </div>
    );
}

