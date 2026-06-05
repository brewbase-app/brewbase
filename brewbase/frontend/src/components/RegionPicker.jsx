import { useEffect, useRef, useState } from "react";

import {
    createRegion,
    getRegions,
    searchRegions,
} from "../api/regionApi";

import "./FlavorProfilePicker.css";
import "./MultiSelectInput.css";

function RegionPicker({ countryId, value, onChange, disabled = false }) {
    const [baseRegions, setBaseRegions] = useState([]);
    const [isLoading, setIsLoading] = useState(false);
    const [loadError, setLoadError] = useState("");
    const [showOther, setShowOther] = useState(false);
    const [searchQuery, setSearchQuery] = useState("");
    const [searchResults, setSearchResults] = useState([]);
    const [searchLoading, setSearchLoading] = useState(false);
    const [searchError, setSearchError] = useState("");
    const [infoMessage, setInfoMessage] = useState("");
    const searchWrapperRef = useRef(null);

    useEffect(() => {
        if (!countryId || disabled) {
            setBaseRegions([]);
            setLoadError("");
            setIsLoading(false);
            setShowOther(false);
            return undefined;
        }

        let cancelled = false;

        const loadRegions = async () => {
            try {
                setIsLoading(true);
                setLoadError("");

                const data = await getRegions(countryId);

                if (cancelled) {
                    return;
                }

                setBaseRegions(Array.isArray(data) ? data : []);
            } catch {
                if (!cancelled) {
                    setBaseRegions([]);
                    setLoadError("Nie udało się pobrać regionów.");
                }
            } finally {
                if (!cancelled) {
                    setIsLoading(false);
                }
            }
        };

        loadRegions();

        return () => {
            cancelled = true;
        };
    }, [countryId, disabled]);

    useEffect(() => {
        if (!showOther || !countryId || disabled) {
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

                const results = await searchRegions(countryId, trimmedQuery);
                setSearchResults(Array.isArray(results) ? results : []);
            } catch {
                setSearchResults([]);
                setSearchError("Nie udało się wyszukać regionów.");
            } finally {
                setSearchLoading(false);
            }
        }, 300);

        return () => clearTimeout(timeoutId);
    }, [searchQuery, showOther, countryId, disabled]);

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

    if (!countryId || disabled) {
        return (
            <p className="flavor-profile-picker-message">
                Najpierw wybierz kraj.
            </p>
        );
    }

    const refreshRegions = async () => {
        const data = await getRegions(countryId);
        setBaseRegions(Array.isArray(data) ? data : []);
    };

    const exactMatch = searchResults.find((result) => result.isExactMatch);
    const topFuzzyMatch = searchResults.find(
        (result) => result.isFuzzyMatch && !result.isExactMatch
    );

    const selectRegion = (region) => {
        onChange(region.name);
        setSearchQuery("");
        setSearchResults([]);
        setInfoMessage(`Wybrano region: ${region.name}`);
        setShowOther(false);
    };

    const toggleRegion = (region) => {
        if (value === region.name) {
            onChange("");
            return;
        }

        selectRegion(region);
    };

    const resolveRegionName = async (rawName) => {
        const trimmedName = rawName.trim();

        if (!trimmedName) {
            return null;
        }

        const results = await searchRegions(countryId, trimmedName);
        const exact = (Array.isArray(results) ? results : []).find(
            (result) => result.isExactMatch
        );

        if (exact) {
            return exact;
        }

        const created = await createRegion(trimmedName, countryId);
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
                selectRegion(exactMatch);
                return;
            }

            const resolved = await resolveRegionName(trimmedQuery);

            if (!resolved?.name) {
                return;
            }

            if (resolved.name !== trimmedQuery) {
                setInfoMessage(
                    `Użyto istniejącego regionu: ${resolved.name}`
                );
            }

            onChange(resolved.name);
            await refreshRegions();

            setSearchQuery("");
            setSearchResults([]);
            setShowOther(false);
        } catch {
            setSearchError("Nie udało się dodać regionu.");
        }
    };

    const handleSearchKeyDown = (event) => {
        if (event.key === "Enter") {
            event.preventDefault();

            if (exactMatch) {
                selectRegion(exactMatch);
                return;
            }

            handleAddCustom();
        }
    };

    if (isLoading) {
        return (
            <p className="flavor-profile-picker-message">
                Ładowanie regionów...
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
                {baseRegions.map((region) => (
                    <button
                        key={region.id ?? region.name}
                        type="button"
                        className={
                            value === region.name
                                ? "multi-select-option selected"
                                : "multi-select-option"
                        }
                        onClick={() => toggleRegion(region)}
                    >
                        {region.name}
                    </button>
                ))}

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
                        placeholder="Szukaj lub dodaj region..."
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
                            Znaleziono istniejący region: {exactMatch.name}.
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
                                                selectRegion(result)
                                            }
                                        >
                                            {result.name}
                                            {result.isExactMatch &&
                                                " — dokładne dopasowanie"}
                                            {result.isFuzzyMatch &&
                                                !result.isExactMatch &&
                                                " — podobny region"}
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

export default RegionPicker;
