import { apiRequest } from "./apiClient";


// USERS


export function getUsers() {

    return apiRequest(
        "/api/admin/users"
    );
}

export function updateUserRole(
    userId,
    role
) {

    return apiRequest(
        `/api/admin/users/${userId}/role`,
        {
            method: "PATCH",

            body: JSON.stringify({
                role,
            }),
        }
    );
}

export function blockUser(
    userId
) {

    return apiRequest(
        `/api/admin/block-user/${userId}`,
        {
            method: "PATCH",
        }
    );
}

export function unblockUser(
    userId
) {

    return apiRequest(
        `/api/admin/unblock-user/${userId}`,
        {
            method: "PATCH",
        }
    );
}


// ARTICLES MODERATION


export function getPendingArticles() {

    return apiRequest(
        "/api/admin/articles/pending"
    );
}

export function approveArticle(
    articleId
) {

    return apiRequest(
        `/api/admin/articles/${articleId}/approve`,
        {
            method: "PATCH",
        }
    );
}

export function rejectArticle(
    articleId,
    reason = ""
) {

    return apiRequest(
        `/api/admin/articles/${articleId}/reject`,
        {
            method: "PATCH",

            body: JSON.stringify({
                reason,
            }),
        }
    );
}


// REPORTS


export function getReports() {

    return apiRequest(
        "/api/admin/reports"
    );
}