import { useParams } from "react-router-dom";

import "../../styles/wiki/BrewingMethodDetails.css";

function BrewingMethodDetails() {

    const { id } = useParams();

    const methods = [

        {
            id: 1,
            name: "V60",
            type: "Pour over",
            difficulty: "Średni",
            brewTime: "2:30–3:00",
            grindSize: "Medium-fine",
            ratio: "1:16",
            waterTemperature: "92–96°C",
            description:
                "V60 to jedna z najpopularniejszych metod przelewowych w świecie specialty coffee. Pozwala uzyskać bardzo czysty, złożony i aromatyczny napar.",
            characteristics:
                "Metoda V60 podkreśla klarowność smaku, wysoką kwasowość oraz złożoność aromatyczną kawy.",
            recipe:
                "Zalej kawę niewielką ilością wody na bloom przez około 30 sekund, następnie dolewaj wodę kolistymi ruchami do osiągnięcia docelowej objętości."
        },

        {
            id: 2,
            name: "Chemex",
            type: "Pour over",
            difficulty: "Łatwy",
            brewTime: "4:00–5:00",
            grindSize: "Medium-coarse",
            ratio: "1:15",
            waterTemperature: "93–96°C",
            description:
                "Chemex to elegancka metoda przelewowa wykorzystująca grube papierowe filtry zapewniające bardzo czysty napar.",
            characteristics:
                "Chemex daje delikatne, lekkie body oraz bardzo wysoką klarowność smaku.",
            recipe:
                "Wlej wodę na bloom przez 30 sekund, następnie powoli dolewaj wodę partiami aż do osiągnięcia pełnej ekstrakcji."
        }
    ];

    const method = methods.find(
        (method) => method.id === Number(id)
    );

    if (!method) {
        return <h1>Nie znaleziono metody.</h1>;
    }

    return (

        <div className="method-details-page">

            <div className="method-details-hero">

                <div className="method-details-overlay">

                    <span>
                        {method.type}
                    </span>

                    <h1>
                        {method.name}
                    </h1>

                    <div className="method-details-tags">

                        <div>
                            {method.difficulty}
                        </div>

                        <div>
                            {method.brewTime}
                        </div>

                    </div>

                </div>

            </div>

            <div className="method-details-content">

                <section>

                    <h2>Opis metody</h2>

                    <p>
                        {method.description}
                    </p>

                </section>

                <section>

                    <h2>Charakterystyka parzenia</h2>

                    <p>
                        {method.characteristics}
                    </p>

                </section>

                <section>

                    <h2>Parametry parzenia</h2>

                    <div className="details-tags">

                        <span>
                            Grind: {method.grindSize}
                        </span>

                        <span>
                            Ratio: {method.ratio}
                        </span>

                        <span>
                            Temperatura: {method.waterTemperature}
                        </span>

                    </div>

                </section>

                <section>

                    <h2>Rekomendowany sposób parzenia</h2>

                    <p>
                        {method.recipe}
                    </p>

                </section>

            </div>

        </div>

    );
}

export default BrewingMethodDetails;