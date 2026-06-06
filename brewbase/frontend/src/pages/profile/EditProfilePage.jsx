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
} from "../../api/profileApi";

import { getFlavorProfiles } from "../../api/flavorProfileApi";

import {
    DEFAULT_USER_PREFERENCES,
    USER_PREFERENCE_OPTIONS,
    hasAnyPreferences/*,
    loadUserPreferences,
    saveUserPreferences,*/
} from "../../utils/userPreferences";

import { getBrewingMethods } from "../../api/brewingMethodApi"; //OC
import { getBody } from "../../api/bodyApi.js"; //OC
import { getAcidity } from "../../api/acidityApi.js"; //OC
import { getRegions } from "../../api/regionApi.js"; //OC

import {
    getPreferences,
    savePreferences
} from "../../api/preferenceApi";

import "../../styles/profile/editProfile.css";

function PreferenceTagGroup({
    label,
    hint,
    options,
    selectedValues,
    multiple = false,
    onSelect,
}) {
    const isSelected = (option) =>
        multiple
            ? selectedValues.includes(option)
            : selectedValues === option;

    return (
        <div className="form-group">
            <label>{label}</label>

            {hint && <p className="preferences-helper">{hint}</p>}

            <div className="preferences-tags">
                {options.map((option) => (
                    <button
                        key={option}
                        type="button"
                        className={
                            isSelected(option)
                                ? "preference-tag selected"
                                : "preference-tag"
                        }
                        onClick={() => onSelect(option)}
                    >
                        {option}
                    </button>
                ))}
            </div>
        </div>
    );
}

