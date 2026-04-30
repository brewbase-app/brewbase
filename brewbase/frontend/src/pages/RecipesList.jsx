const myRecipes = [
    { id: 1, name: "Receptura 1", date: "01-01-2026" },
    { id: 2, name: "Receptura 2", date: "01-01-2026" },
    { id: 3, name: "Receptura 3", date: "01-01-2026" },
];

const favoriteRecipes = [
    { id: 4, name: "Espresso idealne", date: "02-01-2026" },
    { id: 5, name: "V60 poranek", date: "03-01-2026" },
];

const RecipesList = ({ title }) => {
    // 🔥 wybór danych
    const data =
        title === "Ulubione receptury" ? favoriteRecipes : myRecipes;

    return (
        <div
            style={{
                display: "flex",
                justifyContent: "center",
                paddingTop: "60px",
                width: "100%",
            }}
        >
            <div style={{ width: "700px" }}>
                <h1
                    style={{
                        textAlign: "center",
                        fontSize: "40px",
                        marginBottom: "40px",
                        fontFamily: "serif",
                    }}
                >
                    {title}
                </h1>

                <div
                    style={{
                        display: "flex",
                        flexDirection: "column",
                        gap: "20px",
                    }}
                >
                    {data.map((r) => (
                        <div key={r.id} style={cardStyle}>
                            <div>
                                <p>
                                    Nazwa: <b>{r.name}</b>
                                </p>
                                <p style={{ fontSize: "12px", color: "#555" }}>
                                    Data: {r.date}
                                </p>
                            </div>

                            <button style={buttonStyle}>Szczegóły</button>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
};

const cardStyle = {
    backgroundColor: "#e5e5e5",
    borderRadius: "20px",
    padding: "20px",
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    border: "1px solid #999",
};

const buttonStyle = {
    backgroundColor: "#2f2f2f",
    color: "white",
    padding: "8px 20px",
    borderRadius: "20px",
    border: "none",
    cursor: "pointer",
};

export default RecipesList;