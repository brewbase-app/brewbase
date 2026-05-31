import {
    Navigate,
    Outlet
} from "react-router-dom";

import { getAuthToken } from "../../utils/auth";

function ProtectedRoute() {

    const token = getAuthToken();

    if (!token) {
        return <Navigate to="/login" />;
    }

    return <Outlet />;
}

export default ProtectedRoute;