import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Search } from "lucide-react";

import { globalSearch } from "../api/searchApi";

const TYPE_LABELS = {
    coffee: "Kawa",
    recipe: "Receptura",
    user: "Użytkownik",
    wiki: "Wiki",
    quick_note: "Szybka notatka",
    cupping: "Cupping",
};

function GlobalSearch() {
    const navigate = useNavigate();
    const containerRef = useRef(null);

    const [query, setQuery] = useState("");
    const [results, setResults] = useState([]);
    const [isOpen, setIsOpen] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState("");

    useEffect(() => {
        const trimmed = query.trim();

        if (!trimmed) {
            setResults([]);
            setError("");
            setIsLoading(false);
            return undefined;
        }

        setIsLoading(true);
        setError("");

        const timeoutId = setTimeout(async () => {
            try {
                const data = await globalSearch(trimmed);
                setResults(data?.results ?? []);
            } catch {
                setResults([]);
                setError("Nie udało się wyszukać. Spróbuj ponownie.");
            } finally {
                setIsLoading(false);
            }
        }, 350);

        return () => clearTimeout(timeoutId);
    }, [query]);

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (
                containerRef.current
                && !containerRef.current.contains(event.target)
            ) {
                setIsOpen(false);
            }
        };

        document.addEventListener("mousedown", handleClickOutside);
        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, []);

    const trimmedQuery = query.trim();
    const showPanel = isOpen && trimmedQuery.length > 0;

    const handleSelect = (result) => {
        setIsOpen(false);
        setQuery("");
        setResults([]);
        navigate(result.path);
    };

    return (
        <div className="global-search" ref={containerRef}>
            <div className="search-bar">
                <Search size={18} />

                <input
                    type="text"
                    placeholder="Szukaj kaw, receptur, metod..."
                    value={query}
                    onChange={(event) => {
                        setQuery(event.target.value);
                        setIsOpen(true);
                    }}
                    onFocus={() => setIsOpen(true)}
                    aria-label="Wyszukiwarka globalna"
                    aria-expanded={showPanel}
                    aria-controls="global-search-results"
                />
            </div>

            {showPanel && (
                <div
                    id="global-search-results"
                    className="global-search-dropdown"
                    role="listbox"
                >
                    {isLoading && (
                        <div className="global-search-message">
                            Wyszukiwanie...
                        </div>
                    )}

                    {!isLoading && error && (
                        <div className="global-search-message global-search-error">
                            {error}
                        </div>
                    )}

                    {!isLoading && !error && results.length === 0 && (
                        <div className="global-search-message">
                            Brak wyników dla „{trimmedQuery}”.
                        </div>
                    )}

                    {!isLoading && !error && results.length > 0 && (
                        <ul className="global-search-results">
                            {results.map((result) => (
                                <li key={`${result.type}-${result.id}`}>
                                    <button
                                        type="button"
                                        className="global-search-item"
                                        onClick={() => handleSelect(result)}
                                    >
                                        <span className="global-search-type">
                                            {TYPE_LABELS[result.type] ?? result.type}
                                        </span>
                                        <span className="global-search-title">
                                            {result.title}
                                        </span>
                                        {result.snippet && (
                                            <span className="global-search-snippet">
                                                {result.snippet}
                                            </span>
                                        )}
                                    </button>
                                </li>
                            ))}
                        </ul>
                    )}
                </div>
            )}
        </div>
    );
}

export default GlobalSearch;
