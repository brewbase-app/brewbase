import { useEffect, useState } from "react";

import {
    User,
    Mail,
    Lock,
    Save,
    SlidersHorizontal,
    ShieldCheck
} from "lucide-react";

import {
    getProfile,
    updateProfile
} from "../api/profileApi";

import "../styles/editProfile.css";

function EditProfilePage() {

    const [loading, setLoading] =
        useState(true);

    // FORM STATE

    const [username, setUsername] =
        useState("");

    const [email, setEmail] =
        useState("");

    const [currentPassword, setCurrentPassword] =
        useState("");

    const [newPassword, setNewPassword] =
        useState("");

    const [confirmPassword, setConfirmPassword] =
        useState("");

    // PREFERENCES (LOCAL ONLY FOR NOW)

    const [preferences, setPreferences] =
        useState({
            experienceLevel: "",
            brewingMethods: [],
            flavorProfiles: [],
            acidity: "",
            body: "",
            regions: [],
            recommendationStyle: "",
            allowExploration: false,
        });

    // ERRORS

    const [usernameError, setUsernameError] =
        useState("");

    const [emailError, setEmailError] =
        useState("");

    const [passwordError, setPasswordError] =
        useState("");

    useEffect(() => {

        const fetchProfile = async () => {

            try {

                const data =
                    await getProfile();

                console.log(data);

                setUsername(
                    data.login || ""
                );

                setEmail(
                    data.email || ""
                );

            } catch (error) {

                console.error(error);

                alert(
                    "Nie udało się pobrać profilu."
                );

            } finally {

                setLoading(false);
            }
        };

        fetchProfile();

    }, []);

    const toggleArrayValue = (
        field,
        value
    ) => {

        const current =
            preferences[field];

        const exists =
            current.includes(value);

        setPreferences({
            ...preferences,

            [field]: exists
                ? current.filter(
                    (v) => v !== value
                )
                : [...current, value],
        });
    };

    // SAVE

    const handleSubmit = async (e) => {

        e.preventDefault();

        setUsernameError("");
        setEmailError("");
        setPasswordError("");

        // PASSWORD VALIDATION

        if (
            newPassword ||
            confirmPassword
        ) {

            if (
                newPassword !==
                confirmPassword
            ) {

                setPasswordError(
                    "Nowe hasła nie są identyczne."
                );

                return;
            }

            if (
                newPassword.length < 6
            ) {

                setPasswordError(
                    "Hasło musi mieć minimum 6 znaków."
                );

                return;
            }
        }

        try {

            await updateProfile({

                login: username,

                email,

                currentPassword,

                newPassword
            });

            alert(
                "Zmiany zostały zapisane."
            );

            setCurrentPassword("");
            setNewPassword("");
            setConfirmPassword("");

        } catch (error) {

            console.error(error);

            alert(
                "Nie udało się zapisać zmian."
            );
        }
    };

    if (loading) {

        return (

            <div className="edit-profile-page">

                <div
                    style={{
                        color: "white",
                        fontSize: "20px",
                        padding: "40px"
                    }}
                >
                    Ładowanie profilu...
                </div>

            </div>
        );
    }

    return (

        <div className="edit-profile-page">

            <div className="edit-profile-layout">

                {/* HEADER */}

                <div className="edit-profile-header">

                    <div>

                        <p className="edit-profile-label">
                            USTAWIENIA
                        </p>

                        <h1>
                            Edytuj profil
                        </h1>

                    </div>

                </div>

                {/* FORM */}

                <form
                    className="edit-profile-form"
                    onSubmit={handleSubmit}
                >

                    {/* ACCOUNT */}

                    <div className="settings-card">

                        <div className="settings-card-header">

                            <User size={18} />

                            <h2>
                                Dane konta
                            </h2>

                        </div>

                        {/* USERNAME */}

                        <div className="form-group">

                            <label>
                                Nazwa użytkownika
                            </label>

                            <div className="input-wrapper">

                                <User size={16} />

                                <input
                                    type="text"
                                    value={username}
                                    onChange={(e) =>
                                        setUsername(
                                            e.target.value
                                        )
                                    }
                                />

                            </div>

                            {usernameError && (

                                <span className="form-error">
                                    {usernameError}
                                </span>

                            )}

                        </div>

                        {/* EMAIL */}

                        <div className="form-group">

                            <label>
                                Adres e-mail
                            </label>

                            <div className="input-wrapper">

                                <Mail size={16} />

                                <input
                                    type="email"
                                    value={email}
                                    onChange={(e) =>
                                        setEmail(
                                            e.target.value
                                        )
                                    }
                                />

                            </div>

                            {emailError && (

                                <span className="form-error">
                                    {emailError}
                                </span>

                            )}

                        </div>

                    </div>

                    {/* SECURITY */}

                    <div className="settings-card">

                        <div className="settings-card-header">

                            <ShieldCheck size={18} />

                            <h2>
                                Bezpieczeństwo
                            </h2>

                        </div>

                        {/* CURRENT PASSWORD */}

                        <div className="form-group">

                            <label>
                                Aktualne hasło
                            </label>

                            <div className="input-wrapper">

                                <Lock size={16} />

                                <input
                                    type="password"
                                    value={currentPassword}
                                    onChange={(e) =>
                                        setCurrentPassword(
                                            e.target.value
                                        )
                                    }
                                />

                            </div>

                        </div>

                        {/* NEW PASSWORD */}

                        <div className="form-group">

                            <label>
                                Nowe hasło
                            </label>

                            <div className="input-wrapper">

                                <Lock size={16} />

                                <input
                                    type="password"
                                    value={newPassword}
                                    onChange={(e) =>
                                        setNewPassword(
                                            e.target.value
                                        )
                                    }
                                />

                            </div>

                        </div>

                        {/* CONFIRM PASSWORD */}

                        <div className="form-group">

                            <label>
                                Potwierdź nowe hasło
                            </label>

                            <div className="input-wrapper">

                                <Lock size={16} />

                                <input
                                    type="password"
                                    value={confirmPassword}
                                    onChange={(e) =>
                                        setConfirmPassword(
                                            e.target.value
                                        )
                                    }
                                />

                            </div>

                            {passwordError && (

                                <span className="form-error">
                                    {passwordError}
                                </span>

                            )}

                        </div>

                    </div>

                    {/* PREFERENCES */}

                    <div className="settings-card">

                        <div className="settings-card-header">

                            <SlidersHorizontal size={18} />

                            <h2>
                                Preferencje
                            </h2>

                        </div>

                        <p
                            style={{
                                color: "#8a8a8a",
                                fontSize: "14px",
                                marginBottom: "10px"
                            }}
                        >
                            Preferencje są obecnie dostępne tylko lokalnie po stronie frontendu.
                        </p>

                    </div>

                    {/* SAVE */}

                    <button
                        type="submit"
                        className="save-button"
                    >

                        <Save size={18} />

                        Zapisz zmiany

                    </button>

                </form>

            </div>

        </div>
    );
}

export default EditProfilePage;

