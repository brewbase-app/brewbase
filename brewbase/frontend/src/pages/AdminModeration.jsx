import { useEffect, useState } from "react";

import {
    CheckCircle2,
    XCircle,
    MessageSquare,
    Eye,
    Flag,
    FileText,
    Filter
} from "lucide-react";

import {
    getPendingArticles,
    getReports,
    approveArticle,
    rejectArticle
} from "../api/adminApi";

import "../styles/AdminModeration.css";

function AdminModeration() {

    // =========================
    // STATE
    // =========================

    const [activeFilter, setActiveFilter] =
        useState("all");

    const [selectedItem, setSelectedItem] =
        useState(null);

    const [moderationItems, setModerationItems] =
        useState([]);

    const [loading, setLoading] =
        useState(true);

    const [error, setError] =
        useState("");

    // =========================
    // FETCH DATA
    // =========================

    useEffect(() => {

        const fetchModeration =
            async () => {

                try {

                    const pendingArticles =
                        await getPendingArticles();

                    let reports = [];

                    try {

                        reports =
                            await getReports();

                    } catch (err) {

                        console.error(
                            "Reports error:",
                            err
                        );
                    }

                    // MAP ARTICLES

                    const mappedArticles =
                        pendingArticles.map(
                            (article) => ({

                                id:
                                article.id,

                                title:
                                article.title,

                                content:
                                article.content,

                                author:
                                    article.author ||
                                    article.authorName ||
                                    "Unknown",

                                category:
                                    article.category ||
                                    "Article",

                                createdAt:
                                    article.createdAt ||
                                    "Recently",

                                type:
                                    "approval",
                            })
                        );

                    // MAP REPORTS

                    const mappedReports =
                        reports.map(
                            (report) => ({

                                id:
                                report.id,

                                title:
                                    report.title ||
                                    "Reported content",

                                content:
                                    report.content ||
                                    report.description ||
                                    "No content",

                                author:
                                    report.author ||
                                    report.reportedUser ||
                                    "Unknown",

                                category:
                                    "Report",

                                createdAt:
                                    report.createdAt ||
                                    "Recently",

                                reportReason:
                                    report.reason ||
                                    report.reportReason ||
                                    "No reason",

                                type:
                                    "report",
                            })
                        );

                    setModerationItems([
                        ...mappedArticles,
                        ...mappedReports,
                    ]);

                } catch (err) {

                    setError(
                        err.message
                    );

                } finally {

                    setLoading(false);
                }
            };

        fetchModeration();

    }, []);

    // =========================
    // FILTERING
    // =========================

    const filteredItems =
        moderationItems.filter((item) => {

            if (activeFilter === "all") {

                return true;
            }

            return (
                item.type ===
                activeFilter
            );
        });

    // =========================
    // ACTIONS
    // =========================

    const handleApprove =
        async () => {

            try {

                await approveArticle(
                    selectedItem.id
                );

                setModerationItems(
                    moderationItems.filter(
                        (item) =>
                            item.id !==
                            selectedItem.id
                    )
                );

                setSelectedItem(
                    null
                );

            } catch (err) {

                alert(
                    err.message
                );
            }
        };

    const handleReject =
        async () => {

            try {

                await rejectArticle(
                    selectedItem.id,
                    "Rejected by admin"
                );

                setModerationItems(
                    moderationItems.filter(
                        (item) =>
                            item.id !==
                            selectedItem.id
                    )
                );

                setSelectedItem(
                    null
                );

            } catch (err) {

                alert(
                    err.message
                );
            }
        };

    // =========================
    // LOADING
    // =========================

    if (loading) {

        return (
            <p>
                Ładowanie moderacji...
            </p>
        );
    }

    // =========================
    // ERROR
    // =========================

    if (error) {

        return (
            <p>
                {error}
            </p>
        );
    }

    // =========================
    // RENDER
    // =========================

    return (

        <div className="admin-page">

            {/* HEADER */}

            <div className="admin-header">

                <h1>
                    Treści do moderacji
                </h1>

                <p>
                    Zarządzaj zgłoszeniami
                    i moderacją treści.
                </p>

            </div>

            {/* STATS */}

            <div className="admin-stats">

                {/* APPROVALS */}

                <div className="admin-stat-card">

                    <div className="admin-stat-icon">

                        <FileText size={26} />

                    </div>

                    <div>

                        <h2>

                            {
                                moderationItems.filter(
                                    (item) =>
                                        item.type ===
                                        "approval"
                                ).length
                            }

                        </h2>

                        <p>
                            Do akceptacji
                        </p>

                    </div>

                </div>

                {/* REPORTS */}

                <div className="admin-stat-card report">

                    <div className="admin-stat-icon">

                        <Flag size={26} />

                    </div>

                    <div>

                        <h2>

                            {
                                moderationItems.filter(
                                    (item) =>
                                        item.type ===
                                        "report"
                                ).length
                            }

                        </h2>

                        <p>
                            Zgłoszenia
                        </p>

                    </div>

                </div>

            </div>

            {/* CONTENT */}

            <div className="moderation-layout">

                {/* LEFT PANEL */}

                <div className="moderation-list-section">

                    {/* TOOLBAR */}

                    <div className="moderation-toolbar">

                        <div className="moderation-filters">

                            <button
                                className={
                                    activeFilter === "all"
                                        ? "active"
                                        : ""
                                }
                                onClick={() =>
                                    setActiveFilter(
                                        "all"
                                    )
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
                                    setActiveFilter(
                                        "approval"
                                    )
                                }
                            >
                                Do akceptacji
                            </button>

                            <button
                                className={
                                    activeFilter === "report"
                                        ? "active"
                                        : ""
                                }
                                onClick={() =>
                                    setActiveFilter(
                                        "report"
                                    )
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

                    {/* LIST */}

                    <div className="moderation-list">

                        {filteredItems.map(
                            (item) => (

                                <div
                                    key={item.id}
                                    className={`moderation-item ${
                                        selectedItem?.id ===
                                        item.id
                                            ? "selected"
                                            : ""
                                    }`}
                                    onClick={() =>
                                        setSelectedItem(
                                            item
                                        )
                                    }
                                >

                                    <div className="moderation-type">

                                        {item.type ===
                                        "approval"
                                            ? (
                                                <FileText
                                                    size={18}
                                                />
                                            ) : (
                                                <Flag
                                                    size={18}
                                                />
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

                            )
                        )}

                    </div>

                </div>

                {/* RIGHT PANEL */}

                <div className="moderation-preview">

                    {selectedItem ? (

                        <>

                            {/* HEADER */}

                            <div className="preview-header">

                                <h2>
                                    {selectedItem.title}
                                </h2>

                                <span>
                                    {
                                        selectedItem.category
                                    }
                                </span>

                            </div>

                            {/* AUTHOR */}

                            <div className="preview-section">

                                <label>
                                    Autor
                                </label>

                                <p>
                                    {
                                        selectedItem.author
                                    }
                                </p>

                            </div>

                            {/* DATE */}

                            <div className="preview-section">

                                <label>
                                    Data
                                </label>

                                <p>
                                    {
                                        selectedItem.createdAt
                                    }
                                </p>

                            </div>

                            {/* REPORT REASON */}

                            {selectedItem.type ===
                                "report" && (

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

                            {/* CONTENT */}

                            <div className="preview-section">

                                <label>
                                    Treść
                                </label>

                                <div className="preview-box">

                                    {
                                        selectedItem.content
                                    }

                                </div>

                            </div>

                            {/* ACTIONS */}

                            <div className="moderation-actions">

                                <button
                                    className="accept-button"
                                    onClick={
                                        handleApprove
                                    }
                                >

                                    <CheckCircle2
                                        size={18}
                                    />

                                    Akceptuj

                                </button>

                                <button
                                    className="reject-button"
                                    onClick={
                                        handleReject
                                    }
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

                                    Komentarz

                                </button>

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

            </div>f

        </div>

    );
}

export default AdminModeration;