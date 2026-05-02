import { BrowserRouter, Routes, Route } from "react-router-dom";

import Sidebar from "./components/Sidebar";
import Home from "./pages/Home";
import Recipes from "./pages/Recipes";
import RecipesForm from "./pages/RecipesForm";
import RecipesList from "./pages/RecipesList";

function App() {
    return (
        <BrowserRouter>
            <div style={{ display: "flex" }}>
                <Sidebar />

                <div style={{ flex: 1 }}>
                    <Routes>
                        <Route path="/" element={<Home />} />
                        <Route path="/recipes" element={<Recipes />} />
                        <Route path="/recipes/new" element={<RecipesForm />} />
                        <Route path="/recipes/my"
                        element={<RecipesList title="Twoje receptury" />} />
                        <Route
                            path="/recipes/favorites"
                            element={<RecipesList title="Ulubione receptury" />}
                        />
                    </Routes>
                </div>
            </div>
        </BrowserRouter>
    );
}

export default App;