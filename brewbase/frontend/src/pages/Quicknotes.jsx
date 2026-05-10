import { useState, useEffect } from "react";
import "../styles/Quicknotes.css";
import { Pencil, Trash2 } from "lucide-react";

function Quicknotes() {

    const [search, setSearch] = useState("");

    const [noteContent, setNoteContent] = useState("");

    const [selectedNote, setSelectedNote] = useState(null);

    const [notes, setNotes] = useState(() => {

        const savedNotes = localStorage.getItem("quicknotes");

        return savedNotes ? JSON.parse(savedNotes) : [];

    });

    useEffect(() => {

        localStorage.setItem(
            "quicknotes",
            JSON.stringify(notes)
        );

    }, [notes]);

    const filteredNotes = notes.filter((note) =>
        note.content.toLowerCase().includes(search.toLowerCase())
    );

    const getTitle = (content) => {

        if (content.length <= 30) {
            return content;
        }

        return content.substring(0, 30) + "...";
    };

    const getPreview = (content) => {

        if (content.length <= 60) {
            return content.substring(30);
        }

        return content.substring(30, 60) + "...";
    };

    const handleEdit = (note) => {

        setSelectedNote(note.id);

        setNoteContent(note.content);
    };

    const handleSaveNote = () => {

        if (!noteContent.trim()) return;

        if (selectedNote) {

            const updatedNotes = notes.map((note) =>

                note.id === selectedNote
                    ? {
                        ...note,
                        content: noteContent,
                        date: new Date().toLocaleDateString()
                    }
                    : note
            );

            setNotes(updatedNotes);

        } else {

            const newNote = {
                id: Date.now(),
                content: noteContent,
                date: new Date().toLocaleDateString()
            };

            setNotes([newNote, ...notes]);
        }

        setNoteContent("");

        setSelectedNote(null);
    };

    const handleDelete = (id) => {

        const confirmDelete = window.confirm(
            "Czy na pewno chcesz usunąć tę notatkę?"
        );

        if (!confirmDelete) return;

        const updatedNotes = notes.filter(
            (note) => note.id !== id
        );

        setNotes(updatedNotes);

        if (selectedNote === id) {

            setSelectedNote(null);

            setNoteContent("");
        }
    };

    return (

        <div className="quicknotes-page">

            <div className="quicknotes-container">

                <div className="quicknotes-editor-section">

                    <h1>Quick Notes</h1>

                    <textarea
                        placeholder="Twoja szybka notatka..."
                        className="quicknotes-textarea"
                        value={noteContent}
                        onChange={(e) => setNoteContent(e.target.value)}
                    />

                    <button
                        className="save-note-button"
                        onClick={handleSaveNote}
                    >
                        {selectedNote ? "Zapisz zmiany" : "Zapisz notatkę"}
                    </button>

                </div>

                <div className="quicknotes-sidebar">

                    <h2>Twoje notatki</h2>

                    <input
                        type="text"
                        placeholder="Szukaj notatek..."
                        className="quicknotes-search"
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                    />

                    <div className="quicknotes-list">

                        {filteredNotes.map((note) => (

                            <div className="note-card" key={note.id}>

                                <div className="note-card-header">

                                    <h3>{getTitle(note.content)}</h3>

                                    <div className="note-icons">

                                        <Pencil
                                            size={16}
                                            strokeWidth={2}
                                            onClick={() => handleEdit(note)}
                                        />

                                        <Trash2
                                            size={16}
                                            strokeWidth={2}
                                            onClick={() => handleDelete(note.id)}
                                        />

                                    </div>

                                </div>

                                <p>{getPreview(note.content)}</p>

                                <span>
                                    Ostatnia aktualizacja: {note.date}
                                </span>

                            </div>

                        ))}

                    </div>

                </div>

            </div>

        </div>
    );
}

export default Quicknotes;