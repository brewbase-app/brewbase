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
import CuppingList from "./pages/cupping/CuppingList";
import CreateCupping from "./pages/cupping/CreateCupping";
import CuppingDetails from "./pages/cupping/CuppingDetails";
import RecipeDetails from "./pages/RecipeDetails";
import ProtectedRoute from "./components/Auth/ProtectedRoute.jsx";

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
                    <Route path="/" element={<Navigate to="/login" />} />

                    {/* auth */}
                    <Route path="/login" element={<Login />} />
                    <Route path="/register" element={<RegisterPage />} />

                    {/* app */}
                    <Route path="/home" element={<ProtectedRoute>
                        <Home />
                    </ProtectedRoute>} />
                    <Route path="/recipes" element={ <ProtectedRoute>
                        <Recipes />
                    </ProtectedRoute>} />
                    <Route path="/recipes/new" element={<ProtectedRoute>
                        <RecipesForm />
                    </ProtectedRoute>} />
                    <Route
                        path="/recipes/my"
                        element={<ProtectedRoute>
                            <RecipesList title="Twoje receptury" />
                        </ProtectedRoute>}
                    />
                    <Route
                        path="/recipes/favorites"
                        element={<ProtectedRoute>
                            <RecipesList title="Ulubione receptury" />
                        </ProtectedRoute>}
                        
                    />
                    <Route
                        path="/recipes/:id"
                        element={<ProtectedRoute>
                            <RecipeDetails />
                        </ProtectedRoute>}
                    />
                    <Route path="/cupping" element={<ProtectedRoute>
                        <CuppingList />
                    </ProtectedRoute>} />
                    <Route path="/cupping/new" element={ <ProtectedRoute>
                        <CreateCupping />
                    </ProtectedRoute>} />
                    <Route path="/cupping/:id" element={<ProtectedRoute>
                        <CuppingDetails />
                    </ProtectedRoute>} />
                    
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