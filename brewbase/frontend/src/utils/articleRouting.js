export function getApprovedArticlePublicPath(article) {
    if (
        article.module === "coffee"
        && article.status === "Approved"
        && article.coffeeId
    ) {
        return `/wiki/coffees/${article.coffeeId}`;
    }

    return `/wiki/articles/${article.id}`;
}

export function shouldUseCoffeeDetailRoute(article) {
    return (
        article.module === "coffee"
        && article.status === "Approved"
        && article.coffeeId
    );
}
