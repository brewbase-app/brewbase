import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Flag } from "lucide-react";

import { getArticleById } from "../../api/articlesApi";
import { shouldUseCoffeeDetailRoute } from "../../utils/articleRouting";
import { parseRoasteryArticleMetadata } from "../../utils/parseCoffeeArticleMetadata";

import "../../styles/wiki/CoffeeDetails.css";

function WikiArticleDetails() {
    const { id } = useParams();
    const navigate = useNavigate();

    const [article, setArticle] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadArticle = async () => {
            try {
                setIsLoading(true);
                setError("");

                const data = await getArticleById(id);
                setArticle(data);
            } catch {
                setError("Nie udało się pobrać artykułu.");
            } finally {
                setIsLoading(false);
            }
        };

        loadArticle();
    }, [id]);

    useEffect(() => {
        if (article && shouldUseCoffeeDetailRoute(article)) {
            navigate(`/wiki/coffees/${article.coffeeId}`, { replace: true });
        }
    }, [article, navigate]);

    if (isLoading) {
        return <h1>Ładowanie...</h1>;
    }

    if (error) {
        return <h1>{error}</h1>;
    }

    if (!article) {
        return <h1>Nie znaleziono artykułu.</h1>;
    }

    const roasteryMetadata =
        article.module === "roastery"
            ? parseRoasteryArticleMetadata(article.content ?? "")
            : null;

    return (
        <div className="article-page">
            <div className="article-hero">
                <div className="article-overlay">
                    <span>
                        Autor artykułu: {article.authorLogin}
                    </span>

                    <h1>{article.title}</h1>

                    {roasteryMetadata?.roastingStyles.length > 0 && (
                        <div className="article-roastery-styles">
                            {roasteryMetadata.roastingStyles.map((style) => (
                                <span key={style}>{style}</span>
                            ))}
                        </div>
                    )}
                </div>
            </div>

            <div className="article-content">
                <section>
                    {roasteryMetadata ? (
                        <p style={{ whiteSpace: "pre-wrap" }}>
                            {roasteryMetadata.description ||
                                "Brak opisu palarni."}
                        </p>
                    ) : (
                        <p style={{ whiteSpace: "pre-wrap" }}>
                            {article.content}
                        </p>
                    )}
                </section>

                <section>
                    <button
                        type="button"
                        style={reportButtonStyle}
                        onClick={() =>
                            navigate("/report", {
                                state: {
                                    contentType: "article",
                                    contentId: article.id,
                                    contentTitle: article.title,
                                    returnPath: `/wiki/articles/${article.id}`,
                                },
                            })
                        }
                    >
                        <Flag size={16} />
                        Zgłoś treść
                    </button>
                </section>
            </div>
        </div>
    );
}

const reportButtonStyle = {
    display: "inline-flex",
    alignItems: "center",
    gap: "8px",
    padding: "10px 16px",
    border: "1px solid #d4d4d4",
    borderRadius: "16px",
    background: "transparent",
    color: "#6b6b6b",
    fontSize: "14px",
    fontWeight: "600",
    cursor: "pointer",
};

export default WikiArticleDetails;
