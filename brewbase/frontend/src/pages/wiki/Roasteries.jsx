import { useEffect, useMemo, useState } from "react";

import { useNavigate } from "react-router-dom";

import { Search, Flame } from "lucide-react";

import "../../styles/wiki/Roasteries.css";

import { getArticles } from "../../api/articlesApi";
import { getApprovedArticlePublicPath } from "../../utils/articleRouting";
import { parseRoasteryArticleMetadata } from "../../utils/parseCoffeeArticleMetadata";

function mapRoasteryArticle(article) {
    const metadata = parseRoasteryArticleMetadata(article.content ?? "");

    return {
        id: article.id,
        title: article.title,
        authorLogin: article.authorLogin ?? null,
        roastingStyles: metadata.roastingStyles,
        descriptionPreview: metadata.description,
    };
}

function Roasteries() {
    const navigate = useNavigate();

    const [search, setSearch] = useState("");
    const [articles, setArticles] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadArticles = async () => {
            try {
                setIsLoading(true);
                setError("");

                const data = await getArticles("roastery");
                const mappedArticles = (Array.isArray(data) ? data : []).map(
                    mapRoasteryArticle
                );

                setArticles(mappedArticles);
            } catch {
                setError("Nie udało się pobrać artykułów o palarniach.");
            } finally {
                setIsLoading(false);
            }
        };

        loadArticles();
    }, []);

    const filteredArticles = useMemo(() => {
        const query = search.trim().toLowerCase();

        if (!query) {
            return articles;
        }

        return articles.filter((article) => {
            const searchableText = [
                article.title,
                article.authorLogin,
                article.descriptionPreview,
                ...article.roastingStyles,
            ]
                .filter(Boolean)
                .join(" ")
                .toLowerCase();

            return searchableText.includes(query);
        });
    }, [articles, search]);

    if (isLoading) {
        return (
            <div className="roasteries-page">
                <h1>Ładowanie palarni...</h1>
            </div>
        );
    }

    if (error) {
        return (
            <div className="roasteries-page">
                <h1>{error}</h1>
            </div>
        );
    }

    return (
        <div className="roasteries-page">
            <div className="roasteries-header">
                <h1>Palarnie</h1>

                <p>
                    Odkrywaj palarnie specialty coffee
                    z całego świata.
                </p>

                <div className="roasteries-search-container">
                    <Search size={18} />

                    <input
                        type="text"
                        placeholder="Szukaj palarni..."
                        className="roasteries-search"
                        value={search}
                        onChange={(event) =>
                            setSearch(event.target.value)
                        }
                    />
                </div>
            </div>

            <div className="roasteries-grid">
                {filteredArticles.length === 0 ? (
                    <p className="roasteries-empty-state">
                        {articles.length === 0
                            ? "Brak zatwierdzonych artykułów o palarniach."
                            : "Brak wyników dla wybranego wyszukiwania."}
                    </p>
                ) : (
                    filteredArticles.map((article) => (
                        <div
                            key={article.id}
                            className="roastery-card"
                            onClick={() =>
                                navigate(
                                    getApprovedArticlePublicPath({
                                        id: article.id,
                                        module: "roastery",
                                        status: "Approved",
                                    })
                                )
                            }
                        >
                            <div className="roastery-card-top">
                                <div className="roastery-icon">
                                    <Flame size={22} />
                                </div>
                            </div>

                            <div className="roastery-content">
                                <h2>{article.title}</h2>

                                {article.authorLogin && (
                                    <p className="roastery-author">
                                        Autor artykułu: {article.authorLogin}
                                    </p>
                                )}

                                {article.descriptionPreview && (
                                    <p className="roastery-preview">
                                        {article.descriptionPreview}
                                    </p>
                                )}

                                {article.roastingStyles.length > 0 && (
                                    <div className="roastery-tags">
                                        {article.roastingStyles.map((style) => (
                                            <span key={style}>{style}</span>
                                        ))}
                                    </div>
                                )}
                            </div>
                        </div>
                    ))
                )}
            </div>
        </div>
    );
}

export default Roasteries;
