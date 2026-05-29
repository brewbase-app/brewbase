import { useState } from "react";

import {
    User,
    Mail,
    Lock,
    Save,
    SlidersHorizontal,
    ShieldCheck
} from "lucide-react";

import "../styles/editProfile.css";

function EditProfilePage() {

    // SAVED USER

    const savedUser =
        JSON.parse(
            localStorage.getItem("brewbaseUser")
        ) || {
            username: "kontotestowe",
            email: "konto@brewbase.com",
        };

    // MOCK EXISTING USERS

    const existingUsers = [
        "dailybrew",
        "coffeenerd",
        "brew_king"
    ];

    const existingEmails = [
        "test@test.com",
        "coffee@gmail.com"
    ];

    // FORM STATE

    const [username, setUsername] =
        useState(savedUser.username);

    const [email, setEmail] =
        useState(savedUser.email);

    const [currentPassword, setCurrentPassword] =
        useState("");

    const [newPassword, setNewPassword] =
        useState("");

    const [confirmPassword, setConfirmPassword] =
        useState("");

    // PREFERENCES

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

    const toggleArrayValue = (field, value) => {

        const current = preferences[field];

        const exists = current.includes(value);

        setPreferences({
            ...preferences,
            [field]: exists
                ? current.filter((v) => v !== value)
                : [...current, value],
        });
    };

    // ERRORS

    const [usernameError, setUsernameError] =
        useState("");

    const [emailError, setEmailError] =
        useState("");

    const [passwordError, setPasswordError] =
        useState("");

    // SAVE

    const handleSubmit = (e) => {

        e.preventDefault();

        setUsernameError("");
        setEmailError("");
        setPasswordError("");

        // USERNAME VALIDATION

        if (
            existingUsers.includes(
                username.toLowerCase()
            ) &&
            username.toLowerCase() !==
            savedUser.username.toLowerCase()
        ) {

            setUsernameError(
                "Ta nazwa użytkownika jest już zajęta."
            );

            return;
        }

        // EMAIL VALIDATION

        if (
            existingEmails.includes(
                email.toLowerCase()
            ) &&
            email.toLowerCase() !==
            savedUser.email.toLowerCase()
        ) {

            setEmailError(
                "Ten adres e-mail jest już używany."
            );

            return;
        }

        // PASSWORD VALIDATION

        if (
            newPassword ||
            confirmPassword
        ) {

            if (
                newPassword !== confirmPassword
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

        // SAVE TO LOCAL STORAGE

        localStorage.setItem(
            "brewbaseUser",

            JSON.stringify({
                username,
                email,
                preferences,
            })
        );

        alert(
            "Zmiany zostały zapisane."
        );
    };

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

                        {/* EXPERIENCE */}

                        <div className="form-group">

                            <label>
                                Poziom doświadczenia
                            </label>

                            <select
                                value={preferences.experienceLevel}
                                onChange={(e) =>
                                    setPreferences({
                                        ...preferences,
                                        experienceLevel: e.target.value,
                                    })
                                }
                            >

                                <option value="">
                                    Wybierz opcję
                                </option>

                                <option value="Początkujący">
                                    Początkujący
                                </option>

                                <option value="Średniozaawansowany">
                                    Średniozaawansowany
                                </option>

                                <option value="Zaawansowany">
                                    Zaawansowany
                                </option>

                                <option value="Jeszcze nie wiem">
                                    Jeszcze nie wiem
                                </option>

                            </select>

                        </div>

                        {/* BREWING */}

                        <div className="form-group">

                            <label>
                                Preferowane metody parzenia
                            </label>

                            <div className="preferences-tags">

                                {[
                                    "Espresso",
                                    "V60",
                                    "Aeropress",
                                    "French Press",
                                    "Cold Brew",
                                    "Jeszcze nie wiem",
                                ].map((method) => (

                                    <button
                                        type="button"
                                        key={method}
                                        className={
                                            preferences.brewingMethods.includes(method)
                                                ? "preference-tag selected"
                                                : "preference-tag"
                                        }
                                        onClick={() =>
                                            toggleArrayValue(
                                                "brewingMethods",
                                                method
                                            )
                                        }
                                    >
                                        {method}
                                    </button>

                                ))}

                            </div>

                        </div>

                        {/* FLAVOR */}

                        <div className="form-group">

                            <label>
                                Profil smakowy
                            </label>

                            <div className="preferences-tags">

                                {[
                                    "Czekoladowe",
                                    "Orzechowe",
                                    "Owocowe",
                                    "Kwiatowe",
                                    "Słodkie",
                                    "Jeszcze nie wiem",
                                ].map((flavor) => (

                                    <button
                                        type="button"
                                        key={flavor}
                                        className={
                                            preferences.flavorProfiles.includes(flavor)
                                                ? "preference-tag selected"
                                                : "preference-tag"
                                        }
                                        onClick={() =>
                                            toggleArrayValue(
                                                "flavorProfiles",
                                                flavor
                                            )
                                        }
                                    >
                                        {flavor}
                                    </button>

                                ))}

                            </div>

                        </div>

                        {/* ACIDITY */}

                        <div className="form-group">

                            <label>
                                Preferowana kwasowość
                            </label>

                            <select
                                value={preferences.acidity}
                                onChange={(e) =>
                                    setPreferences({
                                        ...preferences,
                                        acidity: e.target.value,
                                    })
                                }
                            >

                                <option value="">
                                    Wybierz opcję
                                </option>

                                <option value="Niska">
                                    Niska
                                </option>

                                <option value="Średnia">
                                    Średnia
                                </option>

                                <option value="Wysoka">
                                    Wysoka
                                </option>

                                <option value="Nie mam zdania">
                                    Nie mam zdania
                                </option>

                            </select>

                        </div>

                        {/* BODY */}

                        <div className="form-group">

                            <label>
                                Preferowane body
                            </label>

                            <select
                                value={preferences.body}
                                onChange={(e) =>
                                    setPreferences({
                                        ...preferences,
                                        body: e.target.value,
                                    })
                                }
                            >

                                <option value="">
                                    Wybierz opcję
                                </option>

                                <option value="Lekkie">
                                    Lekkie
                                </option>

                                <option value="Zbalansowane">
                                    Zbalansowane
                                </option>

                                <option value="Ciężkie">
                                    Ciężkie
                                </option>

                                <option value="Nie mam zdania">
                                    Nie mam zdania
                                </option>

                            </select>

                        </div>

                        {/* REGIONS */}

                        <div className="form-group">

                            <label>
                                Regiony pochodzenia kaw
                            </label>

                            <div className="preferences-tags">

                                {[
                                    "Etiopia",
                                    "Kolumbia",
                                    "Brazylia",
                                    "Kenia",
                                    "Gwatemala",
                                    "Nie mam preferencji",
                                ].map((region) => (

                                    <button
                                        type="button"
                                        key={region}
                                        className={
                                            preferences.regions.includes(region)
                                                ? "preference-tag selected"
                                                : "preference-tag"
                                        }
                                        onClick={() =>
                                            toggleArrayValue(
                                                "regions",
                                                region
                                            )
                                        }
                                    >
                                        {region}
                                    </button>

                                ))}

                            </div>

                        </div>

                        {/* RECOMMENDATIONS */}

                        <div className="form-group">

                            <label>
                                Styl rekomendacji
                            </label>

                            <select
                                value={preferences.recommendationStyle}
                                onChange={(e) =>
                                    setPreferences({
                                        ...preferences,
                                        recommendationStyle: e.target.value,
                                    })
                                }
                            >

                                <option value="">
                                    Wybierz opcję
                                </option>

                                <option value="Bezpieczne wybory">
                                    Bezpieczne wybory
                                </option>

                                <option value="Zbalansowane">
                                    Zbalansowane
                                </option>

                                <option value="Zaskocz mnie">
                                    Zaskocz mnie
                                </option>

                            </select>

                        </div>

                        {/* EXPLORATION */}

                        <div className="preferences-checkbox">

                            <input
                                type="checkbox"
                                checked={preferences.allowExploration}
                                onChange={(e) =>
                                    setPreferences({
                                        ...preferences,
                                        allowExploration: e.target.checked,
                                    })
                                }
                            />

                            <span>
                                Pokazuj rekomendacje spoza moich preferencji
                            </span>

                        </div>

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

