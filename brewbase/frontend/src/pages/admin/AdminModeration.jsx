import { useEffect, useState } from "react";

import {
    CheckCircle2,
    XCircle,
    Eye,
    Flag,
    FileText,
} from "lucide-react";

import {
    getPendingArticles,
    getReports,
    approveArticle,
    rejectArticle,
    dismissReport,
    upholdReport,
} from "../../api/adminApi";

import ConfirmDialog from "../../components/ConfirmDialog";

import "../../styles/admin/AdminModeration.css";

const REPORT_CONTENT_TYPE_LABELS = {
    recipe: "Receptura",
    coffee: "Kawa",
    article: "Artykuł wiki",
};

const REPORT_STATUS_LABELS = {
    open: "Otwarte",
    dismissed: "Odrzucone",
    upheld: "Zatwierdzone",
};

function formatModerationDate(value) {
    if (!value) {
        return "—";
    }

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
        return String(value);
    }

    return date.toLocaleString("pl-PL", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
    });
}

function getContentTypeLabel(contentType) {
    return REPORT_CONTENT_TYPE_LABELS[contentType] ?? "Treść";
}

function getReportStatusLabel(status) {
    return REPORT_STATUS_LABELS[status] ?? status ?? "Otwarte";
}

function mapReport(report) {
    return {
        id: report.reportId ?? report.id,
        contentType: report.contentType ?? "article",
        contentId: report.contentId ?? report.articleId,
        contentTitle:
            report.contentTitle ??
            report.articleTitle ??
            "Zgłoszona treść",
        category: report.category ?? "Inny problem",
        comment: report.comment ?? null,
        reportedBy: report.reportedBy ?? report.author ?? "Nieznany",
        status: report.status ?? "open",
        resolvedAt: report.resolvedAt ?? null,
        resolvedByLogin: report.resolvedByLogin ?? null,
        resolutionAction: report.resolutionAction ?? null,
        createdAt: report.createdAt,
        type: "report",
    };
}

function mapApproval(article) {
    return {
        id: article.id,
        title: article.title,
        content: article.content,
        author: article.author || article.authorName || article.authorLogin || "Unknown",
        category: article.category || "Article",
        createdAt: article.createdAt || "Recently",
        type: "approval",
    };
}

