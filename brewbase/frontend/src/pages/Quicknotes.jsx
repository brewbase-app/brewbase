import { useState, useEffect, useRef } from "react";
import { useSearchParams } from "react-router-dom";
import "../styles/Quicknotes.css";
import {
    Pencil,
    Trash2,
    Download,
    Plus
} from "lucide-react";
import {
    getQuickNotes,
    createQuickNote,
    updateQuickNote,
    deleteQuickNote
} from "../api/quickNotesApi";
import ConfirmDialog from "../components/ConfirmDialog";

function Quicknotes() {
    const [searchParams, setSearchParams] = useSearchParams();
    const initialNoteId = useRef(searchParams.get("id"));

    const [search, setSearch] = useState("");
    const [noteContent, setNoteContent] = useState("");
    const [selectedNote, setSelectedNote] = useState(null);
    const [notes, setNotes] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isSaving, setIsSaving] = useState(false);
    const [isDeleting, setIsDeleting] = useState(false);
    const [noteToDelete, setNoteToDelete] = useState(null);
    const [error, setError] = useState("");

    useEffect(() => {
        const loadNotes = async () => {
            try {
                setError("");
                const data = await getQuickNotes();
                setNotes(data);

                if (initialNoteId.current) {
                    const noteId = Number(initialNoteId.current);
                    const note = data.find((item) => item.id === noteId);

                    if (note) {
                        setSelectedNote(note.id);
                        setNoteContent(note.content);
                    }

                    initialNoteId.current = null;
                    setSearchParams({}, { replace: true });
                }
            } catch {
                setError("Nie udało się pobrać notatek.");
            } finally {
                setIsLoading(false);
            }
        };

        loadNotes();
    }, [setSearchParams]);

    const filteredNotes = notes.filter((note) =>
        note.content.toLowerCase().includes(search.toLowerCase())
    );

    const formatDate = (date) => {
        if (!date) {
            return "Brak daty";
        }

        return new Date(date).toLocaleDateString("pl-PL");
    };

    const getTitle = (content) => {
        const line = content.split("\n")[0].trim();

        if (!line) {
            return "Notatka bez treści";
        }

        if (line.length <= 40) {
            return line;
        }

        return `${line.substring(0, 40)}...`;
    };

    const getPreview = (content) => {
        const trimmed = content.trim();

        if (trimmed.length <= 40) {
            return "";
        }

        const preview = trimmed.substring(40, 100);

        if (trimmed.length > 100) {
            return `${preview}...`;
        }

        return preview;
    };

    const handleNewNote = () => {
        setSelectedNote(null);
        setNoteContent("");
        setError("");
    };

    const handleEdit = (note) => {
        setSelectedNote(note.id);
        setNoteContent(note.content);
        setError("");
    };

    const handleSaveNote = async () => {
        const content = noteContent.trim();

        if (!content || isSaving) {
            return;
        }

        setIsSaving(true);
        setError("");

        try {
            if (selectedNote) {
                const updated = await updateQuickNote(selectedNote, content);
                setNotes((current) =>
                    current.map((note) =>
                        note.id === selectedNote ? updated : note
                    )
                );
            } else {
                const created = await createQuickNote(content);
                setNotes((current) => [created, ...current]);
            }

            setNoteContent("");
            setSelectedNote(null);
        } catch {
            setError("Nie udało się zapisać notatki.");
        } finally {
            setIsSaving(false);
        }
    };

    const handleDeleteClick = (id, event) => {
        event.stopPropagation();
        setNoteToDelete(id);
        setError("");
    };

    const handleCloseDeleteDialog = () => {
        if (!isDeleting) {
            setNoteToDelete(null);
        }
    };

    const handleConfirmDelete = async () => {
        if (noteToDelete == null || isDeleting) {
            return;
        }

        const id = noteToDelete;

        setIsDeleting(true);
        setError("");

        try {
            await deleteQuickNote(id);
            setNotes((current) => current.filter((note) => note.id !== id));

            if (selectedNote === id) {
                setSelectedNote(null);
                setNoteContent("");
            }

            setNoteToDelete(null);
        } catch {
            setError("Nie udało się usunąć notatki.");
        } finally {
            setIsDeleting(false);
        }
    };

    const handleDownloadTXT = (note, event) => {
        event.stopPropagation();

        const blob = new Blob([note.content], { type: "text/plain" });
        const url = URL.createObjectURL(blob);
        const link = document.createElement("a");

        link.href = url;
        link.download = `notatka-${note.id}.txt`;
        link.click();

        URL.revokeObjectURL(url);
    };

    const handleEditClick = (note, event) => {
        event.stopPropagation();
        handleEdit(note);
    };

    if (isLoading) {
        return (
            <div className="quicknotes-page">
                <div className="quicknotes-loading">
                    <p>Ładowanie notatek...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="quicknotes-page">
            <div className="quicknotes-container">
                <div className="quicknotes-editor-section">
                    <h1>Szybkie notatki</h1>

                    <p className="quicknotes-editor-label">
                        {selectedNote ? "Edycja notatki" : "Nowa notatka"}
                    </p>

                    <textarea
                        placeholder="Twoja szybka notatka..."
                        className="quicknotes-textarea"
                        value={noteContent}
                        onChange={(event) => setNoteContent(event.target.value)}
                        disabled={isSaving}
                    />

                    {error && (
                        <p className="quicknotes-error">{error}</p>
                    )}

                    <div className="quicknotes-editor-actions">
                        <button
                            type="button"
                            className="new-note-button"
                            onClick={handleNewNote}
                            disabled={isSaving}
                        >
                            <Plus size={18} />
                            Nowa notatka
                        </button>

                        <button
                            type="button"
                            className="save-note-button"
                            onClick={handleSaveNote}
                            disabled={isSaving || !noteContent.trim()}
                        >
                            {isSaving
                                ? "Zapisywanie..."
                                : selectedNote
                                    ? "Zapisz zmiany"
                                    : "Zapisz notatkę"}
                        </button>
                    </div>
                </div>

                <div className="quicknotes-sidebar">
                    <h2>Twoje notatki</h2>

                    <input
                        type="text"
                        placeholder="Szukaj notatek..."
                        className="quicknotes-search"
                        value={search}
                        onChange={(event) => setSearch(event.target.value)}
                    />

                    <div className="quicknotes-list">
                        {filteredNotes.length === 0 ? (
                            <p className="quicknotes-empty">
                                {search
                                    ? "Brak notatek pasujących do wyszukiwania."
                                    : "Nie masz jeszcze notatek. Użyj „Nowa notatka”, aby dodać pierwszą."}
                            </p>
                        ) : (
                            filteredNotes.map((note) => (
                                <div
                                    className={`note-card${selectedNote === note.id ? " note-card-active" : ""}`}
                                    key={note.id}
                                    onClick={() => handleEdit(note)}
                                    onKeyDown={(event) => {
                                        if (event.key === "Enter" || event.key === " ") {
                                            event.preventDefault();
                                            handleEdit(note);
                                        }
                                    }}
                                    role="button"
                                    tabIndex={0}
                                >
                                    <div className="note-card-header">
                                        <h3>{getTitle(note.content)}</h3>

                                        <div className="note-icons">
                                            <Pencil
                                                size={16}
                                                strokeWidth={2}
                                                onClick={(event) =>
                                                    handleEditClick(note, event)
                                                }
                                            />

                                            <Download
                                                size={16}
                                                strokeWidth={2}
                                                onClick={(event) =>
                                                    handleDownloadTXT(note, event)
                                                }
                                            />

                                            <Trash2
                                                size={16}
                                                strokeWidth={2}
                                                onClick={(event) =>
                                                    handleDeleteClick(note.id, event)
                                                }
                                            />
                                        </div>
                                    </div>

                                    {getPreview(note.content) && (
                                        <p>{getPreview(note.content)}</p>
                                    )}

                                    <span>
                                        Ostatnia aktualizacja:
                                        {" "}
                                        {formatDate(note.updatedAt || note.createdAt)}
                                    </span>
                                </div>
                            ))
                        )}
                    </div>
                </div>
            </div>

            <ConfirmDialog
                isOpen={noteToDelete !== null}
                title="Usunąć notatkę?"
                description="Tej operacji nie można cofnąć."
                confirmLabel="Usuń"
                cancelLabel="Anuluj"
                isConfirming={isDeleting}
                confirmingLabel="Usuwanie..."
                onConfirm={handleConfirmDelete}
                onClose={handleCloseDeleteDialog}
            />
        </div>
    );
}

export default Quicknotes;
