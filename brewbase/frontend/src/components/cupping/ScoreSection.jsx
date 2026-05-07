const ScoreSection = ({ label }) => {
    return (
        <div className="score-section">
            <h3>{label}: 1-10</h3>
            <input type="number" min="1" max="10" />

            <textarea placeholder="Notatki..." />
        </div>
    );
};

export default ScoreSection;