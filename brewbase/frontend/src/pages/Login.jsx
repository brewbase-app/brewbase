import { useState } from "react";
import { useNavigate } from "react-router-dom";

import { apiRequest, ApiError } from "../api/apiClient";
import { establishAuthSession } from "../api/authSession";

function Login() {
    const [login, setLogin] = useState("");
    const [password, setPassword] = useState("");
    const [loginError, setLoginError] = useState("");
    const [passwordHint, setPasswordHint] = useState("");

    const navigate = useNavigate(); //  dodane

    /*const handleSubmit = (e) => {
        e.preventDefault();

        // testowe logowanie
        if (email === "admin@test.com" && password === "1234") {
            navigate("/home"); //  przekierowanie
        } else {
            alert("Nieprawidłowe dane");
        }
    };*/

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
        <div style={styles.page}>
            <div style={styles.card}>
                <h1 style={styles.title}>BrewBase</h1>

                <h2 style={styles.subtitle}>Logowanie</h2>

                {/*  FORM zamiast luźnych inputów */}
                <form onSubmit={handleSubmit}>
                    <input
                        style={styles.input}
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
                        style={styles.input}
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
                        <p style={styles.error}>{loginError}</p>
                    )}

                    {passwordHint && (
                        <div style={styles.hintBox}>
                            <strong>Podpowiedź do hasła:</strong> {passwordHint}
                        </div>
                    )}

                    <div style={styles.buttonContainer}>
                        <button style={styles.button} type="submit">
                            Zaloguj się
                        </button>
                    </div>
                </form>

                <p style={styles.footer}>
                    Nie masz konta?{" "}
                    <span
                        style={styles.link}
                        onClick={() => navigate("/register")}
                    >Zarejestruj się.
                    </span>
                </p>
            </div>
        </div>
    );
}

/*const styles = {
    /!*page: {
        height: "100vh",
        backgroundColor: "#000",
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        fontFamily: "Arial, sans-serif",
    },
    card: {
        backgroundColor: "#d9d9d9",
        padding: "50px 40px",
        borderRadius: "20px",
        width: "400px",
        textAlign: "center",
    },
    title: {
        fontSize: "36px",
        marginBottom: "30px",
    },
    subtitle: {
        fontSize: "22px",
        marginBottom: "20px",
    },
    input: {
        width: "100%",
        padding: "12px",
        marginBottom: "15px",
        borderRadius: "8px",
        border: "1px solid #999",
        boxSizing: "border-box",
    },
    error: {
        color: "#b42318",
        fontSize: "14px",
        marginBottom: "12px",
        textAlign: "left",
    },
    hintBox: {
        backgroundColor: "#fff8e8",
        border: "1px solid #f0d59b",
        borderRadius: "10px",
        padding: "12px 14px",
        marginBottom: "15px",
        fontSize: "14px",
        color: "#5c4a1f",
        textAlign: "left",
        lineHeight: 1.5,
    },
    button: {
        width: "70%",
        padding: "12px",
        marginTop: "10px",
        borderRadius: "25px",
        border: "none",
        backgroundColor: "#1f1f1f",
        color: "white",
        cursor: "pointer",
    },
    footer: {
        marginTop: "20px",
        fontSize: "12px",
    },
    link: {
        fontWeight: "bold",
        cursor: "pointer",
    },*!/
    
};*/

const styles = {
    page: {
        minHeight: "100vh",
        background: "#0f0f0f",

        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        padding: "32px",
        fontFamily: "Arial, sans-serif",
    },
    card: {
        width: "100%",
        maxWidth: "460px",
        background: "#181818",
        border: "1px solid rgba(255,255,255,0.06)",
        borderRadius: "32px",
        padding: "52px 42px",
        boxShadow: "0 10px 40px rgba(0,0,0,0.35)",
        display: "flex",
        flexDirection: "column",
        color: "white",
    },
    title: {
        fontSize: "48px",
        fontWeight: "700",
        textAlign: "center",
        marginBottom: "18px",
        letterSpacing: "-1px",
    },
    subtitle: {
        textAlign: "center",
        color: "#9a9a9a",
        fontSize: "0.96rem",
        marginBottom: "38px",
        lineHeight: 1.5,
    },
    input: {
        width: "100%",
        height: "56px",
        border: "none",
        background: "#232323",
        borderRadius: "18px",
        padding: "0 18px",
        color: "white",
        fontSize: "0.96rem",
        boxSizing: "border-box",
        marginBottom: "16px",
    },
    error: {
        background: "rgba(255, 0, 0, 0.08)",
        border: "1px solid rgba(255, 0, 0, 0.15)",
        color: "#ff9d9d",
        padding: "14px",
        borderRadius: "16px",
        fontSize: "0.9rem",
        marginBottom: "12px",
    },

    hintBox: {
        background: "rgba(255,255,255,0.04)",
        border: "1px solid rgba(255,255,255,0.08)",
        color: "#bdbdbd",
        padding: "14px",
        borderRadius: "16px",
        fontSize: "0.9rem",
        lineHeight: 1.5,
        marginBottom: "15px",
    },
    button: {
        width: "100%",
        height: "56px",
        border: "none",
        borderRadius: "18px",
        background: "white",
        color: "black",
        fontSize: "1rem",
        fontWeight: "600",
        cursor: "pointer",
        marginTop: "12px",
    },
    footer: {
        textAlign: "center",
        marginTop: "28px",
        color: "#8a8a8a",
        fontSize: "0.92rem",
    },
    link: {
        color: "white",
        fontWeight: "600",
        cursor: "pointer",
    },
};

export default Login;