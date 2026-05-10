import { useParams } from "react-router-dom";

import "../../styles/wiki/RegionDetails.css";

function RegionDetails() {

    const { id } = useParams();

    const regions = [

        {
            id: 1,
            name: "Yirgacheffe",
            country: "Etiopia",
            description:
                "Yirgacheffe to jeden z najbardziej rozpoznawalnych regionów uprawy kaw specialty na świecie. Region słynie z wysokiej jakości arabik o floralnym i herbacianym charakterze.",
            altitude: "1800–2200 m n.p.m.",
            characteristics:
                "Kawy z regionu Yirgacheffe charakteryzują się wysoką kwasowością, delikatnym body oraz bardzo złożonym profilem aromatycznym. Często można wyczuć nuty jaśminu, cytrusów i herbaty.",
            terroir:
                "Region charakteryzuje się wysokimi wysokościami upraw, żyznymi glebami oraz chłodnym klimatem sprzyjającym powolnemu dojrzewaniu owoców kawowca."
        },

        {
            id: 2,
            name: "Huila",
            country: "Kolumbia",
            description:
                "Huila to jeden z najbardziej cenionych regionów specialty coffee w Kolumbii. Kawy z tego regionu są słodkie, zbalansowane i bardzo kompleksowe.",
            altitude: "1400–2000 m n.p.m.",
            characteristics:
                "Kawy z regionu Huila są zazwyczaj bardzo słodkie, z wyraźnymi nutami karmelu, czerwonych owoców oraz czekolady. Region słynie z produkcji bardzo czystych i zbalansowanych kaw.",
            terroir:
                "Region posiada wulkaniczne gleby oraz bardzo korzystny mikroklimat, co pozwala na produkcję wysokiej jakości kaw specialty."
        }
    ];

    const region = regions.find(
        (region) => region.id === Number(id)
    );

    if (!region) {
        return <h1>Nie znaleziono regionu.</h1>;
    }

    return (

        <div className="region-page">

            <div className="region-hero">

                <div className="region-overlay">

                    <span>
                        {region.country}
                    </span>

                    <h1>
                        {region.name}
                    </h1>

                    <div className="region-tags">

                        <div>
                            {region.altitude}
                        </div>

                    </div>

                </div>

            </div>

            <div className="region-content">

                <section>

                    <h2>Opis regionu</h2>

                    <p>
                        {region.description}
                    </p>

                </section>

                <section>

                    <h2>Kraj</h2>

                    <div className="details-tags">

                        <span>
                            {region.country}
                        </span>

                    </div>

                </section>

                <section>

                    <h2>Charakterystyka regionu</h2>

                    <p>
                        {region.characteristics}
                    </p>

                </section>

                <section>

                    <h2>Klimat i terroir</h2>

                    <p>
                        {region.terroir}
                    </p>

                </section>

            </div>

        </div>

    );
}

export default RegionDetails;