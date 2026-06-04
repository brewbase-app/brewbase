import { useEffect, useState } from "react";

import { useNavigate } from "react-router-dom";

import { Flame } from "lucide-react";

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
            </div>

            <div className="roasteries-grid">
                {articles.length === 0 ? (
                    <p className="roasteries-empty-state">
                        Brak zatwierdzonych artykułów o palarniach.
                    </p>
                ) : (
                    articles.map((article) => (
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
