import { useEffect, useState } from "react";

import "../../styles/wiki/Coffees.css";

import { useNavigate } from "react-router-dom";

import {
    Search,
    Heart,
    Star
} from "lucide-react";

import {
    addCoffeeFavorite,
    getCoffees,
    removeCoffeeFavorite
} from "../../api/coffeeApi";

import { getArticles } from "../../api/articlesApi";

import MultiSelectInput from "../../components/MultiSelectInput";

import { BEAN_ORIGIN_COUNTRIES } from "../../utils/beanOriginCountries";
import { COFFEE_VARIETIES } from "../../utils/coffeeVarieties";
import { COFFEE_PROCESSING_METHODS } from "../../utils/coffeeProcessingMethods";
import { COFFEE_FLAVOR_PROFILES } from "../../utils/coffeeFlavorProfiles";
import {
    buildFilterOptions,
    parseCoffeeArticleMetadata,
} from "../../utils/parseCoffeeArticleMetadata";

function mapCoffeeArticle(article) {
    const metadata = parseCoffeeArticleMetadata(article.content ?? "");

    return {
        key: `article-${article.id}`,
        name: article.title,
        beanOriginCountry:
            article.beanOriginCountry ?? metadata.beanOriginCountry,
        variety: article.variety ?? metadata.variety,
        processingMethod:
            article.processingMethod ?? metadata.processingMethod,
        flavorProfiles:
            Array.isArray(article.flavorProfiles) &&
            article.flavorProfiles.length > 0
                ? article.flavorProfiles
                : metadata.flavorProfiles,
        roastery: article.roastery ?? metadata.roastery,
        authorLogin: article.authorLogin ?? null,
        isWikiArticle: true,
        coffeeId: article.coffeeId ?? null,
        articleId: article.id,
        averageRating: null,
        ratingCount: 0,
        isFavorite: false,
    };
}

