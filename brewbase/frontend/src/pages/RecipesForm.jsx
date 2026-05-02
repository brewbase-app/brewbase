const RecipesForm = () => {
    return (
        <div style={{
            display: "flex",
            justifyContent: "center",
            paddingTop: "60px",
            width: "100%"
        }}>
            <div style={{ width: "800px" }}>

                <h1 style={{
                    textAlign: "center",
                    fontSize: "40px",
                    marginBottom: "40px",
                    fontFamily: "serif"
                }}>
                    Nowa receptura
                </h1>

                <div style={{ display: "flex", flexDirection: "column", gap: "15px" }}>

                    <input placeholder="Nazwa receptury" style={inputStyle} />

                    <textarea
                        placeholder="Opis przygotowania"
                        style={{ ...inputStyle, height: "100px" }}
                    />

                    <input placeholder="Metoda parzenia" style={inputStyle} />

                    <hr />

                    <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "15px" }}>
                        <input placeholder="Ilość kawy (g)" style={inputStyle} />
                        <input placeholder="Ilość wody (ml)" style={inputStyle} />

                        <input placeholder="Temperatura wody (°C)" style={inputStyle} />
                        <input placeholder="Stopień mielenia" style={inputStyle} />
                    </div>

                    <p style={{ marginTop: "20px" }}>Całkowity czas parzenia:</p>

                    <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "15px" }}>
                        <input placeholder="Minuty" style={inputStyle} />
                        <input placeholder="Sekundy" style={inputStyle} />
                    </div>

                    <div style={{ display: "flex", justifyContent: "flex-end", marginTop: "30px" }}>
                        <button style={buttonStyle}>
                            Zapisz recepturę
                        </button>
                    </div>

                </div>
            </div>
        </div>
    );
};

const inputStyle = {
    padding: "12px",
    borderRadius: "12px",
    border: "1px solid #999",
    backgroundColor: "#e5e5e5",
    width: "100%",
    boxSizing: "border-box" 
};

const buttonStyle = {
    backgroundColor: "#2f2f2f",
    color: "white",
    padding: "12px 20px",
    borderRadius: "20px",
    border: "none",
    cursor: "pointer"
};

export default RecipesForm;