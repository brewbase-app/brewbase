import { useEffect, useState } from "react";

import { useNavigate } from "react-router-dom";

import "../../styles/wiki/BrewingMethods.css";

import { getArticles } from "../../api/articlesApi";
import { getBrewingMethods } from "../../api/brewingMethodApi";

function getArticleExcerpt(content, maxLength = 140) {
    if (!content) {
        return "";
    }

    const firstParagraph = content
        .split("\n\n")[0]
        .replace(/\n/g, " ")
        .trim();

    if (firstParagraph.length <= maxLength) {
        return firstParagraph;
    }

    return `${firstParagraph.slice(0, maxLength).trim()}...`;
}

function mapBrewingArticle(article) {
    return {
        id: article.id,
        title: article.title,
        excerpt: getArticleExcerpt(article.content ?? ""),
    };
}

function BrewingMethods() {
    const navigate = useNavigate();

    const [articles, setArticles] = useState([]);
    const [catalogMethodNames, setCatalogMethodNames] = useState([]);
    const [selectedMethod, setSelectedMethod] = useState("");
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadData = async () => {
            try {
                setIsLoading(true);
                setError("");

                const [articlesData, methodsData] = await Promise.all([
                    getArticles("brewing_method"),
                    getBrewingMethods(),
                ]);

                setArticles(
                    (Array.isArray(articlesData) ? articlesData : []).map(
                        mapBrewingArticle
                    )
                );

                const names = (Array.isArray(methodsData) ? methodsData : [])
                    .map((method) => method.name)
                    .filter(Boolean);

                setCatalogMethodNames(
                    [...new Set(names)].sort((left, right) =>
                        left.localeCompare(right, "pl")
                    )
                );
            } catch {
                setError("Nie udało się pobrać metod parzenia.");
            } finally {
                setIsLoading(false);
            }
        };

        loadData();
    }, []);

    const filteredArticles = articles.filter((article) => {
        const matchesMethod =
            selectedMethod === "" ||
            article.title === selectedMethod;

        return matchesMethod;
    });

    if (isLoading) {
        return (
            <div className="methods-page">
                <h1>Ładowanie metod parzenia...</h1>
            </div>
        );
    }

    if (error) {
        return (
            <div className="methods-page">
                <h1>{error}</h1>
            </div>
        );
    }

    return (
        <div className="methods-page">
            <div className="methods-header">
                <h1>Metody parzenia</h1>

                <p>
                    Poznaj najpopularniejsze metody
                    parzenia kaw specialty.
                </p>
            </div>

            <div className="methods-content">
                <div className="methods-filters">
                    <h3>Filtry</h3>

                    <div className="filter-group">
                        <label>Metoda parzenia</label>

                        <select
                            value={selectedMethod}
                            onChange={(event) =>
                                setSelectedMethod(event.target.value)
                            }
                        >
                            <option value="">
                                Wszystkie
                            </option>

                            {catalogMethodNames.map((method) => (
                                <option
                                    key={method}
                                    value={method}
                                >
                                    {method}
                                </option>
                            ))}
                        </select>
                    </div>
                </div>

                {filteredArticles.length === 0 ? (
                    <p className="methods-empty-state">
                        {articles.length === 0
                            ? "Brak opublikowanych artykułów o metodach parzenia."
                            : "Brak wyników dla wybranych filtrów."}
                    </p>
                ) : (
                    <div className="methods-grid">
                        {filteredArticles.map((article) => (
                            <div
                                className="method-card"
                                key={article.id}
                                onClick={() =>
                                    navigate(
                                        `/wiki/articles/${article.id}`
                                    )
                                }
                            >
                                <div className="method-image" />

                                <div className="method-card-content">
                                    <div className="method-top">
                                        <h2>
                                            {article.title}
                                        </h2>
                                    </div>

                                    {article.excerpt && (
                                        <p>
                                            {article.excerpt}
                                        </p>
                                    )}
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
}

export default BrewingMethods;
