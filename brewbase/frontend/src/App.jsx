import { useEffect, useState } from "react";

import {
    BrowserRouter,
    Routes,
    Route,
    useLocation,
    useNavigate,
    Navigate
} from "react-router-dom";

import { ArrowLeft } from "lucide-react";

import "./App.css";

import { getAuthToken, getUserRole, setUserRole } from "./utils/auth";
import Sidebar from "./components/Sidebar";
import ProtectedRoute from "./components/Auth/ProtectedRoute";
import AdminRoute from "./components/Auth/AdminRoute";

import { getProfile } from "./api/profileApi";

/* HOME */

import Dashboard from "./pages/Dashboard";
import FavoriteCoffees from "./pages/FavoriteCoffees";

/* RECIPES */

import Recipes from "./pages/Recipes";
import RecipesForm from "./pages/RecipesForm";
import RecipesList from "./pages/RecipesList";
import RecipeDetails from "./pages/RecipeDetails";

/* AUTH */

import Login from "./pages/Login";
import RegisterPage from "./pages/RegisterPage";
import PreferencesOnboardingPage from "./pages/PreferencesOnboardingPage";

/* CUPPING */

import CuppingList from "./pages/cupping/CuppingList";
import CreateCupping from "./pages/cupping/CreateCupping";
import CuppingDetails from "./pages/cupping/CuppingDetails";
import CuppingPreview from "./pages/cupping/CuppingPreview";

/* QUICK NOTES */

import Quicknotes from "./pages/Quicknotes";

/* RANKING */

import Ranking from "./pages/Ranking";

/* PROFILE */

import ProfilePage from "./pages/ProfilePage";
import EditProfilePage from "./pages/EditProfilePage";

/* WIKI */

import WikiHome from "./pages/wiki/WikiHome";

import Coffees from "./pages/wiki/Coffees";
import CoffeeDetails from "./pages/wiki/CoffeeDetails";

import Regions from "./pages/wiki/Regions";

import BrewingMethods from "./pages/wiki/BrewingMethods";

import AddWikiArticle from "./pages/wiki/AddWikiArticle";
import Roasteries from "./pages/wiki/Roasteries";
import RoasteryDetails from "./pages/wiki/RoasteryDetails";
import WikiArticleDetails from "./pages/wiki/WikiArticleDetails";
import MyWikiArticles from "./pages/wiki/MyWikiArticles";
import MyWikiArticleDetails from "./pages/wiki/MyWikiArticleDetails";

/* REPORT */

import ReportPage from "./pages/ReportPage";

/* ADMIN */

import AdminModeration from "./pages/AdminModeration";

function Layout() {

    const location = useLocation();
    const navigate = useNavigate();

    const token = getAuthToken();

    const [sidebarExpanded, setSidebarExpanded] =
        useState(false);

    const [userRole, setUserRoleState] = useState(getUserRole());

    useEffect(() => {
        if (!token) {
            setUserRoleState(null);
            return;
        }

        if (getUserRole()) {
            setUserRoleState(getUserRole());
            return;
        }

        getProfile()
            .then((profile) => {
                setUserRole(profile.role);
                setUserRoleState(profile.role);
            })
            .catch(() => {
                setUserRole(null);
                setUserRoleState(null);
            });
    }, [token]);

    
const isAuthPage =
    location.pathname === "/login" ||
    location.pathname === "/register" ||
    location.pathname === "/onboarding";




    const showBackButton =
        location.pathname !== "/home" &&
        location.pathname !== "/login" &&
        location.pathname !== "/register" &&
        location.pathname !== "/onboarding";

    const handleBackClick = () => {
        if (location.pathname.startsWith("/cupping/preview/")) {
            navigate("/cupping/new");
            return;
        }

        window.history.back();
    };

    return (

        <div style={{ display: "flex" }}>

            {!isAuthPage && token && (

                <Sidebar
                    sidebarExpanded={sidebarExpanded}
                    setSidebarExpanded={setSidebarExpanded}
                    showAdmin={userRole === "Admin"}
                />

            )}

            <div
                style={{
                    flex: 1,

                    marginLeft:
                        !isAuthPage
                            ? (
                                sidebarExpanded
                                    ? "300px"
                                    : "100px"
                            )
                            : "0px",

                    transition:
                        "margin-left 0.25s ease",

                    position: "relative",

                    minHeight: "100vh",

                    background: "#f8f6f3"
                }}
            >

                {showBackButton && (

                    <button
                        className="global-back-button"
                        onClick={handleBackClick}
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
                    <Route path="/onboarding" element={<PreferencesOnboardingPage />} />

                    {/* PROTECTED ROUTES */}

                    <Route element={<ProtectedRoute />}>

                        {/* HOME */}

                        <Route
                            path="/home"
                            element={<Dashboard />}
                        />
                        <Route
                            path="/favorite-coffees"
                            element={<FavoriteCoffees />}
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
                            path="/recipes/all"
                            element={
                                <RecipesList
                                    title="Wszystkie receptury"
                                />
                            }
                        />

                        <Route
                            path="/recipes/:id"
                            element={<RecipeDetails />}
                        />
                        <Route
                            path="/recipes/edit/:id"
                            element={<RecipesForm />}
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

                        {/* RANKING */}

                        <Route
                            path="/ranking"
                            element={<Ranking />}
                        />

                        {/* PROFILE */}

                        <Route
                            path="/profile"
                            element={<ProfilePage />}
                        />

                        <Route
                            path="/profile/:username"
                            element={<ProfilePage />}
                        />

                        <Route
                            path="/profile/edit"
                            element={<EditProfilePage />}
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

                        {/* BREWING METHODS */}

                        <Route
                            path="/wiki/methods"
                            element={<BrewingMethods />}
                        />

                        {/* ADD ARTICLE */}

                        <Route
                            path="/wiki/add"
                            element={<AddWikiArticle />}
                        />

                        <Route
                            path="/wiki/my-articles"
                            element={<MyWikiArticles />}
                        />

                        <Route
                            path="/wiki/my-articles/:id"
                            element={<MyWikiArticleDetails />}
                        />

                        <Route
                            path="/wiki/articles/:id"
                            element={<WikiArticleDetails />}
                        />

                        <Route
                            path="/wiki/roasteries"
                            element={<Roasteries />}
                        />

                        <Route
                            path="/wiki/roasteries/:id"
                            element={<RoasteryDetails />}
                        />
                        <Route
                            path="/report"
                            element={<ReportPage />}
                        />

                        {/* ADMIN */}

                        <Route element={<AdminRoute />}>
                            <Route
                                path="/admin"
                                element={<AdminModeration />}
                            />
                        </Route>

                    </Route>

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