function Coffees() {
    const navigate = useNavigate();

    const [coffees, setCoffees] = useState([]);
    const [articles, setArticles] = useState([]);
    const [search, setSearch] = useState("");
    const [selectedOriginCountry, setSelectedOriginCountry] = useState("");
    const [selectedProcessing, setSelectedProcessing] = useState("");
    const [selectedVariety, setSelectedVariety] = useState("");
    const [selectedFlavorProfiles, setSelectedFlavorProfiles] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadData = async () => {
            try {
                setIsLoading(true);
                setError("");

                const [coffeeData, articleData] = await Promise.all([
                    getCoffees(),
                    getArticles("coffee"),
                ]);

                setCoffees(Array.isArray(coffeeData) ? coffeeData : []);
                setArticles(Array.isArray(articleData) ? articleData : []);
            } catch {
                setError("Nie udało się pobrać kaw.");
            } finally {
                setIsLoading(false);
            }
        };

        loadData();
    }, []);

    const catalogItems = coffees.map((coffee) => ({
        key: `catalog-${coffee.id}`,
        name: coffee.name,
        beanOriginCountry:
            coffee.beanOriginCountry ?? coffee.region ?? null,
        variety: coffee.variety ?? null,
        processingMethod: coffee.processingMethod ?? null,
        flavorProfiles: Array.isArray(coffee.flavorProfiles)
            ? coffee.flavorProfiles
            : [],
        roastery: coffee.roastery ?? null,
        authorLogin: null,
        isWikiArticle: false,
        coffeeId: coffee.id,
        articleId: null,
        averageRating: coffee.averageRating ?? null,
        ratingCount: coffee.ratingCount ?? 0,
        isFavorite: coffee.isFavorite ?? false,
    }));

    const catalogCoffeeIds = new Set(
        catalogItems
            .map((item) => item.coffeeId)
            .filter((coffeeId) => coffeeId != null)
    );

    const catalogNames = new Set(
        catalogItems.map((item) => (item.name ?? "").trim().toLowerCase())
    );

    const articleItems = articles
        .map(mapCoffeeArticle)
        .filter((item) => {
            if (item.coffeeId != null && catalogCoffeeIds.has(item.coffeeId)) {
                return false;
            }

            const normalizedName = (item.name ?? "").trim().toLowerCase();
            return normalizedName === "" || !catalogNames.has(normalizedName);
        });

    const allItems = [...catalogItems, ...articleItems];

    const originCountries = buildFilterOptions(
        BEAN_ORIGIN_COUNTRIES,
        catalogItems.map((item) => item.beanOriginCountry),
        articleItems.map((item) => item.beanOriginCountry)
    );

    const processingMethods = buildFilterOptions(
        COFFEE_PROCESSING_METHODS,
        catalogItems.map((item) => item.processingMethod),
        articleItems.map((item) => item.processingMethod)
    );

    const varieties = buildFilterOptions(
        COFFEE_VARIETIES,
        catalogItems.map((item) => item.variety),
        articleItems.map((item) => item.variety)
    );

    const flavorProfileOptions = buildFilterOptions(
        COFFEE_FLAVOR_PROFILES,
        catalogItems.flatMap((item) => item.flavorProfiles),
        articleItems.flatMap((item) => item.flavorProfiles)
    );

    const handleFavorite = async (coffeeId, event) => {
        event.stopPropagation();

        const coffee = coffees.find((item) => item.id === coffeeId);
        if (!coffee) {
            return;
        }

        const wasFavorite = coffee.isFavorite ?? false;

        setCoffees((previous) =>
            previous.map((item) =>
                item.id === coffeeId
                    ? { ...item, isFavorite: !wasFavorite }
                    : item
            )
        );

        try {
            if (wasFavorite) {
                await removeCoffeeFavorite(coffeeId);
            } else {
                await addCoffeeFavorite(coffeeId);
            }
        } catch {
            setCoffees((previous) =>
                previous.map((item) =>
                    item.id === coffeeId
                        ? { ...item, isFavorite: wasFavorite }
                        : item
                )
            );
        }
    };

    const filteredItems = allItems.filter((item) => {
        const query = search.toLowerCase();

        const matchesSearch =
            (item.name ?? "")
                .toLowerCase()
                .includes(query);

        const matchesOriginCountry =
            selectedOriginCountry === "" ||
            item.beanOriginCountry === selectedOriginCountry;

        const matchesProcessing =
            selectedProcessing === "" ||
            item.processingMethod === selectedProcessing;

        const matchesVariety =
            selectedVariety === "" ||
            item.variety === selectedVariety;

        const matchesFlavorProfiles =
            selectedFlavorProfiles.length === 0 ||
            selectedFlavorProfiles.some((profile) =>
                (item.flavorProfiles ?? []).includes(profile)
            );

        return (
            matchesSearch &&
            matchesOriginCountry &&
            matchesProcessing &&
            matchesVariety &&
            matchesFlavorProfiles
        );
    });

    if (isLoading) {
        return (
            <div className="coffees-page">
                <h1>Ładowanie kaw...</h1>
            </div>
        );
    }

    if (error) {
        return (
            <div className="coffees-page">
                <h1>{error}</h1>
            </div>
        );
    }

    return (
        <div className="coffees-page">
            <div className="coffees-header">
                <h1>Kawy</h1>

                <p>
                    Poznaj odmiany kaw specialty i ich pochodzenie.
                </p>

                <div className="coffees-search-container">
                    <Search size={18} />

                    <input
                        type="text"
                        placeholder="Szukaj kaw..."
                        className="coffees-search"
                        value={search}
                        onChange={(event) =>
                            setSearch(event.target.value)
                        }
                    />
                </div>
            </div>

            <div className="coffees-content">
                <div className="coffees-filters">
                    <h3>Filtry</h3>

                    <div className="filter-group">
                        <label>
                            Kraj pochodzenia ziaren
                        </label>

                        <select
                            value={selectedOriginCountry}
                            onChange={(event) =>
                                setSelectedOriginCountry(event.target.value)
                            }
                        >
                            <option value="">
                                Wszystkie
                            </option>

                            {originCountries.map((country) => (
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
                        <label>
                            Obróbka ziaren
                        </label>

                        <select
                            value={selectedProcessing}
                            onChange={(event) =>
                                setSelectedProcessing(event.target.value)
                            }
                        >
                            <option value="">
                                Wszystkie
                            </option>

                            {processingMethods.map((method) => (
                                <option
                                    key={method}
                                    value={method}
                                >
                                    {method}
                                </option>
                            ))}
                        </select>
                    </div>

                    <div className="filter-group">
                        <label>
                            Odmiana
                        </label>

                        <select
                            value={selectedVariety}
                            onChange={(event) =>
                                setSelectedVariety(event.target.value)
                            }
                        >
                            <option value="">
                                Wszystkie
                            </option>

                            {varieties.map((variety) => (
                                <option
                                    key={variety}
                                    value={variety}
                                >
                                    {variety}
                                </option>
                            ))}
                        </select>
                    </div>

                    <div className="filter-group">
                        <label>
                            Profil smakowy
                        </label>

                        <MultiSelectInput
                            options={flavorProfileOptions}
                            value={selectedFlavorProfiles}
                            onChange={setSelectedFlavorProfiles}
                            allowCustom
                            customPlaceholder="Inny..."
                        />
                    </div>
                </div>

                <div className="coffees-grid">
                    {filteredItems.length === 0 ? (
                        <p className="coffees-empty-state">
                            {allItems.length === 0
                                ? "Brak kaw do wyświetlenia."
                                : "Brak wyników dla wybranych filtrów."}
                        </p>
                    ) : (
                        filteredItems.map((item) => (
                        <div
                            className="coffee-card"
                            key={item.key}
                            onClick={() => {
                                if (item.coffeeId != null) {
                                    navigate(`/wiki/coffees/${item.coffeeId}`);
                                    return;
                                }

                                navigate(`/wiki/articles/${item.articleId}`);
                            }}
                        >
                            <div className="coffee-image" />

                            <div className="coffee-card-content">
                                <div className="coffee-top">
                                    <h2>
                                        {item.name}
                                    </h2>

                                    {item.coffeeId != null && (
                                        <button
                                            className="favorite-button"
                                            onClick={(event) =>
                                                handleFavorite(
                                                    item.coffeeId,
                                                    event
                                                )
                                            }
                                            aria-label={
                                                item.isFavorite
                                                    ? "Usuń z ulubionych"
                                                    : "Dodaj do ulubionych"
                                            }
                                        >
                                            <Heart
                                                size={18}
                                                fill={
                                                    item.isFavorite
                                                        ? "currentColor"
                                                        : "none"
                                                }
                                            />
                                        </button>
                                    )}
                                </div>

                                <p>
                                    {item.beanOriginCountry ??
                                        "Brak kraju pochodzenia"}
                                </p>

                                <div className="coffee-card-rating">
                                    <Star
                                        size={14}
                                        fill={
                                            item.averageRating != null
                                                ? "currentColor"
                                                : "none"
                                        }
                                    />

                                    <span>
                                        {item.averageRating != null
                                            ? item.averageRating.toFixed(1)
                                            : "Brak ocen"}

                                        {" · "}

                                        {item.ratingCount ?? 0} ocen
                                    </span>
                                </div>

                                <div className="coffee-tags">
                                    {item.variety && (
                                        <span>
                                            {item.variety}
                                        </span>
                                    )}

                                    {item.processingMethod && (
                                        <span>
                                            {item.processingMethod}
                                        </span>
                                    )}

                                    {(item.flavorProfiles ?? []).map((profile) => (
                                        <span key={profile}>
                                            {profile}
                                        </span>
                                    ))}
                                </div>

                                <small>
                                    {item.isWikiArticle
                                        ? item.authorLogin
                                            ? `Autor artykułu: ${item.authorLogin}`
                                            : "Artykuł wiki"
                                        : item.roastery ?? "Brak palarni"}
                                </small>
                            </div>
                        </div>
                        ))
                    )}
                </div>
            </div>
        </div>
    );
}

export default Coffees;
