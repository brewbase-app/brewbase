
import { useState } from "react";

import {
    BrowserRouter,
    Routes,
    Route,
    useLocation,
    Navigate
} from "react-router-dom";

import {
    ArrowLeft
} from "lucide-react";

import "./App.css";

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

import Quicknotes from "./pages/Quicknotes";

/* WIKI */

import WikiHome from "./pages/wiki/WikiHome";

import Coffees from "./pages/wiki/Coffees";
import CoffeeDetails from "./pages/wiki/CoffeeDetails";

import Regions from "./pages/wiki/Regions";
import RegionDetails from "./pages/wiki/RegionDetails";

import BrewingMethods from "./pages/wiki/BrewingMethods";
import BrewingMethodDetails from "./pages/wiki/BrewingMethodDetails";

import AddWikiArticle from "./pages/wiki/AddWikiArticle";

function Layout() {

    const location = useLocation();

    const token = localStorage.getItem("token");

    const [sidebarExpanded, setSidebarExpanded] =
        useState(false);

    const isAuthPage =
        location.pathname === "/login" ||
        location.pathname === "/register";

    const showBackButton =
        location.pathname !== "/home" &&
        location.pathname !== "/login" &&
        location.pathname !== "/register";

    return (

        <div style={{ display: "flex" }}>

            {!isAuthPage && token && (

                <Sidebar
                    sidebarExpanded={sidebarExpanded}
                    setSidebarExpanded={setSidebarExpanded}
                />

            )}

            <div
                style={{
                    flex: 1,

                    marginLeft:
                        sidebarExpanded
                            ? "220px"
                            : "72px",

                    transition: "0.25s ease",

                    position: "relative",

                    
                }}
            >

                {showBackButton && (

                    <button
                        className="global-back-button"
                        onClick={() => window.history.back()}
                    >

                        <ArrowLeft size={18} />

                        Wróć

                    </button>

                )}

                <Routes>

                    {/* START */}

                    <Route
                        path="/"
                        element={<Navigate to="/login" />}
                    />

                    {/* AUTH */}

                    <Route
                        path="/login"
                        element={<Login />}
                    />

                    <Route
                        path="/register"
                        element={<RegisterPage />}
                    />

                    {/* HOME */}

                    <Route
                        path="/home"
                        element={<Home />}
                    />

                    {/* RECIPES */}

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

                    {/* CUPPING */}

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

                    {/* QUICK NOTES */}

                    <Route
                        path="/quicknotes"
                        element={<Quicknotes />}
                    />

                    {/* WIKI */}

                    <Route
                        path="/wiki"
                        element={<WikiHome />}
                    />

                    {/* COFFEES */}

                    <Route
                        path="/wiki/coffees"
                        element={<Coffees />}
                    />

                    <Route
                        path="/wiki/coffees/:id"
                        element={<CoffeeDetails />}
                    />

                    {/* REGIONS */}

                    <Route
                        path="/wiki/regions"
                        element={<Regions />}
                    />

                    <Route
                        path="/wiki/regions/:id"
                        element={<RegionDetails />}
                    />

                    {/* BREWING METHODS */}

                    <Route
                        path="/wiki/methods"
                        element={<BrewingMethods />}
                    />

                    <Route
                        path="/wiki/methods/:id"
                        element={<BrewingMethodDetails />}
                    />

                    {/* ADD ARTICLE */}

                    <Route
                        path="/wiki/add"
                        element={<AddWikiArticle />}
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
