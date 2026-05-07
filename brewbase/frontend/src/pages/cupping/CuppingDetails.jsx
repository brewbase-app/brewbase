import { useState } from "react";
import "../../styles/CuppingDetails.css";

const CuppingDetails = () => {
    const [cuppings, setCuppings] = useState([{ id: 1 }]);

    const addCupping = () => {
        setCuppings([...cuppings, { id: cuppings.length + 1 }]);
    };

    return (
        <div className="details-container">
            <h1 className="title">Cupping session</h1>
           

            {cuppings.map((cup, index) => (
                <div key={cup.id} className="cupping-block">
                    <h2 className="subtitle">Degustacja {index + 1}</h2>

                    <input className="coffee-input" placeholder="wprowadź nazwę kawy" />

                    <div className="grid">
                        <div className="box">
                            <h3>Aroma (1-10)</h3>
                            <input type="number" />
                            <textarea placeholder="Notatki" />
                        </div>

                        <div className="box">
                            <h3>Profile smakowe</h3>
                            <textarea placeholder="Notatki" />
                        </div>

                        <div className="box">
                            <h3>Słodycz (1-10)</h3>
                            <input type="number" />
                            <textarea placeholder="Notatki" />
                        </div>

                        <div className="box">
                            <h3>Czysta filiżanka</h3>

                            <div className="checkbox-group">
                                <label>
                                    <input type="radio" name={`cleanCup-${cup.id}`} /> Tak
                                </label>
                                <label>
                                    <input type="radio" name={`cleanCup-${cup.id}`} /> Nie
                                </label>
                            </div>
                        </div>

                        <div className="box">
                            <h3>Kwasowość (1-10)</h3>
                            <input type="number" />
                            <textarea placeholder="Notatki" />
                        </div>

                        <div className="box">
                            <h3>Dodatkowy komentarz</h3>
                            <textarea />
                        </div>

                        <div className="box">
                            <h3>Body (1-10)</h3>
                            <input type="number" />
                            <textarea placeholder="Notatki" />
                        </div>

                        <div className="box">
                            <h3>Ogólna ocena</h3>
                            <input type="number" />
                            <textarea placeholder="Notatki" />
                        </div>
                    </div>
                </div>
            ))}

            <button className="add-cup-btn" onClick={addCupping}>+ Dodaj kolejną degustację</button>
        </div>
    );
};

export default CuppingDetails;