import { useEffect, useMemo, useState } from "react";

import { useNavigate } from "react-router-dom";

import "../../styles/wiki/Regions.css";

import { Search } from "lucide-react";

import { getArticles } from "../../api/articlesApi";
import { getFlavorProfiles } from "../../api/flavorProfileApi";

import MultiSelectInput from "../../components/MultiSelectInput";

import { BEAN_ORIGIN_COUNTRIES } from "../../utils/beanOriginCountries";
import {
    buildFilterOptions,
    parseCountryArticleMetadata,
} from "../../utils/parseCoffeeArticleMetadata";

function mapCountryArticle(article) {
    const metadata = parseCountryArticleMetadata(article.content ?? "");

    return {
        ...article,
        region: article.region ?? metadata.region,
        flavorProfiles:
            Array.isArray(article.flavorProfiles) &&
            article.flavorProfiles.length > 0
                ? article.flavorProfiles
                : metadata.flavorProfiles,
    };
}

function Regions() {
    const navigate = useNavigate();

    const [articles, setArticles] = useState([]);
    const [search, setSearch] = useState("");
    const [selectedCountry, setSelectedCountry] = useState("");
    const [selectedFlavorProfiles, setSelectedFlavorProfiles] = useState([]);
    const [flavorProfileNames, setFlavorProfileNames] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadArticles = async () => {
            try {
                setIsLoading(true);
                setError("");

                const [data, flavorProfilesData] = await Promise.all([
                    getArticles("country"),
                    getFlavorProfiles(),
                ]);

                const mappedArticles = (Array.isArray(data) ? data : []).map(
                    mapCountryArticle
                );

                setArticles(mappedArticles);
                setFlavorProfileNames(
                    (Array.isArray(flavorProfilesData)
                        ? flavorProfilesData
                        : []
                    ).map((profile) => profile.name)
                );
            } catch {
                setError("Nie udało się pobrać artykułów o krajach.");
            } finally {
                setIsLoading(false);
            }
        };

        loadArticles();
    }, []);

    const countries = buildFilterOptions(
        BEAN_ORIGIN_COUNTRIES,
        articles.map((article) => article.title)
    );

    const flavorProfileOptions = useMemo(
        () =>
            buildFilterOptions(
                flavorProfileNames,
                articles.flatMap((article) =>
                    Array.isArray(article.flavorProfiles)
                        ? article.flavorProfiles
                        : []
                )
            ),
        [flavorProfileNames, articles]
    );

    const filteredArticles = articles.filter((article) => {
        const query = search.toLowerCase();
        const flavorProfiles = Array.isArray(article.flavorProfiles)
            ? article.flavorProfiles
            : [];

        const matchesSearch =
            (article.title ?? "")
                .toLowerCase()
                .includes(query) ||
            (article.region ?? "")
                .toLowerCase()
                .includes(query) ||
            flavorProfiles.some((profile) =>
                profile.toLowerCase().includes(query)
            );

        const matchesCountry =
            selectedCountry === "" ||
            article.title === selectedCountry;

        const matchesFlavorProfiles =
            selectedFlavorProfiles.length === 0 ||
            selectedFlavorProfiles.some((profile) =>
                flavorProfiles.includes(profile)
            );

        return matchesSearch && matchesCountry && matchesFlavorProfiles;
    });

    if (isLoading) {
        return (
            <div className="regions-page">
                <h1>Ładowanie krajów...</h1>
            </div>
        );
    }

    if (error) {
        return (
            <div className="regions-page">
                <h1>{error}</h1>
            </div>
        );
    }

    return (
        <div className="regions-page">
            <div className="regions-header">
                <h1>Kraje</h1>

                <p>
                    Poznaj kraje pochodzenia kaw specialty.
                </p>

                <div className="regions-search-container">
                    <Search size={18} />

                    <input
                        type="text"
                        placeholder="Szukaj krajów..."
                        className="regions-search"
                        value={search}
                        onChange={(event) =>
                            setSearch(event.target.value)
                        }
                    />
                </div>
            </div>

            <div className="regions-content">
                <div className="regions-filters">
                    <h3>Filtry</h3>

                    <div className="filter-group">
                        <label>Kraj</label>

                        <select
                            value={selectedCountry}
                            onChange={(event) =>
                                setSelectedCountry(event.target.value)
                            }
                        >
                            <option value="">
                                Wszystkie
                            </option>

                            {countries.map((country) => (
                                <option
                                    key={country}
                                    value={country}
                                >
                                    {country}
                                </option>
                            ))}
                        </select>
                    </div>

                    <div className="filter-group">
                        <label>Profil smakowy</label>

                        <MultiSelectInput
                            options={flavorProfileOptions}
                            value={selectedFlavorProfiles}
                            onChange={setSelectedFlavorProfiles}
                            allowCustom
                            customPlaceholder="Inny..."
                        />
                    </div>
                </div>

                {filteredArticles.length === 0 ? (
                    <p className="regions-empty-state">
                        {articles.length === 0
                            ? "Brak opublikowanych artykułów o krajach."
                            : "Brak wyników dla wybranych filtrów."}
                    </p>
                ) : (
                    <div className="regions-grid">
                        {filteredArticles.map((article) => {
                            const flavorProfiles = Array.isArray(
                                article.flavorProfiles
                            )
                                ? article.flavorProfiles
                                : [];

                            return (
                                <div
                                    className="region-card"
                                    key={article.id}
                                    onClick={() =>
                                        navigate(
                                            `/wiki/articles/${article.id}`
                                        )
                                    }
                                >
                                    <div className="region-image" />

                                    <div className="region-card-content">
                                        <div className="region-title">
                                            <h2>
                                                {article.title}
                                            </h2>
                                        </div>

                                        {article.region && (
                                            <p>
                                                {article.region}
                                            </p>
                                        )}

                                        {flavorProfiles.length > 0 && (
                                            <div className="region-tags">
                                                {flavorProfiles.map(
                                                    (profile) => (
                                                        <span key={profile}>
                                                            {profile}
                                                        </span>
                                                    )
                                                )}
                                            </div>
                                        )}
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                )}
            </div>
        </div>
    );
}

export default Regions;
