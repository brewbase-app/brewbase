import { useEffect, useRef, useState } from "react";

import {
    createFlavorProfile,
    getFlavorProfiles,
    searchFlavorProfiles,
} from "../api/flavorProfileApi";

import "./FlavorProfilePicker.css";
import "./MultiSelectInput.css";

function FlavorProfilePicker({ value, onChange }) {
    const [baseProfiles, setBaseProfiles] = useState([]);
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
        const loadProfiles = async () => {
            try {
                setIsLoading(true);
                setLoadError("");

                const data = await getFlavorProfiles();
                setBaseProfiles(Array.isArray(data) ? data : []);
            } catch {
                setLoadError("Nie udało się pobrać profili smakowych.");
            } finally {
                setIsLoading(false);
            }
        };

        loadProfiles();
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

                const results = await searchFlavorProfiles(trimmedQuery);
                setSearchResults(Array.isArray(results) ? results : []);
            } catch {
                setSearchResults([]);
                setSearchError("Nie udało się wyszukać profili smakowych.");
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

    const baseProfileNames = new Set(
        baseProfiles.map((profile) => profile.name)
    );

    const customSelected = value.filter(
        (profileName) => !baseProfileNames.has(profileName)
    );

    const exactMatch = searchResults.find((result) => result.isExactMatch);
    const topFuzzyMatch = searchResults.find(
        (result) => result.isFuzzyMatch && !result.isExactMatch
    );

    const toggleProfile = (profileName) => {
        if (value.includes(profileName)) {
            onChange(value.filter((item) => item !== profileName));
            return;
        }

        onChange([...value, profileName]);
    };

    const addProfile = (profileName) => {
        if (!profileName || value.includes(profileName)) {
            return;
        }

        onChange([...value, profileName]);
    };

    const selectExistingProfile = (profile) => {
        addProfile(profile.name);
        setSearchQuery("");
        setSearchResults([]);
        setInfoMessage(`Przypięto istniejący profil: ${profile.name}`);
        setShowOther(false);
    };

    const resolveProfileName = async (rawName) => {
        const trimmedName = rawName.trim();

        if (!trimmedName) {
            return null;
        }

        const results = await searchFlavorProfiles(trimmedName);
        const exact = (Array.isArray(results) ? results : []).find(
            (result) => result.isExactMatch
        );

        if (exact) {
            return exact.name;
        }

        const created = await createFlavorProfile(trimmedName);
        return created.name;
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
                selectExistingProfile(exactMatch);
                return;
            }

            const resolvedName = await resolveProfileName(trimmedQuery);

            if (!resolvedName) {
                return;
            }

            if (resolvedName !== trimmedQuery) {
                setInfoMessage(
                    `Użyto istniejącego profilu: ${resolvedName}`
                );
            }

            addProfile(resolvedName);

            if (!baseProfileNames.has(resolvedName)) {
                setBaseProfiles((previous) =>
                    [...previous, { id: null, name: resolvedName }].sort(
                        (left, right) =>
                            left.name.localeCompare(right.name, "pl")
                    )
                );
            }

            setSearchQuery("");
            setSearchResults([]);
            setShowOther(false);
        } catch {
            setSearchError("Nie udało się dodać profilu smakowego.");
        }
    };

    const handleSearchKeyDown = (event) => {
        if (event.key === "Enter") {
            event.preventDefault();

            if (exactMatch) {
                selectExistingProfile(exactMatch);
                return;
            }

            handleAddCustom();
        }
    };

    if (isLoading) {
        return (
            <p className="flavor-profile-picker-message">
                Ładowanie profili smakowych...
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
                {baseProfiles.map((profile) => (
                    <button
                        key={profile.id ?? profile.name}
                        type="button"
                        className={
                            value.includes(profile.name)
                                ? "multi-select-option selected"
                                : "multi-select-option"
                        }
                        onClick={() => toggleProfile(profile.name)}
                    >
                        {profile.name}
                    </button>
                ))}

                {customSelected.map((profileName) => (
                    <button
                        key={profileName}
                        type="button"
                        className="multi-select-option selected"
                        onClick={() => toggleProfile(profileName)}
                    >
                        {profileName}
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
                        placeholder="Szukaj lub dodaj profil smakowy..."
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

                    {exactMatch && !value.includes(exactMatch.name) && (
                        <p className="flavor-profile-picker-message flavor-profile-picker-info">
                            Znaleziono istniejący profil: {exactMatch.name}.
                            Wybierz go z listy poniżej.
                        </p>
                    )}

                    {topFuzzyMatch &&
                        !exactMatch &&
                        !value.includes(topFuzzyMatch.name) && (
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
                                .filter(
                                    (result) => !value.includes(result.name)
                                )
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
                                                selectExistingProfile(result)
                                            }
                                        >
                                            {result.name}
                                            {result.isExactMatch &&
                                                " — dokładne dopasowanie"}
                                            {result.isFuzzyMatch &&
                                                !result.isExactMatch &&
                                                " — podobny profil"}
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

export default FlavorProfilePicker;
