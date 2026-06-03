import { useEffect, useRef, useState } from "react";

import {
    createCountry,
    getCountries,
    searchCountries,
} from "../api/countryApi";

import "./FlavorProfilePicker.css";
import "./MultiSelectInput.css";

function CountryPicker({ value, onChange, onCountryChange }) {
    const [baseCountries, setBaseCountries] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [loadError, setLoadError] = useState("");
    const [showOther, setShowOther] = useState(false);
    const [searchQuery, setSearchQuery] = useState("");
    const [searchResults, setSearchResults] = useState([]);
    const [searchLoading, setSearchLoading] = useState(false);
    const [searchError, setSearchError] = useState("");
    const [infoMessage, setInfoMessage] = useState("");
    const searchWrapperRef = useRef(null);

    useEffect(() => {
        const loadCountries = async () => {
            try {
                setIsLoading(true);
                setLoadError("");

                const data = await getCountries();
                setBaseCountries(Array.isArray(data) ? data : []);
            } catch {
                setLoadError("Nie udało się pobrać krajów.");
            } finally {
                setIsLoading(false);
            }
        };

        loadCountries();
    }, []);

    useEffect(() => {
        if (!showOther) {
            return undefined;
        }

        const trimmedQuery = searchQuery.trim();

        if (!trimmedQuery) {
            setSearchResults([]);
            setSearchError("");
            return undefined;
        }

        const timeoutId = setTimeout(async () => {
            try {
                setSearchLoading(true);
                setSearchError("");

                const results = await searchCountries(trimmedQuery);
                setSearchResults(Array.isArray(results) ? results : []);
            } catch {
                setSearchResults([]);
                setSearchError("Nie udało się wyszukać krajów.");
            } finally {
                setSearchLoading(false);
            }
        }, 300);

        return () => clearTimeout(timeoutId);
    }, [searchQuery, showOther]);

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (
                searchWrapperRef.current &&
                !searchWrapperRef.current.contains(event.target)
            ) {
                setShowOther(false);
            }
        };

        document.addEventListener("mousedown", handleClickOutside);

        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
        };
    }, []);

    const baseCountryNames = new Set(
        baseCountries.map((country) => country.name)
    );

    const isCustomSelection =
        value && !baseCountryNames.has(value);

    const exactMatch = searchResults.find((result) => result.isExactMatch);
    const topFuzzyMatch = searchResults.find(
        (result) => result.isFuzzyMatch && !result.isExactMatch
    );

    const applySelection = (country) => {
        onChange(country.name);

        if (onCountryChange) {
            onCountryChange({
                id: country.id,
                name: country.name,
            });
        }
    };

    const selectCountry = (country) => {
        applySelection(country);
        setSearchQuery("");
        setSearchResults([]);
        setInfoMessage(`Wybrano kraj: ${country.name}`);
        setShowOther(false);
    };

    const toggleCountry = (country) => {
        if (value === country.name) {
            onChange("");

            if (onCountryChange) {
                onCountryChange({ id: null, name: "" });
            }

            return;
        }

        selectCountry(country);
    };

    const resolveCountryName = async (rawName) => {
        const trimmedName = rawName.trim();

        if (!trimmedName) {
            return null;
        }

        const results = await searchCountries(trimmedName);
        const exact = (Array.isArray(results) ? results : []).find(
            (result) => result.isExactMatch
        );

        if (exact) {
            return exact;
        }

        const created = await createCountry(trimmedName);
        return created;
    };

    const handleAddCustom = async () => {
        const trimmedQuery = searchQuery.trim();

        if (!trimmedQuery) {
            return;
        }

        try {
            setSearchError("");
            setInfoMessage("");

            if (exactMatch) {
                selectCountry(exactMatch);
                return;
            }

            const resolved = await resolveCountryName(trimmedQuery);

            if (!resolved?.name) {
                return;
            }

            if (resolved.name !== trimmedQuery) {
                setInfoMessage(`Użyto istniejącego kraju: ${resolved.name}`);
            }

            applySelection(resolved);

            if (!baseCountryNames.has(resolved.name)) {
                setBaseCountries((previous) =>
                    [...previous, resolved].sort((left, right) =>
                        left.name.localeCompare(right.name, "pl")
                    )
                );
            }

            setSearchQuery("");
            setSearchResults([]);
            setShowOther(false);
        } catch {
            setSearchError("Nie udało się dodać kraju.");
        }
    };

    const handleSearchKeyDown = (event) => {
        if (event.key === "Enter") {
            event.preventDefault();

            if (exactMatch) {
                selectCountry(exactMatch);
                return;
            }

            handleAddCustom();
        }
    };

    if (isLoading) {
        return (
            <p className="flavor-profile-picker-message">
                Ładowanie krajów...
            </p>
        );
    }

    if (loadError) {
        return (
            <p className="flavor-profile-picker-message flavor-profile-picker-error">
                {loadError}
            </p>
        );
    }

    return (
        <div className="flavor-profile-picker">
            <div className="multi-select">
                {baseCountries.map((country) => (
                    <button
                        key={country.id ?? country.name}
                        type="button"
                        className={
                            value === country.name
                                ? "multi-select-option selected"
                                : "multi-select-option"
                        }
                        onClick={() => toggleCountry(country)}
                    >
                        {country.name}
                    </button>
                ))}

                {isCustomSelection && (
                    <button
                        type="button"
                        className="multi-select-option selected"
                        onClick={() => toggleCountry({ id: null, name: value })}
                    >
                        {value}
                    </button>
                )}

                <button
                    type="button"
                    className={
                        showOther
                            ? "multi-select-option selected"
                            : "multi-select-option"
                    }
                    onClick={() => {
                        setShowOther((previous) => !previous);
                        setInfoMessage("");
                        setSearchError("");
                    }}
                >
                    Inne
                </button>
            </div>

            {showOther && (
                <div
                    className="flavor-profile-picker-other"
                    ref={searchWrapperRef}
                >
                    <input
                        type="text"
                        className="flavor-profile-picker-search"
                        value={searchQuery}
                        placeholder="Szukaj lub dodaj kraj..."
                        autoComplete="off"
                        onChange={(event) => {
                            setSearchQuery(event.target.value);
                            setInfoMessage("");
                        }}
                        onKeyDown={handleSearchKeyDown}
                    />

                    {searchLoading && (
                        <p className="flavor-profile-picker-message">
                            Szukam...
                        </p>
                    )}

                    {searchError && (
                        <p className="flavor-profile-picker-message flavor-profile-picker-error">
                            {searchError}
                        </p>
                    )}

                    {infoMessage && (
                        <p className="flavor-profile-picker-message flavor-profile-picker-info">
                            {infoMessage}
                        </p>
                    )}

                    {exactMatch && value !== exactMatch.name && (
                        <p className="flavor-profile-picker-message flavor-profile-picker-info">
                            Znaleziono istniejący kraj: {exactMatch.name}.
                            Wybierz go z listy poniżej.
                        </p>
                    )}

                    {topFuzzyMatch &&
                        !exactMatch &&
                        value !== topFuzzyMatch.name && (
                        <p className="flavor-profile-picker-message flavor-profile-picker-fuzzy">
                            Czy chodziło Ci o „{topFuzzyMatch.name}”?
                        </p>
                    )}

                    {searchResults.length > 0 && (
                        <ul
                            className="flavor-profile-picker-results"
                            role="listbox"
                        >
                            {searchResults
                                .filter((result) => result.name !== value)
                                .map((result) => (
                                    <li key={result.id}>
                                        <button
                                            type="button"
                                            className={
                                                result.isExactMatch
                                                    ? "flavor-profile-picker-result exact-match"
                                                    : result.isFuzzyMatch
                                                      ? "flavor-profile-picker-result fuzzy-match"
                                                      : "flavor-profile-picker-result"
                                            }
                                            onClick={() =>
                                                selectCountry(result)
                                            }
                                        >
                                            {result.name}
                                            {result.isExactMatch &&
                                                " — dokładne dopasowanie"}
                                            {result.isFuzzyMatch &&
                                                !result.isExactMatch &&
                                                " — podobny kraj"}
                                        </button>
                                    </li>
                                ))}
                        </ul>
                    )}

                    {searchQuery.trim() &&
                        !searchLoading &&
                        !exactMatch && (
                            <button
                                type="button"
                                className="flavor-profile-picker-add"
                                onClick={handleAddCustom}
                            >
                                Dodaj „{searchQuery.trim()}”
                            </button>
                        )}
                </div>
            )}
        </div>
    );
}

export default CountryPicker;
