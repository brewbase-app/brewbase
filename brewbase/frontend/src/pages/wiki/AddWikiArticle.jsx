import { useEffect, useState } from "react";

import { useNavigate } from "react-router-dom";

import "../../styles/wiki/AddWikiArticle.css";

import {
    Send,
    ChevronDown
} from "lucide-react";

import { createArticle } from "../../api/articlesApi";
import { lookupCoffeesByName } from "../../api/coffeeApi";

import ComboBoxInput from "../../components/ComboBoxInput";
import MultiSelectInput from "../../components/MultiSelectInput";

import { BEAN_ORIGIN_COUNTRIES } from "../../utils/beanOriginCountries";
import { COFFEE_VARIETIES } from "../../utils/coffeeVarieties";
import { COFFEE_PROCESSING_METHODS } from "../../utils/coffeeProcessingMethods";
import { BREWING_METHOD_OPTIONS } from "../../utils/brewingMethodOptions";
import { ROASTING_STYLE_OPTIONS } from "../../utils/roastingStyleOptions";
import { COFFEE_REGIONS } from "../../utils/coffeeRegions";
import { COFFEE_FLAVOR_PROFILES } from "../../utils/coffeeFlavorProfiles";

const CATEGORY_TO_MODULE = {
    coffee: "coffee",
    country: "country",
    brewing: "brewing_method",
    roastery: "roastery",
};

