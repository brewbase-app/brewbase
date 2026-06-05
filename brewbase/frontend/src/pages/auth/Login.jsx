import { useState } from "react";
import { useNavigate } from "react-router-dom";

import { apiRequest, ApiError } from "../../api/apiClient";
import { establishAuthSession } from "../../api/authSession";

import "../../styles/auth/Login.css";

function Login() {
    const [login, setLogin] = useState("");
    const [password, setPassword] = useState("");
    const [loginError, setLoginError] = useState("");
    const [passwordHint, setPasswordHint] = useState("");

    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoginError("");
        setPasswordHint("");

        try {
            const data = await apiRequest("/api/Auth/login", {
                method: "POST",
                body: JSON.stringify({
                    login: login,
                    password: password,
                }),
            });

            await establishAuthSession(data.token);

            navigate("/home");
        } catch (error) {
            setLoginError(
                error instanceof Error
                    ? error.message
                    : "Nieprawidłowe dane logowania"
            );

            if (error instanceof ApiError && error.passwordHint) {
                setPasswordHint(error.passwordHint);
            }
        }
    };

    return (
        <div className="login-wrapper">
            <div className="login-card">
                <h1 className="logo">BrewBase</h1>

                <h2>Logowanie</h2>

                <form onSubmit={handleSubmit}>
                    <input
                        type="text"
                        placeholder="login"
                        value={login}
                        onChange={(e) => {
                            setLogin(e.target.value);
                            setPasswordHint("");
                            setLoginError("");
                        }}
                    />

                    <input
                        type="password"
                        placeholder="hasło"
                        value={password}
                        onChange={(e) => {
                            setPassword(e.target.value);
                            setPasswordHint("");
                            setLoginError("");
                        }}
                    />

                    {loginError && (
                        <p className="login-error">{loginError}</p>
                    )}

                    {passwordHint && (
                        <div className="login-hint">
                            <strong>Podpowiedź do hasła:</strong> {passwordHint}
                        </div>
                    )}

                    <button type="submit">Zaloguj się</button>
                </form>

                <p className="register-text">
                    Nie masz konta?{" "}
                    <span onClick={() => navigate("/register")}>
                        Zarejestruj się.
                    </span>
                </p>
            </div>
        </div>
    );
}

export default Login;
