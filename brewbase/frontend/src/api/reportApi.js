import { apiRequest } from "./apiClient";

export function submitReport({
    contentType,
    contentId,
    contentTitle,
    category,
    comment,
}) {
    return apiRequest("/api/reports", {
        method: "POST",
        body: JSON.stringify({
            contentType,
            contentId,
            contentTitle,
            category,
            comment: comment?.trim() || null,
        }),
    });
}

export function submitArticleReport(articleId, { category, comment }) {
    return submitReport({
        contentType: "article",
        contentId: articleId,
        contentTitle: null,
        category,
        comment,
    });
}
