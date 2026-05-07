
import { useParams } from "react-router-dom";

const recipes = [
    {
        id: 1,
        title: "V60 klasyczny balans",
        coffee: "Ethiopia",
        brewingMethod: "V60",
        steps:
            "1. Zmiel kawę\n2. Przelej filtr\n3. Zalej wodą powoli",
        parameters: {
            coffee: "18g",
            water: "300ml",
            temperature: "93°C",
        },
        isPublic: true,
    },
    {
        id: 2,
        title: "AeroPress szybki i słodki",
        coffee: "Brazil Santos",
        brewingMethod: "AeroPress",
        steps:
            "1. Wsyp kawę\n2. Dodaj wodę\n3. Zamieszaj i przeciskaj",
        parameters: {
            coffee: "15g",
            water: "250ml",
            temperature: "90°C",
        },
        isPublic: false,
    },
];

const RecipeDetails = () => {
    const { id } = useParams();

    const recipe = recipes.find((r) => r.id === Number(id));

    if (!recipe) {
        return <h1>Nie znaleziono receptury</h1>;
    }

    return (
        <div
            style={{
                display: "flex",
                justifyContent: "center",
                paddingTop: "60px",
            }}
        >
            <div
                style={{
                    width: "700px",
                    backgroundColor: "#e5e5e5",
                    padding: "40px",
                    borderRadius: "20px",
                    border: "1px solid #999",
                }}
            >
                <h1
                    style={{
                        fontSize: "40px",
                        marginBottom: "30px",
                        fontFamily: "serif",
                    }}
                >
                    {recipe.title}
                </h1>

                <p>
                    <b>Kawa:</b> {recipe.coffee}
                </p>

                <p>
                    <b>Metoda:</b> {recipe.brewingMethod}
                </p>

                <p>
                    <b>Publiczna:</b>{" "}
                    {recipe.isPublic ? "Tak" : "Nie"}
                </p>

                <div style={{ marginTop: "30px" }}>
                    <h3>Kroki</h3>

                    <p style={{ whiteSpace: "pre-line" }}>
                        {recipe.steps}
                    </p>
                </div>

                <div style={{ marginTop: "30px" }}>
                    <h3>Parametry</h3>

                    <div
                        style={{
                            backgroundColor: "#d6d6d6",
                            padding: "20px",
                            borderRadius: "10px",
                        }}
                    >
                        <p>
                            <b>Kawa:</b> {recipe.parameters.coffee}
                        </p>

                        <p>
                            <b>Woda:</b> {recipe.parameters.water}
                        </p>

                        <p>
                            <b>Temperatura:</b>{" "}
                            {recipe.parameters.temperature}
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default RecipeDetails;
