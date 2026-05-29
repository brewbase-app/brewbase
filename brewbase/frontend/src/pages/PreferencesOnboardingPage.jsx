import { useState } from "react";
import { useNavigate } from "react-router-dom";

import "../styles/PreferencesOnboardingPage.css";

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
        experienceLevel: "",
        brewingMethods: [],
        flavorProfiles: [],
        acidity: "",
        body: "",
        regions: [],
        processingMethods: [],
        recommendationStyle: "",
        allowExploration: false,
    });

    const nextStep = async () => {

        if (step < steps.length - 1) {

            setStep(step + 1);

        } else {

            navigate("/home");
        }
    };

    const prevStep = () => {

        if (step > 0) {
            setStep(step - 1);
        }
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

                        {[
                            "Początkujący",
                            "Średniozaawansowany",
                            "Zaawansowany",
                            "Jeszcze nie wiem",
                        ].map((option) => (
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
                                        ? "selected"
                                        : ""
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
                    </>
                );

            case 2:
                return (
                    <>
                        <h2>
                            Jakie profile smakowe preferujesz?
                        </h2>

                        <p className="field-description">
                            Możesz wybrać kilka opcji
                        </p>

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
                                        ? "selected"
                                        : ""
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

                        {[
                            "Niska",
                            "Średnia",
                            "Wysoka",
                            "Nie mam zdania",
                        ].map((option) => (
                            <button
                                type="button"
                                key={option}
                                className={
                                    preferences.acidity === option
                                        ? "selected"
                                        : ""
                                }
                                onClick={() =>
                                    setPreferences({
                                        ...preferences,
                                        acidity: option,
                                    })
                                }
                            >
                                {option}
                            </button>
                        ))}

                        <h2 className="secondary-title">
                            Preferowane body
                        </h2>

                        <p className="field-description">
                            Wybierz jedną opcję
                        </p>

                        {[
                            "Lekkie",
                            "Zbalansowane",
                            "Ciężkie",
                            "Nie mam zdania",
                        ].map((option) => (
                            <button
                                type="button"
                                key={option}
                                className={
                                    preferences.body === option
                                        ? "selected"
                                        : ""
                                }
                                onClick={() =>
                                    setPreferences({
                                        ...preferences,
                                        body: option,
                                    })
                                }
                            >
                                {option}
                            </button>
                        ))}
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

                        {[
                            "Bezpieczne wybory",
                            "Zbalansowane",
                            "Zaskocz mnie",
                        ].map((style) => (
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

