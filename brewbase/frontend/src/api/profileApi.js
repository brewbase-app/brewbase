import { apiRequest } from "./apiClient";

export function getProfile() {

    return apiRequest(
        "/api/users/profile_info"
    );
}

export function updateProfile(profileData) {

    return apiRequest(
        "/api/users/edit_profile",
        {
            method: "PUT",

            body: JSON.stringify(profileData),
        }
    );
}