function AdminModeration() {
    const [activeFilter, setActiveFilter] = useState("all");
    const [selectedItem, setSelectedItem] = useState(null);
    const [pendingArticles, setPendingArticles] = useState([]);
    const [openReports, setOpenReports] = useState([]);
    const [historyReports, setHistoryReports] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    const [rejectComment, setRejectComment] = useState("");
    const [upholdComment, setUpholdComment] = useState("");
    const [showUpholdConfirm, setShowUpholdConfirm] = useState(false);
    const [isUpholding, setIsUpholding] = useState(false);
    const [showRejectForm, setShowRejectForm] = useState(false);
    const [actionError, setActionError] = useState("");

    useEffect(() => {
        const fetchModeration = async () => {
            try {
                const [pending, open, history] = await Promise.all([
                    getPendingArticles(),
                    getReports("open"),
                    getReports("history"),
                ]);

                setPendingArticles(pending.map(mapApproval));
                setOpenReports(open.map(mapReport));
                setHistoryReports(history.map(mapReport));
            } catch (err) {
                setError(err.message);
            } finally {
                setLoading(false);
            }
        };

        fetchModeration();
    }, []);

    useEffect(() => {
        setUpholdComment("");
        setShowUpholdConfirm(false);
        setIsUpholding(false);
        setActionError("");
    }, [selectedItem?.id, selectedItem?.type]);

    const moderationItems = [
        ...pendingArticles,
        ...openReports,
    ];

    const filteredItems = (() => {
        if (activeFilter === "approval") {
            return pendingArticles;
        }

        if (activeFilter === "report") {
            return openReports;
        }

        if (activeFilter === "report-history") {
            return historyReports;
        }

        return moderationItems;
    })();

    const removeSelectedFromLists = () => {
        if (!selectedItem) {
            return;
        }

        if (selectedItem.type === "approval") {
            setPendingArticles((items) =>
                items.filter((item) => item.id !== selectedItem.id)
            );
        } else if (selectedItem.status === "open") {
            setOpenReports((items) =>
                items.filter((item) => item.id !== selectedItem.id)
            );
        } else {
            setHistoryReports((items) =>
                items.filter((item) => item.id !== selectedItem.id)
            );
        }

        setSelectedItem(null);
    };

    const refreshReports = async () => {
        const [open, history] = await Promise.all([
            getReports("open"),
            getReports("history"),
        ]);

        setOpenReports(open.map(mapReport));
        setHistoryReports(history.map(mapReport));
    };

    const handleDismissReport = async () => {
        if (selectedItem?.type !== "report" || selectedItem.status !== "open") {
            return;
        }

        setActionError("");

        try {
            await dismissReport(selectedItem.id);
            await refreshReports();
            setSelectedItem(null);
        } catch (err) {
            setActionError(err.message);
        }
    };

    const handleOpenUpholdConfirm = () => {
        if (selectedItem?.type !== "report" || selectedItem.status !== "open") {
            return;
        }

        const comment = upholdComment.trim();

        if (!comment) {
            setActionError("Komentarz moderacji jest wymagany przy usuwaniu treści.");
            return;
        }

        setActionError("");
        setShowUpholdConfirm(true);
    };

    const handleCloseUpholdConfirm = () => {
        if (isUpholding) {
            return;
        }

        setShowUpholdConfirm(false);
    };

    const handleConfirmUpholdReport = async () => {
        if (selectedItem?.type !== "report" || selectedItem.status !== "open") {
            return;
        }

        const comment = upholdComment.trim();

        if (!comment) {
            setActionError("Komentarz moderacji jest wymagany przy usuwaniu treści.");
            setShowUpholdConfirm(false);
            return;
        }

        setActionError("");
        setIsUpholding(true);

        try {
            await upholdReport(selectedItem.id, comment);
            await refreshReports();
            setSelectedItem(null);
            setUpholdComment("");
            setShowUpholdConfirm(false);
        } catch (err) {
            setActionError(err.message);
        } finally {
            setIsUpholding(false);
        }
    };

    const handleApprove = async () => {
        if (selectedItem?.type !== "approval") {
            return;
        }

        setActionError("");

        try {
            await approveArticle(selectedItem.id);
            removeSelectedFromLists();
            setShowRejectForm(false);
            setRejectComment("");
        } catch (err) {
            setActionError(err.message);
        }
    };

    const handleReject = async () => {
        if (selectedItem?.type !== "approval") {
            return;
        }

        const comment = rejectComment.trim();

        if (!comment) {
            setActionError("Komentarz moderacji jest wymagany.");
            return;
        }

        setActionError("");

        try {
            await rejectArticle(selectedItem.id, comment);
            removeSelectedFromLists();
            setShowRejectForm(false);
            setRejectComment("");
        } catch (err) {
            setActionError(err.message);
        }
    };

    const handleSelectItem = (item) => {
        setSelectedItem(item);
        setShowRejectForm(false);
        setRejectComment("");
        setActionError("");
    };

    if (loading) {
        return <p>Ładowanie moderacji...</p>;
    }

    if (error) {
        return <p>{error}</p>;
    }

    const isHistoryReport =
        selectedItem?.type === "report" && selectedItem.status !== "open";

    return (
        <div className="admin-page">
            <div className="admin-header">
                <h1>Treści do moderacji</h1>
                <p>Zarządzaj zgłoszeniami i moderacją treści.</p>
            </div>

            <div className="admin-stats">
                <div className="admin-stat-card">
                    <div className="admin-stat-icon">
                        <FileText size={26} />
                    </div>
                    <div>
                        <h2>{pendingArticles.length}</h2>
                        <p>Do akceptacji</p>
                    </div>
                </div>

                <div className="admin-stat-card report">
                    <div className="admin-stat-icon">
                        <Flag size={26} />
                    </div>
                    <div>
                        <h2>{openReports.length}</h2>
                        <p>Otwarte zgłoszenia</p>
                    </div>
                </div>
            </div>

            <div className="moderation-layout">
                <div className="moderation-list-section">
                    <div className="moderation-toolbar">
                        <div className="moderation-filters">
                            <button
                                type="button"
                                className={activeFilter === "all" ? "active" : ""}
                                onClick={() => setActiveFilter("all")}
                            >
                                Wszystkie
                            </button>
                            <button
                                type="button"
                                className={
                                    activeFilter === "approval" ? "active" : ""
                                }
                                onClick={() => setActiveFilter("approval")}
                            >
                                Do akceptacji
                            </button>
                            <button
                                type="button"
                                className={activeFilter === "report" ? "active" : ""}
                                onClick={() => setActiveFilter("report")}
                            >
                                Zgłoszenia
                            </button>
                            <button
                                type="button"
                                className={
                                    activeFilter === "report-history"
                                        ? "active"
                                        : ""
                                }
                                onClick={() => setActiveFilter("report-history")}
                            >
                                Historia zgłoszeń
                            </button>
                        </div>
                    </div>

                    <div className="moderation-list">
                        {filteredItems.length === 0 ? (
                            <div className="admin-empty-list">
                                Brak pozycji w tej sekcji.
                            </div>
                        ) : (
                            filteredItems.map((item) => {
                                const isReport = item.type === "report";
                                const listTitle = isReport
                                    ? item.contentTitle
                                    : item.title;
                                const listSubtitle = isReport
                                    ? `${item.category} · ${item.reportedBy}`
                                    : item.content;
                                const listMetaLabel = isReport
                                    ? getContentTypeLabel(item.contentType)
                                    : item.category;
                                const listMetaAuthor = isReport
                                    ? getReportStatusLabel(item.status)
                                    : item.author;

                                return (
                                    <div
                                        key={`${item.type}-${item.id}-${item.status ?? "open"}`}
                                        className={`moderation-item ${
                                            selectedItem?.type === item.type &&
                                            selectedItem?.id === item.id &&
                                            selectedItem?.status === item.status
                                                ? "selected"
                                                : ""
                                        }`}
                                        onClick={() => handleSelectItem(item)}
                                    >
                                        <div className="moderation-type">
                                            {isReport ? (
                                                <Flag size={18} />
                                            ) : (
                                                <FileText size={18} />
                                            )}
                                        </div>

                                        <div className="moderation-content">
                                            <h3>{listTitle}</h3>
                                            <p>{listSubtitle}</p>
                                        </div>

                                        <div className="moderation-meta">
                                            <span>{listMetaLabel}</span>
                                            <small>{listMetaAuthor}</small>
                                        </div>

                                        <div className="moderation-date">
                                            {formatModerationDate(item.createdAt)}
                                        </div>

                                        <button type="button" className="preview-button">
                                            <Eye size={18} />
                                        </button>
                                    </div>
                                );
                            })
                        )}
                    </div>
                </div>

                <div className="moderation-preview">
                    {selectedItem ? (
                        selectedItem.type === "report" ? (
                            <>
                                <div className="preview-header">
                                    <p className="admin-report-label">Zgłoszenie</p>
                                    <h2>Szczegóły zgłoszenia</h2>
                                    <span>
                                        {getContentTypeLabel(selectedItem.contentType)}
                                    </span>
                                </div>

                                <div className="admin-report-target-summary">
                                    <span className="admin-report-target-type">
                                        {getContentTypeLabel(selectedItem.contentType)}
                                    </span>
                                    <span className="admin-report-target-title">
                                        {selectedItem.contentTitle}
                                    </span>
                                    <span className="admin-report-target-id">
                                        ID: {selectedItem.contentId}
                                    </span>
                                </div>

                                <div className="preview-section">
                                    <label>Status zgłoszenia</label>
                                    <div className="admin-report-field-value">
                                        {getReportStatusLabel(selectedItem.status)}
                                    </div>
                                </div>

                                <div className="preview-section">
                                    <label>Kategoria zgłoszenia</label>
                                    <div className="admin-report-field-value">
                                        {selectedItem.category}
                                    </div>
                                </div>

                                <div className="preview-section">
                                    <label>Komentarz (opcjonalny)</label>
                                    <div
                                        className={`admin-report-field-value admin-report-field-value--comment ${
                                            selectedItem.comment?.trim()
                                                ? ""
                                                : "admin-report-field-value--empty"
                                        }`}
                                    >
                                        {selectedItem.comment?.trim() ||
                                            "Brak komentarza"}
                                    </div>
                                </div>

                                <div className="preview-section">
                                    <label>Zgłoszono przez</label>
                                    <p>{selectedItem.reportedBy}</p>
                                </div>

                                <div className="preview-section">
                                    <label>Data zgłoszenia</label>
                                    <p>
                                        {formatModerationDate(selectedItem.createdAt)}
                                    </p>
                                </div>

                                {isHistoryReport && (
                                    <>
                                        <div className="preview-section">
                                            <label>Rozpatrzone przez</label>
                                            <p>
                                                {selectedItem.resolvedByLogin ||
                                                    "—"}
                                            </p>
                                        </div>
                                        <div className="preview-section">
                                            <label>Data rozpatrzenia</label>
                                            <p>
                                                {formatModerationDate(
                                                    selectedItem.resolvedAt
                                                )}
                                            </p>
                                        </div>
                                    </>
                                )}

                                {actionError && (
                                    <p className="admin-action-error">{actionError}</p>
                                )}

                                {!isHistoryReport && (
                                    <div className="preview-section">
                                        <label>Komentarz moderacji</label>
                                        <textarea
                                            className="admin-reject-textarea"
                                            value={upholdComment}
                                            placeholder="Wyjaśnij autorowi, dlaczego treść została usunięta..."
                                            onChange={(event) =>
                                                setUpholdComment(event.target.value)
                                            }
                                        />
                                    </div>
                                )}

                                {!isHistoryReport && (
                                    <div className="moderation-actions">
                                        <button
                                            type="button"
                                            className="reject-button"
                                            onClick={handleDismissReport}
                                        >
                                            <XCircle size={18} />
                                            Odrzuć zgłoszenie
                                        </button>
                                        <button
                                            type="button"
                                            className="accept-button"
                                            onClick={handleOpenUpholdConfirm}
                                        >
                                            <CheckCircle2 size={18} />
                                            Zatwierdź zgłoszenie i usuń treść
                                        </button>
                                    </div>
                                )}
                            </>
                        ) : (
                            <>
                                <div className="preview-header">
                                    <h2>{selectedItem.title}</h2>
                                    <span>{selectedItem.category}</span>
                                </div>

                                <div className="preview-section">
                                    <label>Autor</label>
                                    <p>{selectedItem.author}</p>
                                </div>

                                <div className="preview-section">
                                    <label>Data</label>
                                    <p>
                                        {formatModerationDate(selectedItem.createdAt)}
                                    </p>
                                </div>

                                <div className="preview-section">
                                    <label>Treść</label>
                                    <div className="preview-box">
                                        {selectedItem.content}
                                    </div>
                                </div>

                                {showRejectForm && (
                                    <div className="preview-section">
                                        <label>Komentarz moderacji</label>
                                        <textarea
                                            className="admin-reject-textarea"
                                            value={rejectComment}
                                            placeholder="Wyjaśnij, dlaczego treść została odrzucona..."
                                            onChange={(event) =>
                                                setRejectComment(event.target.value)
                                            }
                                        />
                                    </div>
                                )}

                                {actionError && (
                                    <p className="admin-action-error">{actionError}</p>
                                )}

                                <div className="moderation-actions">
                                    <button
                                        type="button"
                                        className="accept-button"
                                        onClick={handleApprove}
                                    >
                                        <CheckCircle2 size={18} />
                                        Zatwierdź treść
                                    </button>

                                    {showRejectForm ? (
                                        <>
                                            <button
                                                type="button"
                                                className="reject-button"
                                                onClick={handleReject}
                                            >
                                                <XCircle size={18} />
                                                Potwierdź odrzucenie
                                            </button>
                                            <button
                                                type="button"
                                                className="comment-button"
                                                onClick={() => {
                                                    setShowRejectForm(false);
                                                    setRejectComment("");
                                                    setActionError("");
                                                }}
                                            >
                                                Anuluj
                                            </button>
                                        </>
                                    ) : (
                                        <button
                                            type="button"
                                            className="reject-button"
                                            onClick={() => {
                                                setShowRejectForm(true);
                                                setActionError("");
                                            }}
                                        >
                                            <XCircle size={18} />
                                            Odrzuć treść
                                        </button>
                                    )}
                                </div>
                            </>
                        )
                    ) : (
                        <div className="empty-preview">
                            <p>Wybierz treść do podglądu</p>
                        </div>
                    )}
                </div>
            </div>

            <ConfirmDialog
                isOpen={showUpholdConfirm}
                title="Zatwierdzić zgłoszenie i usunąć treść?"
                description={
                    selectedItem?.type === "report"
                        ? `Zatwierdzasz zgłoszenie dotyczące „${selectedItem.contentTitle}” (${getContentTypeLabel(selectedItem.contentType)}). Treść zostanie usunięta lub ukryta, a autor otrzyma Twój komentarz moderacji.`
                        : "Treść zostanie usunięta lub ukryta."
                }
                confirmLabel="Zatwierdź i usuń"
                cancelLabel="Anuluj"
                isConfirming={isUpholding}
                confirmingLabel="Usuwanie..."
                onConfirm={handleConfirmUpholdReport}
                onClose={handleCloseUpholdConfirm}
                variant="danger"
            />
        </div>
    );
}

export default AdminModeration;
