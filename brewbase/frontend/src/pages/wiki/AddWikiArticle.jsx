import { useState } from "react";

import "../../styles/wiki/AddWikiArticle.css";

import {
    Send,
    ChevronDown
} from "lucide-react";

function AddWikiArticle() {

    const [title, setTitle] = useState("");

    const [category, setCategory] = useState("");

    const [content, setContent] = useState("");

    const [files, setFiles] = useState([]);

    return (

        <div className="add-article-page">

            <div className="add-article-container">

                <div className="add-article-header">

                    <h1>
                        Dodaj treść do wiki
                    </h1>

                    <p>
                        Dodaj własny artykuł do encyklopedii kawy.
                        <br />
                        Po przesłaniu treść zostanie przekazana
                        do moderacji i akceptacji.
                    </p>

                </div>

                <div className="article-form">

                    {/* CATEGORY */}

                    <div className="form-group">

                        <label>
                            Kategoria
                        </label>

                        <div className="select-wrapper category-select">

                            <select
                                value={category}
                                onChange={(e) =>
                                    setCategory(e.target.value)
                                }
                            >

                                <option value="">
                                    Wybierz kategorię
                                </option>

                                <option value="coffee">
                                    Kawy
                                </option>

                                <option value="region">
                                    Regiony
                                </option>

                                <option value="brewing">
                                    Metody parzenia
                                </option>

                                <option value="roastery">
                                    Palarnie
                                </option>

                            </select>

                            <ChevronDown size={18} />

                        </div>

                    </div>

                    {/* COFFEES */}

                    {category === "coffee" && (

                        <>

                            <div className="form-group">

                                <label>
                                    Nazwa kawy
                                </label>

                                <input
                                    type="text"
                                    placeholder="Np. Geisha z Panamy"
                                    value={title}
                                    onChange={(e) =>
                                        setTitle(e.target.value)
                                    }
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Region
                                </label>

                                <div className="select-wrapper">

                                    <select>

                                        <option value="">
                                            Wybierz region
                                        </option>

                                        <option value="Etiopia">
                                            Etiopia
                                        </option>

                                        <option value="Kenia">
                                            Kenia
                                        </option>

                                        <option value="Kolumbia">
                                            Kolumbia
                                        </option>

                                    </select>

                                    <ChevronDown size={18} />

                                </div>

                            </div>

                            <div className="form-group">

                                <label>
                                    Variety
                                </label>

                                <div className="select-wrapper">

                                    <select>

                                        <option value="">
                                            Wybierz variety
                                        </option>

                                        <option value="Geisha">
                                            Geisha
                                        </option>

                                        <option value="Bourbon">
                                            Bourbon
                                        </option>

                                        <option value="Typica">
                                            Typica
                                        </option>

                                    </select>

                                    <ChevronDown size={18} />

                                </div>

                            </div>

                            <div className="form-group">

                                <label>
                                    Processing method
                                </label>

                                <div className="select-wrapper">

                                    <select>

                                        <option value="">
                                            Wybierz processing
                                        </option>

                                        <option value="Washed">
                                            Washed
                                        </option>

                                        <option value="Natural">
                                            Natural
                                        </option>

                                        <option value="Honey">
                                            Honey
                                        </option>

                                    </select>

                                    <ChevronDown size={18} />

                                </div>

                            </div>

                            <div className="form-group">

                                <label>
                                    Profil smakowy
                                </label>

                                <input
                                    type="text"
                                    placeholder="Np. jaśmin, cytrusy, miód"
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Opis kawy
                                </label>

                                <textarea
                                    placeholder="Dodaj opis kawy..."
                                    value={content}
                                    onChange={(e) =>
                                        setContent(e.target.value)
                                    }
                                />

                            </div>

                        </>

                    )}

                    {/* REGIONS */}

                    {category === "region" && (

                        <>

                            <div className="form-group">

                                <label>
                                    Nazwa regionu
                                </label>

                                <input
                                    type="text"
                                    placeholder="Np. Yirgacheffe"
                                    value={title}
                                    onChange={(e) =>
                                        setTitle(e.target.value)
                                    }
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Kraj
                                </label>

                                <input
                                    type="text"
                                    placeholder="Np. Etiopia"
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Wysokość upraw
                                </label>

                                <input
                                    type="text"
                                    placeholder="Np. 1800–2200 m n.p.m."
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Opis regionu
                                </label>

                                <textarea
                                    placeholder="Dodaj opis regionu..."
                                    value={content}
                                    onChange={(e) =>
                                        setContent(e.target.value)
                                    }
                                />

                            </div>

                        </>

                    )}

                    {/* BREWING */}

                    {category === "brewing" && (

                        <>

                            <div className="form-group">

                                <label>
                                    Nazwa metody
                                </label>

                                <input
                                    type="text"
                                    placeholder="Np. V60"
                                    value={title}
                                    onChange={(e) =>
                                        setTitle(e.target.value)
                                    }
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Typ metody
                                </label>

                                <div className="select-wrapper">

                                    <select>

                                        <option value="">
                                            Wybierz typ metody
                                        </option>

                                        <option value="Pour over">
                                            Pour over
                                        </option>

                                        <option value="Immersion">
                                            Immersion
                                        </option>

                                        <option value="Espresso">
                                            Espresso
                                        </option>

                                    </select>

                                    <ChevronDown size={18} />

                                </div>

                            </div>

                            <div className="form-group">

                                <label>
                                    Opis metody parzenia
                                </label>

                                <textarea
                                    placeholder="Dodaj opis metody..."
                                    value={content}
                                    onChange={(e) =>
                                        setContent(e.target.value)
                                    }
                                />

                            </div>

                        </>

                    )}

                    {/* ROASTERIES */}

                    {category === "roastery" && (

                        <>

                            <div className="form-group">

                                <label>
                                    Nazwa palarni
                                </label>

                                <input
                                    type="text"
                                    placeholder="Np. Coffee Collective"
                                    value={title}
                                    onChange={(e) =>
                                        setTitle(e.target.value)
                                    }
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Miasto
                                </label>

                                <input
                                    type="text"
                                    placeholder="Np. Kopenhaga"
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Kraj
                                </label>

                                <input
                                    type="text"
                                    placeholder="Np. Dania"
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Rok założenia
                                </label>

                                <input
                                    type="text"
                                    placeholder="Np. 2007"
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Styl palenia
                                </label>

                                <div className="select-wrapper">

                                    <select>

                                        <option value="">
                                            Wybierz styl palenia
                                        </option>

                                        <option value="Light Roast">
                                            Light Roast
                                        </option>

                                        <option value="Omni Roast">
                                            Omni Roast
                                        </option>

                                        <option value="Espresso Roast">
                                            Espresso Roast
                                        </option>

                                    </select>

                                    <ChevronDown size={18} />

                                </div>

                            </div>

                            <div className="form-group">

                                <label>
                                    Specjalizacja
                                </label>

                                <input
                                    type="text"
                                    placeholder="Np. Filter Coffee, Single Origin"
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Regiony sourcingu
                                </label>

                                <input
                                    type="text"
                                    placeholder="Np. Etiopia, Kenia, Kolumbia"
                                />

                            </div>

                            <div className="form-group">

                                <label>
                                    Opis palarni
                                </label>

                                <textarea
                                    placeholder="Dodaj opis palarni..."
                                    value={content}
                                    onChange={(e) =>
                                        setContent(e.target.value)
                                    }
                                />

                            </div>

                        </>

                    )}

                    {/* IMAGES + ACTIONS */}

                    {category && (

                        <>

                            <div className="form-group">

                                <label>
                                    Zdjęcia
                                </label>

                                <label className="upload-box">

                                    <input
                                        type="file"
                                        accept="image/*"
                                        multiple
                                        onChange={(e) =>
                                            setFiles(
                                                [...e.target.files]
                                            )
                                        }
                                    />

                                    <span>
                                        Przeciągnij zdjęcia lub kliknij,
                                        aby dodać fotografie
                                    </span>

                                </label>

                                {files.length > 0 && (

                                    <div className="uploaded-files">

                                        {files.map((file, index) => (

                                            <div
                                                key={index}
                                                className="uploaded-file"
                                            >
                                                {file.name}
                                            </div>

                                        ))}

                                    </div>

                                )}

                            </div>

                            <div className="article-actions">

                                <button
                                    className="submit-article-button"
                                >

                                    <Send size={16} />

                                    Wyślij do moderacji

                                </button>

                            </div>

                        </>

                    )}

                </div>

            </div>

        </div>

    );
}

export default AddWikiArticle;