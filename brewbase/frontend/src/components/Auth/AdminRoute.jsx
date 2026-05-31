import { useEffect, useState } from "react";
import { Navigate, Outlet } from "react-router-dom";

import { getProfile } from "../../api/profileApi";
import {
    getAuthToken,
    getUserRole,
    isAdmin,
    setUserRole,
} from "../../utils/auth";

function AdminRoute() {
    const [access, setAccess] = useState(null);

    useEffect(() => {
        if (!getAuthToken()) {
            setAccess(false);
            return;
        }

        if (getUserRole()) {
            setAccess(isAdmin());
            return;
        }

        getProfile()
            .then((profile) => {
                setUserRole(profile.role);
                setAccess(profile.role === "Admin");
            })
            .catch(() => {
                setUserRole(null);
                setAccess(false);
            });
    }, []);

    if (access === null) {
        return null;
    }

    if (!access) {
        return <Navigate to="/home" replace />;
    }

    return <Outlet />;
}

export default AdminRoute;
