import { useNavigate } from "react-router-dom";
import { Heart } from "lucide-react";
const Recipes = () => {
    const navigate = useNavigate();

    return (
        <div style={{
            display: "flex",
            justifyContent: "center",
            alignItems: "flex-start",
            width: "100%",
            paddingTop: "80px"
        }}>
            <div style={{ width: "500px", textAlign: "center" }}>

                <h1 style={{
                    fontSize: "40px",
                    marginBottom: "50px",
                    fontFamily: "serif"
                }}>
                    Receptury
                </h1>

                <div style={{
                    display: "flex",
                    flexDirection: "column",
                    gap: "25px"
                }}>

                    <button
                        onClick={() => navigate("/recipes/new")}
                        style={buttonStyle}
                    >
                        + Dodaj recepturę
                    </button>

                    <button
                        onClick={() => navigate("/recipes/favorites")}
                        style={buttonStyle}
                    >
                        <div style={{ display: "flex", alignItems: "center", justifyContent: "center", gap: "10px" }}>
                            <Heart size={20} />
                            Polubione receptury
                        </div>
                    </button>

                    <button
                        onClick={() => navigate("/recipes/my")}
                        style={buttonStyle}
                    >
                        Twoje receptury
                    </button>

                </div>
            </div>
        </div>
    );
};

const buttonStyle = {
    backgroundColor: "#2f2f2f",
    color: "white",
    padding: "18px",
    borderRadius: "20px",
    fontSize: "18px",
    border: "none",
    cursor: "pointer"
};

export default Recipes;