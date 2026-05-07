import {
    BrowserRouter,
    Routes,
    Route,
    useLocation,
    Navigate
} from "react-router-dom";

import Sidebar from "./components/Sidebar";
import Home from "./pages/Home";
import Recipes from "./pages/Recipes";
import RecipesForm from "./pages/RecipesForm";
import RecipesList from "./pages/RecipesList";
import Login from "./pages/Login";
import RegisterPage from "./pages/RegisterPage";

function Layout() {
    const location = useLocation();

    const isAuthPage =
        location.pathname === "/login" ||
        location.pathname === "/register";

    return (
        <div style={{ display: "flex" }}>
            {!isAuthPage && <Sidebar />}

            <div style={{ flex: 1 }}>
                <Routes>
                    {/* start */}
                    <Route path="/" element={<Navigate to="/login" />} />

                    {/* auth */}
                    <Route path="/login" element={<Login />} />
                    <Route path="/register" element={<RegisterPage />} />

                    {/* app */}
                    <Route path="/home" element={<Home />} />
                    <Route path="/recipes" element={<Recipes />} />
                    <Route path="/recipes/new" element={<RecipesForm />} />
                    <Route
                        path="/recipes/my"
                        element={<RecipesList title="Twoje receptury" />}
                    />
                    <Route
                        path="/recipes/favorites"
                        element={<RecipesList title="Ulubione receptury" />}
                    />
                </Routes>
            </div>
        </div>
    );
}

function App() {
    return (
        <BrowserRouter>
            <Layout />
        </BrowserRouter>
    );
}

export default App;