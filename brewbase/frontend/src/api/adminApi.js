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
    comment = ""
) {

    return apiRequest(
        `/api/admin/articles/${articleId}/reject`,
        {
            method: "PATCH",

            body: JSON.stringify({
                comment,
            }),
        }
    );
}


// REPORTS


export function getReports(scope = "open") {

    const query = scope ? `?scope=${encodeURIComponent(scope)}` : "";

    return apiRequest(
        `/api/admin/reports${query}`
    );
}

export function dismissReport(reportId) {

    return apiRequest(
        `/api/admin/reports/${reportId}/dismiss`,
        {
            method: "PATCH",
        }
    );
}

export function upholdReport(reportId, comment = "") {

    return apiRequest(
        `/api/admin/reports/${reportId}/uphold`,
        {
            method: "PATCH",

            body: JSON.stringify({
                comment,
            }),
        }
    );
}
