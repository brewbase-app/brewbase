import { useEffect, useRef, useState } from "react";

import {
    createRoastery,
    getRoasteries,
    searchRoasteries,
} from "../api/roasteryApi";

import "./FlavorProfilePicker.css";
import "./MultiSelectInput.css";

function RoasteryPicker({ value, onChange }) {
    const [baseRoasteries, setBaseRoasteries] = useState([]);
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
        const loadRoasteries = async () => {
            try {
                setIsLoading(true);
                setLoadError("");

                const data = await getRoasteries();
                setBaseRoasteries(Array.isArray(data) ? data : []);
            } catch {
                setLoadError("Nie udało się pobrać palarni.");
            } finally {
                setIsLoading(false);
            }
        };

        loadRoasteries();
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

                const results = await searchRoasteries(trimmedQuery);
                setSearchResults(Array.isArray(results) ? results : []);
            } catch {
                setSearchResults([]);
                setSearchError("Nie udało się wyszukać palarni.");
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

    const refreshRoasteries = async () => {
        const data = await getRoasteries();
        setBaseRoasteries(Array.isArray(data) ? data : []);
    };

    const exactMatch = searchResults.find((result) => result.isExactMatch);
    const topFuzzyMatch = searchResults.find(
        (result) => result.isFuzzyMatch && !result.isExactMatch
    );

    const selectRoastery = (roastery) => {
        onChange(roastery.name);
        setSearchQuery("");
        setSearchResults([]);
        setInfoMessage(`Wybrano palarnię: ${roastery.name}`);
        setShowOther(false);
    };

    const toggleRoastery = (roastery) => {
        if (value === roastery.name) {
            onChange("");
            return;
        }

        selectRoastery(roastery);
    };

    const resolveRoasteryName = async (rawName) => {
        const trimmedName = rawName.trim();

        if (!trimmedName) {
            return null;
        }

        const results = await searchRoasteries(trimmedName);
        const exact = (Array.isArray(results) ? results : []).find(
            (result) => result.isExactMatch
        );

        if (exact) {
            return exact;
        }

        const created = await createRoastery(trimmedName);
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
                selectRoastery(exactMatch);
                return;
            }

            const resolved = await resolveRoasteryName(trimmedQuery);

            if (!resolved?.name) {
                return;
            }

            if (resolved.name !== trimmedQuery) {
                setInfoMessage(
                    `Użyto istniejącej palarni: ${resolved.name}`
                );
            }

            onChange(resolved.name);
            await refreshRoasteries();

            setSearchQuery("");
            setSearchResults([]);
            setShowOther(false);
        } catch {
            setSearchError("Nie udało się dodać palarni.");
        }
    };

    const handleSearchKeyDown = (event) => {
        if (event.key === "Enter") {
            event.preventDefault();

            if (exactMatch) {
                selectRoastery(exactMatch);
                return;
            }

            handleAddCustom();
        }
    };

    if (isLoading) {
        return (
            <p className="flavor-profile-picker-message">
                Ładowanie palarni...
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
                {baseRoasteries.map((roastery) => (
                    <button
                        key={roastery.id ?? roastery.name}
                        type="button"
                        className={
                            value === roastery.name
                                ? "multi-select-option selected"
                                : "multi-select-option"
                        }
                        onClick={() => toggleRoastery(roastery)}
                    >
                        {roastery.name}
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
                        placeholder="Szukaj lub dodaj palarnię..."
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
                            Znaleziono istniejącą palarnię: {exactMatch.name}.
                            Wybierz ją z listy poniżej.
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
                                                selectRoastery(result)
                                            }
                                        >
                                            {result.name}
                                            {result.isExactMatch &&
                                                " — dokładne dopasowanie"}
                                            {result.isFuzzyMatch &&
                                                !result.isExactMatch &&
                                                " — podobna palarnia"}
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

export default RoasteryPicker;
