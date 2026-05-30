import { useCallback, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import { Clock3, FileText, Trash2 } from "lucide-react";

import { deleteMyArticle, getMyArticles } from "../../api/articlesApi";

import {
    canDeleteArticle,
    getArticleModuleLabel,
    getArticleStatusLabel,
} from "../../utils/articleLabels";
import { getApprovedArticlePublicPath } from "../../utils/articleRouting";

import "../../styles/wiki/MyWikiArticles.css";

function getStatusClassName(status) {
    if (status === "Pending") {
        return "pending";
    }

    if (status === "Approved") {
        return "approved";
    }

    if (status === "Rejected") {
        return "rejected";
    }

    return "";
}

function formatDate(value) {
    if (!value) {
        return "";
    }

    return new Date(value).toLocaleDateString("pl-PL");
}

function MyWikiArticles() {
    const navigate = useNavigate();

    const [articles, setArticles] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState("");
    const [deletingArticleId, setDeletingArticleId] = useState(null);

    const loadArticles = useCallback(async () => {
        try {
            setIsLoading(true);
            setError("");

            const data = await getMyArticles();
            setArticles(Array.isArray(data) ? data : []);
        } catch {
            setError("Nie udało się pobrać Twoich artykułów.");
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => {
        loadArticles();
    }, [loadArticles]);

    const handleArticleClick = (article) => {
        if (article.status === "Approved") {
            navigate(getApprovedArticlePublicPath(article));
            return;
        }

        navigate(`/wiki/my-articles/${article.id}`);
    };

    const handleDelete = async (event, article) => {
        event.stopPropagation();

        const confirmed = window.confirm(
            "Czy na pewno chcesz usunąć ten artykuł? Tej operacji nie można cofnąć."
        );

        if (!confirmed) {
            return;
        }

        try {
            setDeletingArticleId(article.id);
            setError("");

            await deleteMyArticle(article.id);
            setArticles((current) =>
                current.filter((item) => item.id !== article.id)
            );
        } catch {
            setError("Nie udało się usunąć artykułu.");
        } finally {
            setDeletingArticleId(null);
        }
    };

    if (isLoading) {
        return (
            <div className="wiki-my-articles-page">
                <h1>Ładowanie artykułów...</h1>
            </div>
        );
    }

    return (
        <div className="wiki-my-articles-page">
            <div className="wiki-my-articles-header">
                <h1>Moje artykuły</h1>

                <p>
                    Sprawdź status swoich zgłoszeń do naszej kawowej wiki —
                    w moderacji, opublikowane i odrzucone.
                </p>
            </div>

            {error && (
                <div className="wiki-my-article-comment">
                    {error}
                </div>
            )}

            {articles.length === 0 ? (
                <div className="wiki-my-articles-empty">
                    <FileText size={28} />

                    <p>
                        Nie masz jeszcze żadnych artykułów.
                    </p>
                </div>
            ) : (
                <div className="wiki-my-articles-list">
                    {articles.map((article) => (
                        <div
                            key={article.id}
                            className="wiki-my-article-card"
                            onClick={() => handleArticleClick(article)}
                        >
                            <div className="wiki-my-article-top">
                                <h2>{article.title}</h2>

                                <div className="wiki-my-article-actions">
                                    <span
                                        className={`wiki-my-article-status ${getStatusClassName(article.status)}`}
                                    >
                                        {getArticleStatusLabel(article.status)}
                                    </span>

                                    {canDeleteArticle(article.status) && (
                                        <button
                                            type="button"
                                            className="wiki-my-article-delete-button"
                                            title="Usuń artykuł"
                                            disabled={
                                                deletingArticleId === article.id
                                            }
                                            onClick={(event) =>
                                                handleDelete(event, article)
                                            }
                                        >
                                            <Trash2 size={16} />
                                        </button>
                                    )}
                                </div>
                            </div>

                            <div className="wiki-my-article-meta">
                                <span>
                                    {getArticleModuleLabel(article.module)}
                                </span>

                                <span>
                                    <Clock3 size={14} />
                                    {" "}
                                    Dodano: {formatDate(article.createdAt)}
                                </span>

                                {article.publishedAt && (
                                    <span>
                                        Opublikowano:{" "}
                                        {formatDate(article.publishedAt)}
                                    </span>
                                )}
                            </div>

                            {article.status === "Rejected" &&
                                article.moderationComment && (
                                    <div className="wiki-my-article-comment">
                                        Komentarz moderatora:{" "}
                                        {article.moderationComment}
                                    </div>
                                )}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}

export default MyWikiArticles;
