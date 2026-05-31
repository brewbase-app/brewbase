import { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { CheckCircle2, Flag } from "lucide-react";

import "../styles/ReportPage.css";

const REPORT_CATEGORIES = [
    "Dezinformacja",
    "Spam lub reklama",
    "Obraźliwa treść",
    "Niebezpieczne instrukcje",
    "Fałszywe informacje",
    "Naruszenie praw autorskich",
    "Duplikat treści",
    "spam",
    "Harassment / nękanie",
    "Inny problem",
];

const CONTENT_TYPE_LABELS = {
    recipe: "Receptura",
    coffee: "Kawa",
    article: "Artykuł wiki",
};

const COMMENT_MAX_LENGTH = 1000;

function ReportPage() {
    const location = useLocation();
    const navigate = useNavigate();

    const reportTarget = location.state;

    const [category, setCategory] = useState("");
    const [comment, setComment] = useState("");
    const [errors, setErrors] = useState({});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isSubmitted, setIsSubmitted] = useState(false);

    const validate = () => {
        const nextErrors = {};

        if (!category) {
            nextErrors.category = "Wybierz kategorię zgłoszenia.";
        }

        if (comment.length > COMMENT_MAX_LENGTH) {
            nextErrors.comment = `Komentarz może mieć maksymalnie ${COMMENT_MAX_LENGTH} znaków.`;
        }

        setErrors(nextErrors);
        return Object.keys(nextErrors).length === 0;
    };

    const handleSubmit = async (event) => {
        event.preventDefault();

        if (!validate()) {
            return;
        }

        setIsSubmitting(true);

        // Mock submit — gotowe do podłączenia backendu moderacji
        await new Promise((resolve) => {
            setTimeout(resolve, 700);
        });

        console.log("Report submitted (mock):", {
            contentType: reportTarget?.contentType,
            contentId: reportTarget?.contentId,
            contentTitle: reportTarget?.contentTitle,
            category,
            comment: comment.trim() || null,
        });

        setIsSubmitting(false);
        setIsSubmitted(true);
    };

    const handleCancel = () => {
        if (reportTarget?.returnPath) {
            navigate(reportTarget.returnPath);
            return;
        }

        navigate(-1);
    };

    if (!reportTarget?.contentType || reportTarget?.contentId == null) {
        return (
            <div className="report-page">
                <div className="report-page-layout">
                    <div className="report-empty-state">
                        <h2>Brak treści do zgłoszenia</h2>
                        <p>
                            Otwórz stronę szczegółów i użyj przycisku
                            &quot;Zgłoś treść&quot;, aby rozpocząć zgłoszenie.
                        </p>
                        <button
                            type="button"
                            className="report-submit-button"
                            onClick={() => navigate("/home")}
                        >
                            Wróć do dashboardu
                        </button>
                    </div>
                </div>
            </div>
        );
    }

    if (isSubmitted) {
        return (
            <div className="report-page">
                <div className="report-page-layout">
                    <div className="report-success-card">
                        <div className="report-success-icon">
                            <CheckCircle2 size={34} />
                        </div>
                        <h2>Zgłoszenie wysłane</h2>
                        <p>
                            Dziękujemy. Zespół moderacji przeanalizuje
                            zgłoszenie dotyczące &quot;{reportTarget.contentTitle}
                            &quot;.
                        </p>
                        <div className="report-success-actions">
                            {reportTarget.returnPath && (
                                <button
                                    type="button"
                                    className="report-submit-button"
                                    onClick={() =>
                                        navigate(reportTarget.returnPath)
                                    }
                                >
                                    Wróć do treści
                                </button>
                            )}
                            <button
                                type="button"
                                className="report-cancel-button"
                                onClick={() => navigate("/home")}
                            >
                                Dashboard
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    const contentTypeLabel =
        CONTENT_TYPE_LABELS[reportTarget.contentType] ?? "Treść";

    return (
        <div className="report-page">
            <div className="report-page-layout">
                <header className="report-page-header">
                    <p className="report-page-label">Moderacja</p>
                    <h1>Zgłoś treść</h1>
                    <p>
                        Pomóż nam utrzymać wysoką jakość społeczności BrewBase.
                        Twoje zgłoszenie trafi do zespołu moderacji.
                    </p>
                </header>

                <div className="report-form-card">
                    <div className="report-target-summary">
                        <span className="report-target-type">
                            {contentTypeLabel}
                        </span>
                        <span className="report-target-title">
                            {reportTarget.contentTitle}
                        </span>
                        <span className="report-target-id">
                            ID: {reportTarget.contentId}
                        </span>
                    </div>

                    <form
                        className="report-form"
                        onSubmit={handleSubmit}
                        noValidate
                    >
                        <div className="report-field">
                            <label htmlFor="report-category">
                                Kategoria zgłoszenia
                            </label>
                            <select
                                id="report-category"
                                className={`report-select ${
                                    errors.category
                                        ? "report-select--error"
                                        : ""
                                }`}
                                value={category}
                                onChange={(event) => {
                                    setCategory(event.target.value);
                                    if (errors.category) {
                                        setErrors((previous) => ({
                                            ...previous,
                                            category: undefined,
                                        }));
                                    }
                                }}
                            >
                                <option value="">
                                    Wybierz kategorię...
                                </option>
                                {REPORT_CATEGORIES.map((option) => (
                                    <option key={option} value={option}>
                                        {option}
                                    </option>
                                ))}
                            </select>
                            {errors.category && (
                                <p className="report-field-error">
                                    {errors.category}
                                </p>
                            )}
                        </div>

                        <div className="report-field">
                            <label htmlFor="report-comment">
                                Komentarz (opcjonalny)
                            </label>
                            <textarea
                                id="report-comment"
                                className={`report-textarea ${
                                    errors.comment
                                        ? "report-textarea--error"
                                        : ""
                                }`}
                                value={comment}
                                placeholder="Opisz problem dla zespołu moderacji..."
                                maxLength={COMMENT_MAX_LENGTH}
                                onChange={(event) => {
                                    setComment(event.target.value);
                                    if (errors.comment) {
                                        setErrors((previous) => ({
                                            ...previous,
                                            comment: undefined,
                                        }));
                                    }
                                }}
                            />
                            <span
                                className={`report-char-counter ${
                                    comment.length >= COMMENT_MAX_LENGTH
                                        ? "report-char-counter--limit"
                                        : ""
                                }`}
                            >
                                {comment.length}/{COMMENT_MAX_LENGTH}
                            </span>
                            {errors.comment && (
                                <p className="report-field-error">
                                    {errors.comment}
                                </p>
                            )}
                        </div>

                        <div className="report-form-actions">
                            <button
                                type="button"
                                className="report-cancel-button"
                                onClick={handleCancel}
                                disabled={isSubmitting}
                            >
                                Anuluj
                            </button>
                            <button
                                type="submit"
                                className="report-submit-button"
                                disabled={isSubmitting}
                            >
                                <Flag size={16} />
                                {isSubmitting
                                    ? "Wysyłanie..."
                                    : "Wyślij zgłoszenie"}
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
}

export default ReportPage;
