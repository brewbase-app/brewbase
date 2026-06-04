import { useEffect, useState } from "react";
import { Navigate, useNavigate } from "react-router-dom";

import {
    DEFAULT_USER_PREFERENCES,
    USER_PREFERENCE_OPTIONS,
    saveUserPreferences,
} from "../../utils/userPreferences";
import { getAuthToken } from "../../utils/auth";

import { savePreferences } from "../../api/preferenceApi";
import { getOnboardingFlavorProfiles } from "../../api/flavorProfileApi";
import { getBrewingMethods } from "../../api/brewingMethodApi"; //OC
import { getBody } from "../../api/bodyApi.js"; //OC
import { getAcidity } from "../../api/acidityApi.js"; //OC

import "../../styles/auth/PreferencesOnboardingPage.css";

const steps = [
    "Poziom",
    "Parzenie",
    "Smak",
    "Body",
    "Regiony",
    "Rekomendacje",
];

export default function PreferencesOnboardingPage() {

    const navigate = useNavigate();

    const [step, setStep] = useState(0);

    const [preferences, setPreferences] = useState({
        ...DEFAULT_USER_PREFERENCES,
    });

    const [flavorProfiles, setFlavorProfiles] = useState([]);
    const [brewingMethods, setBrewingMethods] = useState([]); //OC
    const [brewingMethodsUndecided, setBrewingMethodsUndecided] = useState(false); //OC
    const [body, setBody] = useState([]); //OC
    const [acidity, setAcidity] = useState([]); //OC
    const [selectedFlavorProfileIds, setSelectedFlavorProfileIds] = useState([]);
    const [flavorPreferencesUndecided, setFlavorPreferencesUndecided] = useState(false);
    const [flavorProfilesLoading, setFlavorProfilesLoading] = useState(true);
    const [flavorProfilesError, setFlavorProfilesError] = useState("");

    useEffect(() => {
        const loadFlavorProfiles = async () => {
            try {
                setFlavorProfilesLoading(true);
                setFlavorProfilesError("");

                const data = await getOnboardingFlavorProfiles(10);
                setFlavorProfiles(Array.isArray(data) ? data : []);

                const methods = await getBrewingMethods(); //OC
                setBrewingMethods(Array.isArray(methods) ? methods : []); //OC

                const bodyData = await getBody(); //OC
                setBody(Array.isArray(bodyData) ? bodyData : []); //OC

                const acidityData = await getAcidity(); //OC
                setAcidity(Array.isArray(acidityData) ? acidityData : []); //OC
            } catch {
                setFlavorProfilesError(
                    "Nie udało się pobrać profili smakowych."
                );
            } finally {
                setFlavorProfilesLoading(false);
            }
        };

        loadFlavorProfiles();
    }, []);

    if (!getAuthToken()) {
        return <Navigate to="/login" replace />;
    }

    const nextStep = async () => {

        if (step < steps.length - 1) {

            setStep(step + 1);

        } else {
            const selectedFlavorProfileNames = flavorPreferencesUndecided
                ? []
                : flavorProfiles
                      .filter((profile) =>
                          selectedFlavorProfileIds.includes(profile.id)
                      )
                      .map((profile) => profile.name);

            saveUserPreferences({
                ...preferences,
                flavorProfiles: selectedFlavorProfileNames,
            });

            const dto = {
                experienceLevel: preferences.experienceLevel,

                preferredRoastLevel: "Średnie",

                preferredAcidity: preferences.acidity,

                preferredBody: preferences.body,

                recommendationStyle:
                preferences.recommendationStyle,

                allowExploration:
                preferences.allowExploration,

                flavorProfileIds: flavorPreferencesUndecided
                    ? []
                    : selectedFlavorProfileIds,

                brewingMethods:
                preferences.brewingMethods,

                regions:
                preferences.regions
            };

            await savePreferences(dto);

            navigate("/home");
        }
    };

    const prevStep = () => {

        if (step > 0) {
            setStep(step - 1);
        }
    };

    const toggleFlavorProfile = (profileId) => {
        setFlavorPreferencesUndecided(false);
        setSelectedFlavorProfileIds((current) =>
            current.includes(profileId)
                ? current.filter((id) => id !== profileId)
                : [...current, profileId]
        );
    };
    
    const selectFlavorPreferencesUndecided = () => {
        setFlavorPreferencesUndecided(true);
        setSelectedFlavorProfileIds([]);
    };

    const toggleBrewingMethod = (methodName) => {

        setBrewingMethodsUndecided(false);

        const current = preferences.brewingMethods;

        const exists = current.includes(methodName);

        setPreferences({
            ...preferences,
            brewingMethods: exists
                ? current.filter((x) => x !== methodName)
                : [...current, methodName],
        });
    };
    
    const selectBrewingMethodsUndecided = () => {
        setBrewingMethodsUndecided(true);

        setPreferences({
            ...preferences,
            brewingMethods: [],
        });
    };
    
    
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

    const renderStep = () => {

        switch (step) {

            case 0:
                return (
                    <>
                        <h2>
                            Jak oceniasz swoją wiedzę o kawie?
                        </h2>

                        <p className="field-description">
                            Wybierz jedną opcję
                        </p>

                        {USER_PREFERENCE_OPTIONS.experienceLevel.map((option) => (
                            <button
                                type="button"
                                key={option}
                                className={
                                    preferences.experienceLevel === option
                                        ? "selected"
                                        : ""
                                }
                                onClick={() =>
                                    setPreferences({
                                        ...preferences,
                                        experienceLevel: option,
                                    })
                                }
                            >
                                {option}
                            </button>
                        ))}
                    </>
                );

            case 1:
                return (
                    <>
                        <h2>
                            Jakie metody parzenia lubisz?
                        </h2>

                        <p className="field-description">
                            Możesz wybrać kilka opcji
                        </p>

                        {brewingMethods.slice(0, 10).map((method) => (
                            <button
                                type="button"
                                key={method.id}
                                className={
                                    preferences.brewingMethods.includes(method.name)
                                        ? "selected"
                                        : ""
                                }
                                onClick={() =>
                                    toggleBrewingMethod(method.name)
                                }
                            >
                                {method.name}
                            </button>
                            
                            
                        ))}
                        <button
                            type="button"
                            className={
                                brewingMethodsUndecided
                                    ? "selected"
                                    : ""
                            }
                            onClick={selectBrewingMethodsUndecided}
                        >
                            Jeszcze nie wiem
                        </button>
                    </>
                );

            case 2:
                return (
                    <>
                        <h2>
                            Jakie profile smakowe preferujesz?
                        </h2>

                        <p className="field-description">
                            Wybierz profile smakowe lub zaznacz, że
                            jeszcze nie wiesz
                        </p>

                        {flavorProfilesLoading && (
                            <p className="field-description">
                                Ładowanie profili smakowych...
                            </p>
                        )}

                        {flavorProfilesError && (
                            <p className="field-description">
                                {flavorProfilesError}
                            </p>
                        )}

                        {!flavorProfilesLoading &&
                            !flavorProfilesError && (
                            <>
                                {flavorProfiles.slice(0, 10).map((profile) => (
                                    <button
                                        type="button"
                                        key={profile.id}
                                        className={
                                            selectedFlavorProfileIds.includes(
                                                profile.id
                                            )
                                                ? "selected"
                                                : ""
                                        }
                                        onClick={() =>
                                            toggleFlavorProfile(profile.id)
                                        }
                                    >
                                        {profile.name}
                                    </button>
                                ))}
                                
                                <button
                                    type="button"
                                    className={
                                        flavorPreferencesUndecided
                                            ? "selected"
                                            : ""
                                    }
                                    onClick={selectFlavorPreferencesUndecided}
                                >
                                    Nie wiem jeszcze
                                </button>
                            </>
                        )}
                    </>
                );

            case 3:
                return (
                    <>
                        <h2>
                            Preferowana kwasowość
                        </h2>

                        <p className="field-description">
                            Wybierz jedną opcję
                        </p>

                        {acidity.slice(0, 10).map((acidityOption) => (
                            <button
                                type="button"
                                key={acidityOption.id}
                                className={
                                    preferences.acidity === acidityOption.name
                                        ? "selected"
                                        : ""
                                }
                                onClick={() =>
                                    setPreferences({
                                        ...preferences,
                                        acidity: acidityOption.name,
                                    })
                                }
                            >
                                {acidityOption.name}
                            </button>
                        ))}

                        <button
                            type="button"
                            className={
                                preferences.acidity === "Nie mam zdania"
                                    ? "selected"
                                    : ""
                            }
                            onClick={() =>
                                setPreferences({
                                    ...preferences,
                                    acidity:
                                        preferences.acidity === "Nie mam zdania"
                                            ? ""
                                            : "Nie mam zdania",
                                })
                            }
                        >
                            Nie mam zdania
                        </button>

                        <h2 className="secondary-title">
                            Preferowane body
                        </h2>

                        <p className="field-description">
                            Wybierz jedną opcję
                        </p>

                        {body.slice(0, 10).map((bodyOption) => (
                            <button
                                type="button"
                                key={bodyOption.id}
                                className={
                                    preferences.body === bodyOption.name
                                        ? "selected"
                                        : ""
                                }
                                onClick={() =>
                                    setPreferences({
                                        ...preferences,
                                        body: bodyOption.name,
                                    })
                                }
                            >
                                {bodyOption.name}
                            </button>
                        ))}

                        <button
                            type="button"
                            className={
                                preferences.body === "Nie mam zdania"
                                    ? "selected"
                                    : ""
                            }
                            onClick={() =>
                                setPreferences({
                                    ...preferences,
                                    body:
                                        preferences.body === "Nie mam zdania"
                                            ? ""
                                            : "Nie mam zdania",
                                })
                            }
                        >
                            Nie mam zdania
                        </button>
                    </>
                );

            case 4:
                return (
                    <>
                        <h2>
                            Jakie regiony Cię interesują?
                        </h2>

                        <p className="field-description">
                            Możesz wybrać kilka opcji
                        </p>

                        {USER_PREFERENCE_OPTIONS.regions.map((region) => (
                            <button
                                type="button"
                                key={region}
                                className={
                                    preferences.regions.includes(region)
                                        ? "selected"
                                        : ""
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
                    </>
                );

            case 5:
                return (
                    <>
                        <h2>
                            Jak mają wyglądać rekomendacje?
                        </h2>

                        <p className="field-description">
                            Wybierz jedną opcję
                        </p>

                        {USER_PREFERENCE_OPTIONS.recommendationStyle.map((style) => (
                            <button
                                type="button"
                                key={style}
                                className={
                                    preferences.recommendationStyle === style
                                        ? "selected"
                                        : ""
                                }
                                onClick={() =>
                                    setPreferences({
                                        ...preferences,
                                        recommendationStyle: style,
                                    })
                                }
                            >
                                {style}
                            </button>
                        ))}

                        <div className="checkbox-wrapper">

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
                                Pokazuj również rekomendacje
                                spoza moich preferencji
                            </span>

                        </div>
                    </>
                );

            default:
                return null;
        }
    };

    return (
        <div className="onboarding-wrapper">

            <div className="onboarding-card">

                <div className="progress-bar">

                    <div
                        className="progress-fill"
                        style={{
                            width: `${
    ((step + 1) / steps.length) * 100
}%`,
                        }}
                    />

                </div>

                <p className="step-counter">
                    Krok {step + 1} z {steps.length}
                </p>

                <div className="step-content">
                    {renderStep()}
                </div>

                <div className="navigation-buttons">

                    {step > 0 && (
                        <button
                            type="button"
                            onClick={prevStep}
                        >
                            Wstecz
                        </button>
                    )}

                    <button
                        type="button"
                        onClick={nextStep}
                    >
                        {step === steps.length - 1
                            ? "Zakończ"
                            : "Dalej"}
                    </button>

                </div>

            </div>

        </div>
    );
}