function AddWikiArticle() {

    const navigate = useNavigate();

    const [title, setTitle] = useState("");

    const [category, setCategory] = useState("");

    const [content, setContent] = useState("");

    const [beanOriginCountry, setBeanOriginCountry] = useState("");

    const [coffeeVariety, setCoffeeVariety] = useState("");

    const [coffeeProcessing, setCoffeeProcessing] = useState("");

    const [flavorProfiles, setFlavorProfiles] = useState([]);

    const [brewingMethod, setBrewingMethod] = useState("");

    const [roastingStyles, setRoastingStyles] = useState([]);

    const [countryRegion, setCountryRegion] = useState("");

    const [countryFlavorProfiles, setCountryFlavorProfiles] = useState([]);

    const [files, setFiles] = useState([]);

    const [isSubmitting, setIsSubmitting] = useState(false);

    const [submitError, setSubmitError] = useState("");

    const [linkedCoffeeId, setLinkedCoffeeId] = useState(null);

    const [linkedCoffeeName, setLinkedCoffeeName] = useState("");

    const [coffeeSuggestions, setCoffeeSuggestions] = useState([]);

    const [suggestionsLoading, setSuggestionsLoading] = useState(false);

    useEffect(() => {
        if (category !== "coffee") {
            setCoffeeSuggestions([]);
            return undefined;
        }

        const trimmedTitle = title.trim();

        if (trimmedTitle.length < 2) {
            setCoffeeSuggestions([]);
            return undefined;
        }

        const timeoutId = setTimeout(async () => {
            try {
                setSuggestionsLoading(true);

                const matches = await lookupCoffeesByName(trimmedTitle);

                setCoffeeSuggestions(Array.isArray(matches) ? matches : []);
            } catch {
                setCoffeeSuggestions([]);
            } finally {
                setSuggestionsLoading(false);
            }
        }, 400);

        return () => clearTimeout(timeoutId);
    }, [category, title]);

    const handleSelectLinkedCoffee = (coffee) => {
        setLinkedCoffeeId(coffee.id);
        setLinkedCoffeeName(coffee.name);
        setCoffeeSuggestions([]);
    };

    const handleClearLinkedCoffee = () => {
        setLinkedCoffeeId(null);
        setLinkedCoffeeName("");
    };

    const handleSubmit = async () => {
        const module = CATEGORY_TO_MODULE[category];

        const articleTitle =
            category === "brewing"
                ? brewingMethod.trim()
                : title.trim();

        if (!module || !articleTitle || !content.trim()) {
            setSubmitError(
                "Uzupełnij wymagane pola: tytuł, opis i kategorię."
            );
            return;
        }

        if (category === "coffee" && !beanOriginCountry.trim()) {
            setSubmitError(
                "Podaj kraj pochodzenia ziaren."
            );
            return;
        }

        if (category === "coffee" && !coffeeVariety.trim()) {
            setSubmitError(
                "Podaj odmianę."
            );
            return;
        }

        if (category === "coffee" && !coffeeProcessing.trim()) {
            setSubmitError(
                "Podaj obróbkę ziaren."
            );
            return;
        }

        if (category === "roastery" && roastingStyles.length === 0) {
            setSubmitError(
                "Wybierz co najmniej jeden styl palenia."
            );
            return;
        }

        if (category === "country" && !title.trim()) {
            setSubmitError(
                "Podaj nazwę kraju."
            );
            return;
        }

        if (category === "country" && !countryRegion.trim()) {
            setSubmitError(
                "Podaj region."
            );
            return;
        }

        try {
            setIsSubmitting(true);
            setSubmitError("");

            let articleContent = content.trim();

            if (category === "coffee") {
                const metadataLines = [
                    `Kraj pochodzenia ziaren: ${beanOriginCountry.trim()}`,
                    `Odmiana: ${coffeeVariety.trim()}`,
                    `Obróbka ziaren: ${coffeeProcessing.trim()}`,
                ];

                if (flavorProfiles.length > 0) {
                    metadataLines.push(
                        `Profil smakowy: ${flavorProfiles.join(", ")}`
                    );
                }

                articleContent =
                    `${metadataLines.join("\n")}\n\n` +
                    articleContent;
            }

            if (category === "roastery") {
                articleContent =
                    `Styl palenia: ${roastingStyles.join(", ")}\n\n` +
                    articleContent;
            }

            if (category === "country") {
                const metadataLines = [
                    `Region: ${countryRegion.trim()}`,
                ];

                if (countryFlavorProfiles.length > 0) {
                    metadataLines.push(
                        `Profil smakowy: ${countryFlavorProfiles.join(", ")}`
                    );
                }

                articleContent =
                    `${metadataLines.join("\n")}\n\n` +
                    articleContent;
            }

            await createArticle({
                title: articleTitle,
                content: articleContent,
                module,
                coffeeId: linkedCoffeeId ?? undefined,
            });

            navigate("/wiki", {
                state: { articleSubmitted: true },
            });
        } catch {
            setSubmitError(
                "Nie udało się wysłać artykułu. Sprawdź, czy jesteś zalogowany."
            );
        } finally {
            setIsSubmitting(false);
        }
    };

    return (

        <div className="add-article-page">

            <div className="add-article-container">

                <div className="add-article-header">

                    <h1>
                        Dodaj treść do wiki
                    </h1>

                    <p>
                        Dodaj własny artykuł do encyklopedii kawy.
                        <br />
                        Po przesłaniu treść zostanie przekazana
                        do moderacji i akceptacji.
                    </p>

                </div>

                <div className="article-form">

                    {/* CATEGORY */}

                    <div className="form-group">

                        <label>
                            Kategoria
                        </label>

                        <div className="select-wrapper category-select">

                            <select
                                value={category}
                                onChange={(e) =>
                                    setCategory(e.target.value)
                                }
                            >

                                <option value="">
                                    Wybierz kategorię
                                </option>

                                <option value="coffee">
                                    Kawy
                                </option>

                                <option value="country">
                                    Kraje
                                </option>

                                <option value="brewing">
                                    Metody parzenia
                                </option>

                                <option value="roastery">
                                    Palarnie
                                </option>

                            </select>

                            <ChevronDown size={18} />

                        </div>

                    </div>

                    {/* COFFEES */}

                    {category === "coffee" && (

                        <>

                            <div className="form-group">

                                <label>
                                    Nazwa kawy
                                </label>

                                <input
                                    type="text"
                                    placeholder="Np. Geisha z Panamy"
                                    value={title}
                                    onChange={(e) => {
                                        setTitle(e.target.value);

                                        if (
                                            linkedCoffeeId &&
                                            e.target.value.trim() !== linkedCoffeeName
                                        ) {
                                            handleClearLinkedCoffee();
                                        }
                                    }}
                                />

                                {linkedCoffeeId && (
                                    <p className="linked-coffee-notice">
                                        Powiązana kawa w katalogu:{" "}
                                        <strong>{linkedCoffeeName}</strong>
                                        <button
                                            type="button"
                                            className="link-button"
                                            onClick={handleClearLinkedCoffee}
                                        >
                                            Usuń powiązanie
                                        </button>
                                    </p>
                                )}

                                {!linkedCoffeeId &&
                                    title.trim().length >= 2 && (
                                        <div className="coffee-suggestions">
                                            {suggestionsLoading ? (
                                                <p>Szukam podobnych kaw...</p>
                                            ) : coffeeSuggestions.length > 0 ? (
                                                <>
                                                    <p>
                                                        Podobne kawy w katalogu:
                                                    </p>
                                                    <ul>
                                                        {coffeeSuggestions.map(
                                                            (coffee) => (
                                                                <li
                                                                    key={
                                                                        coffee.id
                                                                    }
                                                                >
                                                                    {
                                                                        coffee.name
                                                                    }
                                                                    <button
                                                                        type="button"
                                                                        className="link-button"
                                                                        onClick={() =>
                                                                            handleSelectLinkedCoffee(
                                                                                coffee
                                                                            )
                                                                        }
                                                                    >
                                                                        Powiąż
                                                                    </button>
                                                                </li>
                                                            )
                                                        )}
                                                    </ul>
                                                    <p>
                                                        Możesz też pominąć
                                                        sugestie i wysłać artykuł
                                                        bez powiązania.
                                                    </p>
                                                </>
                                            ) : null}
                                        </div>
                                    )}

                            </div>

                            <div className="form-group">

                                <label>
                                    Kraj pochodzenia ziaren
                                </label>

                                <ComboBoxInput
                                    value={beanOriginCountry}
                                    onChange={setBeanOriginCountry}
                                    options={BEAN_ORIGIN_COUNTRIES}
                                    placeholder="Wybierz z listy lub wpisz kraj"
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Odmiana
                                </label>

                                <ComboBoxInput
                                    value={coffeeVariety}
                                    onChange={setCoffeeVariety}
                                    options={COFFEE_VARIETIES}
                                    placeholder="Wybierz z listy lub wpisz odmianę"
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Obróbka ziaren
                                </label>

                                <ComboBoxInput
                                    value={coffeeProcessing}
                                    onChange={setCoffeeProcessing}
                                    options={COFFEE_PROCESSING_METHODS}
                                    placeholder="Wybierz z listy lub wpisz obróbkę"
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Profil smakowy
                                </label>

                                <MultiSelectInput
                                    options={COFFEE_FLAVOR_PROFILES}
                                    value={flavorProfiles}
                                    onChange={setFlavorProfiles}
                                    allowCustom
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Opis kawy
                                </label>

                                <textarea
                                    placeholder="Dodaj opis kawy..."
                                    value={content}
                                    onChange={(e) =>
                                        setContent(e.target.value)
                                    }
                                />

                            </div>

                        </>

                    )}

                    {/* COUNTRIES */}

                    {category === "country" && (

                        <>

                            <div className="form-group">

                                <label>
                                    Nazwa kraju
                                </label>

                                <ComboBoxInput
                                    value={title}
                                    onChange={setTitle}
                                    options={BEAN_ORIGIN_COUNTRIES}
                                    placeholder="Wybierz z listy lub wpisz kraj"
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Region
                                </label>

                                <ComboBoxInput
                                    value={countryRegion}
                                    onChange={setCountryRegion}
                                    options={COFFEE_REGIONS}
                                    placeholder="Wybierz z listy lub wpisz region"
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Profil smakowy
                                </label>

                                <MultiSelectInput
                                    options={COFFEE_FLAVOR_PROFILES}
                                    value={countryFlavorProfiles}
                                    onChange={setCountryFlavorProfiles}
                                    allowCustom
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Opis
                                </label>

                                <textarea
                                    placeholder="Dodaj opis..."
                                    value={content}
                                    onChange={(e) =>
                                        setContent(e.target.value)
                                    }
                                />

                            </div>

                        </>

                    )}

                    {/* BREWING */}

                    {category === "brewing" && (

                        <>

                            <div className="form-group">

                                <label>
                                    Metoda parzenia
                                </label>

                                <ComboBoxInput
                                    value={brewingMethod}
                                    onChange={setBrewingMethod}
                                    options={BREWING_METHOD_OPTIONS}
                                    placeholder="Wybierz z listy lub wpisz metodę"
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Opis metody parzenia
                                </label>

                                <textarea
                                    placeholder="Dodaj opis metody..."
                                    value={content}
                                    onChange={(e) =>
                                        setContent(e.target.value)
                                    }
                                />

                            </div>

                        </>

                    )}

                    {/* ROASTERIES */}

                    {category === "roastery" && (

                        <>

                            <div className="form-group">

                                <label>
                                    Nazwa palarni
                                </label>

                                <input
                                    type="text"
                                    placeholder="Np. Coffee Collective"
                                    value={title}
                                    onChange={(e) =>
                                        setTitle(e.target.value)
                                    }
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Miasto
                                </label>

                                <input
                                    type="text"
                                    placeholder="Np. Kopenhaga"
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Kraj
                                </label>

                                <input
                                    type="text"
                                    placeholder="Np. Dania"
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Rok założenia
                                </label>

                                <input
                                    type="text"
                                    placeholder="Np. 2007"
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Styl palenia
                                </label>

                                <MultiSelectInput
                                    options={ROASTING_STYLE_OPTIONS}
                                    value={roastingStyles}
                                    onChange={setRoastingStyles}
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Opis palarni
                                </label>

                                <textarea
                                    placeholder="Dodaj opis palarni..."
                                    value={content}
                                    onChange={(e) =>
                                        setContent(e.target.value)
                                    }
                                />

                            </div>

                        </>

                    )}

                    {/* IMAGES + ACTIONS */}

                    {category && (

                        <>

                            <div className="form-group">

                                <label>
                                    Zdjęcia
                                </label>

                                <label className="upload-box">

                                    <input
                                        type="file"
                                        accept="image/*"
                                        multiple
                                        onChange={(e) =>
                                            setFiles(
                                                [...e.target.files]
                                            )
                                        }
                                    />

                                    <span>
                                        Przeciągnij zdjęcia lub kliknij,
                                        aby dodać fotografie
                                    </span>

                                </label>

                                {files.length > 0 && (

                                    <div className="uploaded-files">

                                        {files.map((file, index) => (

                                            <div
                                                key={index}
                                                className="uploaded-file"
                                            >
                                                {file.name}
                                            </div>

                                        ))}

                                    </div>

                                )}

                            </div>

                            <div className="article-actions">

                                {submitError && (
                                    <p className="submit-error">
                                        {submitError}
                                    </p>
                                )}

                                <button
                                    className="submit-article-button"
                                    type="button"
                                    onClick={handleSubmit}
                                    disabled={isSubmitting}
                                >

                                    <Send size={16} />

                                    {isSubmitting
                                        ? "Wysyłanie..."
                                        : "Wyślij do moderacji"}

                                </button>

                            </div>

                        </>

                    )}

                </div>

            </div>

        </div>

    );
}

export default AddWikiArticle;