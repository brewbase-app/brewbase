import { useEffect, useRef } from "react";
import "../styles/components/ConfirmDialog.css";

function ConfirmDialog({
    isOpen,
    title,
    description,
    confirmLabel = "Potwierdź",
    cancelLabel = "Anuluj",
    isConfirming = false,
    confirmingLabel,
    onConfirm,
    onClose,
    variant = "danger",
}) {
    const cancelButtonRef = useRef(null);

    useEffect(() => {
        if (!isOpen) {
            return undefined;
        }

        const previousOverflow = document.body.style.overflow;
        document.body.style.overflow = "hidden";

        cancelButtonRef.current?.focus();

        const handleKeyDown = (event) => {
            if (event.key === "Escape" && !isConfirming) {
                onClose();
            }
        };

        window.addEventListener("keydown", handleKeyDown);

        return () => {
            document.body.style.overflow = previousOverflow;
            window.removeEventListener("keydown", handleKeyDown);
        };
    }, [isOpen, isConfirming, onClose]);

    if (!isOpen) {
        return null;
    }

    const handleOverlayClick = () => {
        if (!isConfirming) {
            onClose();
        }
    };

    const handleConfirm = () => {
        if (!isConfirming) {
            onConfirm();
        }
    };

    return (
        <div
            className="confirm-dialog-overlay"
            onClick={handleOverlayClick}
            role="presentation"
        >
            <div
                className="confirm-dialog"
                role="dialog"
                aria-modal="true"
                aria-labelledby="confirm-dialog-title"
                aria-describedby="confirm-dialog-description"
                onClick={(event) => event.stopPropagation()}
            >
                <h2 id="confirm-dialog-title" className="confirm-dialog-title">
                    {title}
                </h2>

                {description && (
                    <p
                        id="confirm-dialog-description"
                        className="confirm-dialog-description"
                    >
                        {description}
                    </p>
                )}

                <div className="confirm-dialog-actions">
                    <button
                        ref={cancelButtonRef}
                        type="button"
                        className="confirm-dialog-button confirm-dialog-button-cancel"
                        onClick={onClose}
                        disabled={isConfirming}
                    >
                        {cancelLabel}
                    </button>

                    <button
                        type="button"
                        className={`confirm-dialog-button confirm-dialog-button-confirm confirm-dialog-button-${variant}`}
                        onClick={handleConfirm}
                        disabled={isConfirming}
                    >
                        {isConfirming
                            ? (confirmingLabel ?? `${confirmLabel}...`)
                            : confirmLabel}
                    </button>
                </div>
            </div>
        </div>
    );
}

export default ConfirmDialog;
