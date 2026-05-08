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
import RecipeDetails from "./pages/RecipeDetails";

import Login from "./pages/Login";
import RegisterPage from "./pages/RegisterPage";

import CuppingList from "./pages/cupping/CuppingList";
import CreateCupping from "./pages/cupping/CreateCupping";
import CuppingDetails from "./pages/cupping/CuppingDetails";
import CuppingPreview from "./pages/cupping/CuppingPreview";

function Layout() {
    const location = useLocation();

    const token = localStorage.getItem("token");

    const isAuthPage =
        location.pathname === "/login" ||
        location.pathname === "/register";

    return (
        <div style={{ display: "flex" }}>
            {!isAuthPage && token && <Sidebar />}

            <div style={{ flex: 1 }}>
                <Routes>
                    {/* start */}
                    <Route
                        path="/"
                        element={<Navigate to="/login" />}
                    />

                    {/* auth */}
                    <Route
                        path="/login"
                        element={<Login />}
                    />

                    <Route
                        path="/register"
                        element={<RegisterPage />}
                    />

                    {/* home */}
                    <Route
                        path="/home"
                        element={<Home />}
                    />

                    {/* recipes */}
                    <Route
                        path="/recipes"
                        element={<Recipes />}
                    />

                    <Route
                        path="/recipes/new"
                        element={<RecipesForm />}
                    />

                    <Route
                        path="/recipes/my"
                        element={
                            <RecipesList
                                title="Twoje receptury"
                            />
                        }
                    />

                    <Route
                        path="/recipes/favorites"
                        element={
                            <RecipesList
                                title="Ulubione receptury"
                            />
                        }
                    />

                    <Route
                        path="/recipes/:id"
                        element={<RecipeDetails />}
                    />

                    {/* cupping */}
                    <Route
                        path="/cupping"
                        element={<CuppingList />}
                    />

                    <Route
                        path="/cupping/new"
                        element={<CreateCupping />}
                    />

                    <Route
                        path="/cupping/:id"
                        element={<CuppingDetails />}
                    />

                    <Route
                        path="/cupping/preview/:id"
                        element={<CuppingPreview />}
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