function EditProfilePage() {
    const [loading, setLoading] = useState(true);

    const [username, setUsername] = useState("");
    const [email, setEmail] = useState("");
    const [currentPassword, setCurrentPassword] = useState("");
    const [newPassword, setNewPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");

    const [preferences, setPreferences] = useState({
        ...DEFAULT_USER_PREFERENCES,
    });

    const [brewingMethods, setBrewingMethods] = useState([]);
    const [regions, setRegions] = useState([]);
    const [bodyOptions, setBodyOptions] = useState([]);
    const [acidityOptions, setAcidityOptions] = useState([]);
    
    const [preferencesLoaded, setPreferencesLoaded] = useState(false);
    /*const [flavorProfileOptions, setFlavorProfileOptions] = useState([]);*/
    const [flavorProfiles, setFlavorProfiles] = useState([]);
    const [flavorProfilesError, setFlavorProfilesError] = useState("");

    const [usernameError, setUsernameError] = useState("");
    const [emailError, setEmailError] = useState("");
    const [passwordError, setPasswordError] = useState("");

    useEffect(() => {
        /*const fetchProfile = async () => {
            try {
                const data = await getProfile();

                setUsername(data.login || "");
                setEmail(data.email || "");
            } catch (error) {
                console.error(error);
                alert("Nie udało się pobrać profilu.");
            } finally {
                setLoading(false);
            }
        };*/

        const fetchProfile = async () => {
            try {
                const data = await getProfile();

                setUsername(data.login || "");
                setEmail(data.email || "");

                const preferencesData =
                    await getPreferences();

                if (preferencesData) {
                    setPreferences({
                        ...DEFAULT_USER_PREFERENCES,

                        experienceLevel:
                            preferencesData.experienceLevel ?? "",

                        acidity:
                            preferencesData.preferredAcidity ?? "",

                        body:
                            preferencesData.preferredBody ?? "",

                        recommendationStyle:
                            preferencesData.recommendationStyle ?? "",

                       

                        flavorProfiles:
                            preferencesData.flavorProfiles ?? [],

                        brewingMethods:
                            preferencesData.brewingMethods ?? [],

                        regions:
                            preferencesData.regions ?? [],
                    });
                }

                setPreferencesLoaded(true);

            } catch (error) {
                console.error(error);
                alert("Nie udało się pobrać profilu.");
            } finally {
                setLoading(false);
            }
        };

        fetchProfile();
        /*setPreferences(loadUserPreferences());
        setPreferencesLoaded(true);*/

       /* const loadFlavorProfiles = async () => {
            try {
                const data = await getFlavorProfiles();
                setFlavorProfileOptions(
                    (Array.isArray(data) ? data : []).map(
                        (profile) => profile.name
                    )
                );
            } catch {
                setFlavorProfilesError(
                    "Nie udało się pobrać profili smakowych."
                );
            }
        };*/
        
        loadFlavorProfiles();
        loadPreferenceOptions();
    }, []);

    const loadFlavorProfiles = async () => {
        try {
            const data = await getFlavorProfiles();

            setFlavorProfiles(
                Array.isArray(data)
                    ? data
                    : []
            );
        } catch {
            setFlavorProfilesError(
                "Nie udało się pobrać profili smakowych."
            );
        }
    };
    
    const loadPreferenceOptions = async () => {
        try {
            const brewingMethodsData =
                await getBrewingMethods();

            setBrewingMethods(
                Array.isArray(brewingMethodsData)
                    ? brewingMethodsData
                    : []
            );

            const regionsData =
                await getRegions();

            setRegions(
                Array.isArray(regionsData)
                    ? regionsData
                    : []
            );

            const bodyData =
                await getBody();

            setBodyOptions(
                Array.isArray(bodyData)
                    ? bodyData
                    : []
            );

            const acidityData =
                await getAcidity();

            setAcidityOptions(
                Array.isArray(acidityData)
                    ? acidityData
                    : []
            );
        }
        catch (error) {
            console.error(error);
        }
    };

    const setSinglePreference = (field, value) => {
        setPreferences((previous) => ({
            ...previous,
            [field]: value,
        }));
    };

    const toggleArrayPreference = (field, value) => {
        setPreferences((previous) => {
            const current = previous[field];
            const exists = current.includes(value);

            return {
                ...previous,
                [field]: exists
                    ? current.filter((item) => item !== value)
                    : [...current, value],
            };
        });
    };

    const handleSubmit = async (event) => {
        event.preventDefault();

        setUsernameError("");
        setEmailError("");
        setPasswordError("");

        if (newPassword || confirmPassword) {
            if (newPassword !== confirmPassword) {
                setPasswordError("Nowe hasła nie są identyczne.");
                return;
            }

            if (newPassword.length < 6) {
                setPasswordError("Hasło musi mieć minimum 6 znaków.");
                return;
            }
        }

        try {
            await updateProfile({
                login: username,
                email,
                currentPassword,
                newPassword,
            });

            const dto = {
                experienceLevel: preferences.experienceLevel,

                preferredRoastLevel: "Średnie",

                preferredAcidity: preferences.acidity,

                preferredBody: preferences.body,

                recommendationStyle:
                preferences.recommendationStyle,
                

                flavorProfileIds: flavorProfiles
                    .filter(x =>
                        preferences.flavorProfiles.includes(x.name)
                    )
                    .map(x => x.id),

                brewingMethodIds: brewingMethods
                    .filter(x =>
                        preferences.brewingMethods.includes(x.name)
                    )
                    .map(x => x.id),

                regionIds: regions
                    .filter(x =>
                        preferences.regions.includes(x.name)
                    )
                    .map(x => x.id),
            };

            await savePreferences(dto);

            alert("Zmiany zostały zapisane.");

            setCurrentPassword("");
            setNewPassword("");
            setConfirmPassword("");
        } catch (error) {
            console.error(error);
            alert("Nie udało się zapisać zmian.");
        }
    };

    if (loading) {
        return (
            <div className="edit-profile-page">
                <div
                    style={{
                        color: "white",
                        fontSize: "20px",
                        padding: "40px",
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
                <div className="edit-profile-header">
                    <div>
                        <p className="edit-profile-label">USTAWIENIA</p>
                        <h1>Edytuj profil</h1>
                    </div>
                </div>

                <form className="edit-profile-form" onSubmit={handleSubmit}>
                    <div className="settings-card">
                        <div className="settings-card-header">
                            <User size={18} />
                            <h2>Dane konta</h2>
                        </div>

                        <div className="form-group">
                            <label>Nazwa użytkownika</label>
                            <div className="input-wrapper">
                                <User size={16} />
                                <input
                                    type="text"
                                    value={username}
                                    onChange={(event) =>
                                        setUsername(event.target.value)
                                    }
                                />
                            </div>
                            {usernameError && (
                                <span className="form-error">
                                    {usernameError}
                                </span>
                            )}
                        </div>

                        <div className="form-group">
                            <label>Adres e-mail</label>
                            <div className="input-wrapper">
                                <Mail size={16} />
                                <input
                                    type="email"
                                    value={email}
                                    onChange={(event) =>
                                        setEmail(event.target.value)
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

                    <div className="settings-card">
                        <div className="settings-card-header">
                            <ShieldCheck size={18} />
                            <h2>Bezpieczeństwo</h2>
                        </div>

                        <div className="form-group">
                            <label>Aktualne hasło</label>
                            <div className="input-wrapper">
                                <Lock size={16} />
                                <input
                                    type="password"
                                    value={currentPassword}
                                    onChange={(event) =>
                                        setCurrentPassword(event.target.value)
                                    }
                                />
                            </div>
                        </div>

                        <div className="form-group">
                            <label>Nowe hasło</label>
                            <div className="input-wrapper">
                                <Lock size={16} />
                                <input
                                    type="password"
                                    value={newPassword}
                                    onChange={(event) =>
                                        setNewPassword(event.target.value)
                                    }
                                />
                            </div>
                        </div>

                        <div className="form-group">
                            <label>Potwierdź nowe hasło</label>
                            <div className="input-wrapper">
                                <Lock size={16} />
                                <input
                                    type="password"
                                    value={confirmPassword}
                                    onChange={(event) =>
                                        setConfirmPassword(event.target.value)
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

                    <div className="settings-card">
                        <div className="settings-card-header">
                            <SlidersHorizontal size={18} />
                            <h2>Preferencje z rejestracji</h2>
                        </div>

                        {preferencesLoaded &&
                            !hasAnyPreferences(preferences) && (
                                <p className="preferences-empty">
                                    Nie wybrano jeszcze preferencji podczas
                                    rejestracji. Uzupełnij je poniżej.
                                </p>
                            )}

                        <PreferenceTagGroup
                            label="Poziom wiedzy o kawie"
                            hint="Wybierz jedną opcję"
                            options={USER_PREFERENCE_OPTIONS.experienceLevel}
                            selectedValues={preferences.experienceLevel}
                            onSelect={(option) =>
                                setSinglePreference(
                                    "experienceLevel",
                                    option
                                )
                            }
                        />

                        <PreferenceTagGroup
                            label="Ulubione metody parzenia"
                            hint="Możesz wybrać kilka opcji"
                            options={brewingMethods.map(x => x.name)}
                            selectedValues={preferences.brewingMethods}
                            multiple
                            onSelect={(option) =>
                                toggleArrayPreference(
                                    "brewingMethods",
                                    option
                                )
                            }
                        />

                        <PreferenceTagGroup
                            label="Preferowane profile smakowe"
                            hint="Możesz wybrać kilka opcji"
                            options={flavorProfiles.map(x => x.name)}
                            selectedValues={preferences.flavorProfiles}
                            multiple
                            onSelect={(option) =>
                                toggleArrayPreference(
                                    "flavorProfiles",
                                    option
                                )
                            }
                        />

                        {flavorProfilesError && (
                            <p className="preferences-helper">
                                {flavorProfilesError}
                            </p>
                        )}

                        <PreferenceTagGroup
                            label="Preferowana kwasowość"
                            hint="Wybierz jedną opcję"
                            options={acidityOptions.map(x => x.name)}
                            selectedValues={preferences.acidity}
                            onSelect={(option) =>
                                setSinglePreference("acidity", option)
                            }
                        />

                        <PreferenceTagGroup
                            label="Preferowane body"
                            hint="Wybierz jedną opcję"
                            options={bodyOptions.map(x => x.name)}
                            selectedValues={preferences.body}
                            onSelect={(option) =>
                                setSinglePreference("body", option)
                            }
                        />

                        <PreferenceTagGroup
                            label="Interesujące regiony"
                            hint="Możesz wybrać kilka opcji"
                            options={regions.map(x => x.name)}
                            selectedValues={preferences.regions}
                            multiple
                            onSelect={(option) =>
                                toggleArrayPreference("regions", option)
                            }
                        />

                        <PreferenceTagGroup
                            label="Styl rekomendacji"
                            hint="Wybierz jedną opcję"
                            options={
                                USER_PREFERENCE_OPTIONS.recommendationStyle
                            }
                            selectedValues={preferences.recommendationStyle}
                            onSelect={(option) =>
                                setSinglePreference(
                                    "recommendationStyle",
                                    option
                                )
                            }
                        />
                        
                    </div>

                    <button type="submit" className="save-button">
                        <Save size={18} />
                        Zapisz zmiany
                    </button>
                </form>
            </div>
        </div>
    );
}

export default EditProfilePage;
