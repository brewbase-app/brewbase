import { useState } from "react";

import {
    CheckCircle2,
    XCircle,
    MessageSquare,
    Eye,
    Flag,
    FileText,
    Filter
} from "lucide-react";

import "../styles/AdminModeration.css";

function AdminModeration() {

    const [activeFilter, setActiveFilter] =
        useState("all");

    const [selectedItem, setSelectedItem] =
        useState(null);

    const moderationItems = [

        {
            id: 1,
            type: "approval",
            category: "Kawy",
            title: "Geisha z Panamy — charakterystyka",
            author: "CoffeeLover",
            createdAt: "2 godz. temu",
            content:
                "Geisha z Panamy to jedna z najbardziej cenionych kaw specialty...",
        },

        {
            id: 2,
            type: "report",
            category: "Receptury",
            title: "V60 — najlepszy przepis",
            author: "User123",
            createdAt: "3 godz. temu",
            reportReason: "Wulgaryzmy",
            content: "gowno gowno",
        },

        {
            id: 3,
            type: "approval",
            category: "Regiony",
            title: "Yirgacheffe — region kultowy",
            author: "BeanExplorer",
            createdAt: "5 godz. temu",
            content:
                "Yirgacheffe słynie z herbacianego body i wysokiej słodyczy...",
        },

        {
            id: 4,
            type: "report",
            category: "Cuppingi",
            title: "Etiopia Gedeb",
            author: "CoffeeFan",
            createdAt: "1 dzień temu",
            reportReason: "Spam",
            content:
                "Smak okropny, nie polecam!!!",
        }
    ];

    const filteredItems =
        moderationItems.filter((item) => {

            if (activeFilter === "all")
                return true;

            return item.type === activeFilter;
        });

    return (

        <div className="admin-page">

            {/* HEADER */}

            <div className="admin-header">

                <h1>
                    Treści do moderacji
                </h1>

                <p>
                    Przeglądaj i moderuj treści
                    tworzone przez użytkowników.
                </p>

            </div>

            {/* STATS */}

            <div className="admin-stats">

                <div className="admin-stat-card">

                    <div className="admin-stat-icon">

                        <FileText size={26} />

                    </div>

                    <div>

                        <h2>
                            23
                        </h2>

                        <p>
                            Treści do akceptacji
                        </p>

                    </div>

                </div>

                <div className="admin-stat-card report">

                    <div className="admin-stat-icon">

                        <Flag size={26} />

                    </div>

                    <div>

                        <h2>
                            45
                        </h2>

                        <p>
                            Zgłoszenia
                        </p>

                    </div>

                </div>

            </div>

            {/* CONTENT */}

            <div className="moderation-layout">

                {/* LEFT */}

                <div className="moderation-list-section">

                    {/* FILTERS */}

                    <div className="moderation-toolbar">

                        <div className="moderation-filters">

                            <button
                                className={
                                    activeFilter === "all"
                                        ? "active"
                                        : ""
                                }
                                onClick={() =>
                                    setActiveFilter("all")
                                }
                            >
                                Wszystkie
                            </button>

                            <button
                                className={
                                    activeFilter === "approval"
                                        ? "active"
                                        : ""
                                }
                                onClick={() =>
                                    setActiveFilter("approval")
                                }
                            >
                                Treści do akceptacji
                            </button>

                            <button
                                className={
                                    activeFilter === "report"
                                        ? "active"
                                        : ""
                                }
                                onClick={() =>
                                    setActiveFilter("report")
                                }
                            >
                                Zgłoszenia
                            </button>

                        </div>

                        <button className="filter-button">

                            <Filter size={16} />

                            Filtry

                        </button>

                    </div>

                    {/* TABLE */}

                    <div className="moderation-list">

                        {filteredItems.map((item) => (

                            <div
                                key={item.id}
                                className={`moderation-item ${
                                    selectedItem?.id === item.id
                                        ? "selected"
                                        : ""
                                }`}
                                onClick={() =>
                                    setSelectedItem(item)
                                }
                            >

                                <div className="moderation-type">

                                    {item.type === "approval"
                                        ? (
                                            <FileText size={18} />
                                        ) : (
                                            <Flag size={18} />
                                        )}

                                </div>

                                <div className="moderation-content">

                                    <h3>
                                        {item.title}
                                    </h3>

                                    <p>
                                        {item.content}
                                    </p>

                                </div>

                                <div className="moderation-meta">

                                    <span>
                                        {item.category}
                                    </span>

                                    <small>
                                        {item.author}
                                    </small>

                                </div>

                                <div className="moderation-date">

                                    {item.createdAt}

                                </div>

                                <button
                                    className="preview-button"
                                >

                                    <Eye size={18} />

                                </button>

                            </div>

                        ))}

                    </div>

                </div>

                {/* RIGHT PREVIEW */}

                <div className="moderation-preview">

                    {selectedItem ? (

                        <>

                            <div className="preview-header">

                                <h2>
                                    {selectedItem.title}
                                </h2>

                                <span>
                                    {selectedItem.category}
                                </span>

                            </div>

                            <div className="preview-section">

                                <label>
                                    Autor
                                </label>

                                <p>
                                    {selectedItem.author}
                                </p>

                            </div>

                            <div className="preview-section">

                                <label>
                                    Dodano
                                </label>

                                <p>
                                    {selectedItem.createdAt}
                                </p>

                            </div>

                            {selectedItem.type === "report" && (

                                <div className="preview-section">

                                    <label>
                                        Powód zgłoszenia
                                    </label>

                                    <p>
                                        {
                                            selectedItem.reportReason
                                        }
                                    </p>

                                </div>

                            )}

                            <div className="preview-section">

                                <label>
                                    Podgląd treści
                                </label>

                                <div className="preview-box">

                                    {selectedItem.content}

                                </div>

                            </div>

                            {/* ACTIONS */}

                            <div className="moderation-actions">

                                {selectedItem.type ===
                                "approval" ? (

                                    <>

                                        <button
                                            className="accept-button"
                                        >

                                            <CheckCircle2
                                                size={18}
                                            />

                                            Akceptuj

                                        </button>

                                        <button
                                            className="reject-button"
                                        >

                                            <XCircle
                                                size={18}
                                            />

                                            Odrzuć

                                        </button>

                                        <button
                                            className="comment-button"
                                        >

                                            <MessageSquare
                                                size={18}
                                            />

                                            Odrzuć z komentarzem

                                        </button>

                                    </>

                                ) : (

                                    <>

                                        <button
                                            className="accept-button"
                                        >

                                            Brak naruszeń

                                        </button>

                                        <button
                                            className="reject-button"
                                        >

                                            Usuń treść

                                        </button>

                                        <button
                                            className="comment-button"
                                        >

                                            Usuń i ostrzeż

                                        </button>

                                    </>

                                )}

                            </div>

                        </>

                    ) : (

                        <div className="empty-preview">

                            <p>
                                Wybierz treść do podglądu
                            </p>

                        </div>

                    )}

                </div>

            </div>

        </div>

    );
}

export default AdminModeration;