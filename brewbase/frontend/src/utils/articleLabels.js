export const ARTICLE_MODULE_LABELS = {
    coffee: "Kawy",
    country: "Kraje",
    region: "Kraje",
    brewing_method: "Metody parzenia",
    roastery: "Palarnie",
    general: "Ogólne",
};

export const ARTICLE_STATUS_LABELS = {
    Pending: "W moderacji",
    Approved: "Opublikowany",
    Rejected: "Odrzucony",
};

export function getArticleModuleLabel(module) {
    return ARTICLE_MODULE_LABELS[module] ?? module;
}

export function getArticleStatusLabel(status) {
    return ARTICLE_STATUS_LABELS[status] ?? status;
}

export function canDeleteArticle(status) {
    return status !== "Approved";
}
