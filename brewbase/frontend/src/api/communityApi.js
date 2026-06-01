import { apiRequest } from "./apiClient";

export function getUserProfile(userId) {
    return apiRequest(`/api/community/profile/${userId}`);
}

export function getUserProfileByLogin(login) {
    return apiRequest(
        `/api/community/profile/by-login/${encodeURIComponent(login)}`
    );
}

export function getFollowers(userId) {
    return apiRequest(`/api/community/followers/${userId}`);
}

export function getFollowing(userId) {
    void userId;
    return apiRequest("/api/community/following");
}

export function getCommunityFeed() {
    return apiRequest("/api/community/feed");
}

export function followUser(userId) {
    return apiRequest(`/api/community/follow/${userId}`, {
        method: "POST",
    });
}

export function unfollowUser(userId) {
    return apiRequest(`/api/community/unfollow/${userId}`, {
        method: "DELETE",
    });
}
