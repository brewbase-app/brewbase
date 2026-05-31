import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";

import { getMyArticleById } from "../../api/articlesApi";

import {
    getArticleModuleLabel,
    getArticleStatusLabel,
} from "../../utils/articleLabels";

import "../../styles/wiki/CoffeeDetails.css";
import "../../styles/wiki/MyWikiArticles.css";

function MyWikiArticleDetails() {
    const { id } = useParams();

    const [article, setArticle] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadArticle = async () => {
            try {
                setIsLoading(true);
                setError("");

                const data = await getMyArticleById(id);
                setArticle(data);
            } catch {
                setError("Nie udało się pobrać artykułu.");
            } finally {
                setIsLoading(false);
            }
        };

        loadArticle();
    }, [id]);

    if (isLoading) {
        return <h1>Ładowanie...</h1>;
    }

    if (error) {
        return <h1>{error}</h1>;
    }

    if (!article) {
        return <h1>Nie znaleziono artykułu.</h1>;
    }

    return (
        <div className="article-page">
            <div className="article-hero">
                <div className="article-overlay">
                    <span>
                        {getArticleModuleLabel(article.module)}
                        {" · "}
                        {getArticleStatusLabel(article.status)}
                    </span>

                    <h1>{article.title}</h1>
                </div>
            </div>

            <div className="article-content">
                {(article.status === "Draft" ||
                    article.status === "Rejected") &&
                    article.moderationComment && (
                        <section>
                            <div className="wiki-my-article-comment">
                                Komentarz moderatora:{" "}
                                {article.moderationComment}
                            </div>
                        </section>
                    )}

                <section>
                    <p style={{ whiteSpace: "pre-wrap" }}>
                        {article.content}
                    </p>
                </section>
            </div>
        </div>
    );
}

export default MyWikiArticleDetails;
