import "../styles/FavoriteCoffees.css";

import { useNavigate } from "react-router-dom";

import {
    Heart,
    Star
} from "lucide-react";

function FavoriteCoffees() {

    const navigate = useNavigate();

    const favoriteCoffees = [
        {
            id: 1,
            name: "Etiopia Gedeb",
            region: "Etiopia",
            notes: "Jaśmin, bergamotka, miód",
            rating: 4.8,
        },
        {
            id: 2,
            name: "Kenya AA",
            region: "Kenia",
            notes: "Porzeczka, cytrusy",
            rating: 4.7,
        },
        {
            id: 3,
            name: "Pink Bourbon",
            region: "Kolumbia",
            notes: "Kwiaty, karmel",
            rating: 4.6,
        },
        {
            id: 4,
            name: "Brazil Fazenda",
            region: "Brazylia",
            notes: "Orzechy, czekolada",
            rating: 4.5,
        },
    ];

    return (

        <div className="favorite-coffees-page">

            <div className="favorite-coffees-header">

                <h1>Ulubione kawy</h1>

                <p>
                    Wszystkie kawy oznaczone przez Ciebie jako ulubione.
                </p>

            </div>

            <div className="favorite-coffees-list">

                {favoriteCoffees.map((coffee) => (

                    <div
                        className="favorite-coffee-card"
                        key={coffee.id}
                        onClick={() =>
                            navigate(`/wiki/coffees/${coffee.id}`)
                        }
                    >

                        <div className="favorite-coffee-image"></div>

                        <div className="favorite-coffee-content">

                            <div className="favorite-coffee-top">

                                <div>

                                    <h3>{coffee.name}</h3>

                                    <span>{coffee.region}</span>

                                </div>

                                <Heart
                                    size={20}
                                    fill="black"
                                />

                            </div>

                            <p>
                                {coffee.notes}
                            </p>

                            <div className="favorite-coffee-rating">

                                <Star size={15} />

                                {coffee.rating}

                            </div>

                        </div>

                    </div>

                ))}

            </div>

        </div>

    );
}

export default FavoriteCoffees;