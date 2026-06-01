import { getProfile } from "./profileApi";
import { setAuthToken, setUserRole } from "../utils/auth";

export async function establishAuthSession(token) {
    setAuthToken(token);

    try {
        const profile = await getProfile();
        setUserRole(profile.role);
    } catch {
        setUserRole(null);
    }
}
