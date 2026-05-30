import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";

import { getArticleById } from "../../api/articlesApi";
import { shouldUseCoffeeDetailRoute } from "../../utils/articleRouting";

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

    return (
        <div className="article-page">
            <div className="article-hero">
                <div className="article-overlay">
                    <span>
                        Autor artykułu: {article.authorLogin}
                    </span>

                    <h1>{article.title}</h1>
                </div>
            </div>

            <div className="article-content">
                <section>
                    <p style={{ whiteSpace: "pre-wrap" }}>{article.content}</p>
                </section>
            </div>
        </div>
    );
}

export default WikiArticleDetails;
