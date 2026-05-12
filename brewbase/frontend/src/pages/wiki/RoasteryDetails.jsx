import { useParams } from "react-router-dom";

import {
    Flame,
    MapPin,
    Globe
} from "lucide-react";

import "../../styles/wiki/RoasteryDetails.css";

function RoasteryDetails() {

    const { id } = useParams();

    const roasteries = [

        {
            id: 1,
            name: "Coffee Collective",
            city: "Kopenhaga",
            country: "Dania",
            founded: "2007",
            roastingStyle: "Light Roast",

            description:
                "Coffee Collective to jedna z najbardziej rozpoznawalnych palarni specialty coffee w Europie. Znana jest z transparentności sourcingu oraz jasnego profilu palenia.",

            specialties: [
                "Filter Coffee",
                "Nordic Roast",
                "Single Origin"
            ],

            sourcingRegions: [
                "Etiopia",
                "Kenia",
                "Kolumbia"
            ]
        },

        {
            id: 2,
            name: "Audun Coffee",
            city: "Oslo",
            country: "Norwegia",
            founded: "2014",
            roastingStyle: "Omni Roast",

            description:
                "Audun Coffee specjalizuje się w kawach konkursowych i wysokiej jakości ziarnach specialty coffee wypalanych pod metody przelewowe.",

            specialties: [
                "Competition Coffee",
                "Light Roast",
                "Filter Coffee"
            ],

            sourcingRegions: [
                "Panama",
                "Etiopia",
                "Kostaryka"
            ]
        }
    ];

    const roastery = roasteries.find(
        (item) => item.id === Number(id)
    );

    if (!roastery) {

        return (
            <h1>
                Nie znaleziono palarni.
            </h1>
        );
    }

    return (

        <div className="roastery-details-page">

            {/* HERO */}

            <div className="roastery-details-hero">

                <div className="roastery-details-overlay">

                    <span>

                        {roastery.city},
                        {" "}
                        {roastery.country}

                    </span>

                    <h1>
                        {roastery.name}
                    </h1>

                    <div className="roastery-details-tags">

                        <div>

                            <Flame size={15} />

                            {roastery.roastingStyle}

                        </div>

                        <div>

                            <Globe size={15} />

                            Founded {roastery.founded}

                        </div>

                    </div>

                </div>

            </div>

            {/* CONTENT */}

            <div className="roastery-details-content">

                <section>

                    <h2>
                        O palarni
                    </h2>

                    <p>
                        {roastery.description}
                    </p>

                </section>

                <section>

                    <h2>
                        Specjalizacja
                    </h2>

                    <div className="details-tags">

                        {roastery.specialties.map((item) => (

                            <span key={item}>
                                {item}
                            </span>

                        ))}

                    </div>

                </section>

                <section>

                    <h2>
                        Regiony sourcingu
                    </h2>

                    <div className="details-tags">

                        {roastery.sourcingRegions.map((region) => (

                            <span key={region}>
                                {region}
                            </span>

                        ))}

                    </div>

                </section>

            </div>

        </div>

    );
}

export default RoasteryDetails;