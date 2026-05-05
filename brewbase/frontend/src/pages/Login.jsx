import { useState } from "react";
import { useNavigate } from "react-router-dom";

function Login() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");

    const navigate = useNavigate(); // 🔥 dodane

    const handleSubmit = (e) => {
        e.preventDefault();

        // testowe logowanie
        if (email === "admin@test.com" && password === "1234") {
            navigate("/home"); // 🔥 przekierowanie
        } else {
            alert("Nieprawidłowe dane");
        }
    };

    return (
        <div style={styles.page}>
            <div style={styles.card}>
                <h1 style={styles.title}>BrewBase</h1>

                <h2 style={styles.subtitle}>Logowanie</h2>

                {/* 🔥 FORM zamiast luźnych inputów */}
                <form onSubmit={handleSubmit}>
                    <input
                        style={styles.input}
                        type="text"
                        placeholder="e-mail"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                    />

                    <input
                        style={styles.input}
                        type="password"
                        placeholder="hasło"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                    />

                    <button style={styles.button} type="submit">
                        Zaloguj się
                    </button>
                </form>

                <p style={styles.footer}>
                    Nie masz konta?{" "}
                    <span style={styles.link}>Zarejestruj się.</span>
                </p>
            </div>
        </div>
    );
}

const styles = {
    page: {
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
    },
};

export default Login;