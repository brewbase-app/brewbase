import { useEffect, useState } from "react";

import { useNavigate } from "react-router-dom";

import { getArticles } from "../api/articlesApi";

import "../styles/wiki/Coffees.css";

function WikiArticlesSection({ module, gridClassName, cardClassName }) {
    const navigate = useNavigate();
    const [articles, setArticles] = useState([]);

    useEffect(() => {
        const loadArticles = async () => {
            try {
                const data = await getArticles(module);
                setArticles(Array.isArray(data) ? data : []);
            } catch {
                setArticles([]);
            }
        };

        loadArticles();
    }, [module]);

    if (articles.length === 0) {
        return null;
    }

    return (
        <section className="wiki-articles-section">
            <h3>Artykuły wiki</h3>

            <div className={gridClassName}>
                {articles.map((article) => (
                    <div
                        key={article.id}
                        className={cardClassName}
                        onClick={() =>
                            navigate(`/wiki/articles/${article.id}`)
                        }
                    >
                        <div className="coffee-card-content">
                            <h2>{article.title}</h2>

                            {article.authorLogin && (
                                <small>
                                    Autor artykułu: {article.authorLogin}
                                </small>
                            )}
                        </div>
                    </div>
                ))}
            </div>
        </section>
    );
}

export default WikiArticlesSection;
