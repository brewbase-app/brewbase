import { useNavigate } from "react-router-dom";
import {
    Heart,
    Plus,
    BookOpen,
    ChevronRight
} from "lucide-react";

const Recipes = () => {
    const navigate = useNavigate();

    const cards = [
        {
            title: "Wszystkie receptury",
            description: "Przeglądaj wszystkie dostępne receptury.",
            icon: <BookOpen size={22} />,
            path: "/recipes/all"
        },
        {
            title: "Nowa receptura",
            description: "Stwórz własny przepis parzenia kawy.",
            icon: <Plus size={22} />,
            path: "/recipes/new"
        },
        {
            title: "Polubione receptury",
            description: "Zapisane i najczęściej używane przepisy.",
            icon: <Heart size={22} />,
            path: "/recipes/favorites"
        },
        {
            title: "Twoje receptury",
            description: "Wszystkie stworzone przez Ciebie receptury.",
            icon: <BookOpen size={22} />,
            path: "/recipes/my"
        }
    ];

    return (
        <div
            style={{
                width: "100%",
                minHeight: "100vh",
                backgroundColor: "#f3f3f3",
                padding: "55px 60px",
                boxSizing: "border-box"
            }}
        >
            {/* HEADER */}
            <div style={{ marginBottom: "38px" }}>
                <h1
                    style={{
                        fontSize: "58px",
                        fontWeight: "700",
                        color: "#1f1f1f",
                        marginBottom: "8px",
                        lineHeight: "1"
                    }}
                >
                    Receptury
                </h1>

                <p
                    style={{
                        fontSize: "16px",
                        color: "#6f6f6f"
                    }}
                >
                    Twórz, zapisuj i organizuj swoje przepisy parzenia.
                </p>
            </div>

            {/* CARDS */}
            <div
                style={{
                    display: "flex",
                    flexDirection: "column",
                    gap: "20px",
                    maxWidth: "960px"
                }}
            >
                {cards.map((card) => (
                    <div
                        key={card.title}
                        onClick={() => navigate(card.path)}
                        style={{
                            backgroundColor: "#fafafa",
                            borderRadius: "26px",
                            padding: "24px 28px",
                            cursor: "pointer",
                            border: "1px solid #e6e6e6",
                            display: "flex",
                            alignItems: "center",
                            justifyContent: "space-between",
                            transition: "0.2s ease",
                            boxShadow:
                                "0 2px 10px rgba(0,0,0,0.03)"
                        }}
                        onMouseEnter={(e) => {
                            e.currentTarget.style.transform =
                                "translateY(-2px)";
                            e.currentTarget.style.boxShadow =
                                "0 6px 18px rgba(0,0,0,0.05)";
                        }}
                        onMouseLeave={(e) => {
                            e.currentTarget.style.transform =
                                "translateY(0)";
                            e.currentTarget.style.boxShadow =
                                "0 2px 10px rgba(0,0,0,0.03)";
                        }}
                    >
                        <div
                            style={{
                                display: "flex",
                                alignItems: "center",
                                gap: "22px"
                            }}
                        >
                            <div
                                style={{
                                    width: "58px",
                                    height: "58px",
                                    borderRadius: "18px",
                                    backgroundColor: "#efefef",
                                    display: "flex",
                                    alignItems: "center",
                                    justifyContent: "center",
                                    color: "#2a2a2a",
                                    flexShrink: 0
                                }}
                            >
                                {card.icon}
                            </div>

                            <div>
                                <h2
                                    style={{
                                        fontSize: "22px",
                                        fontWeight: "700",
                                        color: "#1f1f1f",
                                        marginBottom: "6px"
                                    }}
                                >
                                    {card.title}
                                </h2>

                                <p
                                    style={{
                                        fontSize: "15px",
                                        color: "#707070",
                                        lineHeight: "1.4"
                                    }}
                                >
                                    {card.description}
                                </p>
                            </div>
                        </div>

                        <ChevronRight
                            size={26}
                            color="#666"
                        />
                    </div>
                ))}
            </div>
        </div>
    );
};

export default Recipes;