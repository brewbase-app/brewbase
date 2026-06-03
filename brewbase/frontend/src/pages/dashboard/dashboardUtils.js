export function filterByPeriod(items, days) {
    if (!Array.isArray(items) || !days) {
        return items ?? [];
    }

    const cutoff = Date.now() - days * 24 * 60 * 60 * 1000;

    return items.filter((item) => {
        const dateValue = item.createdAt ?? item.sessionDate;

        if (!dateValue) {
            return true;
        }

        const parsed = new Date(dateValue);

        if (Number.isNaN(parsed.getTime())) {
            return true;
        }

        return parsed.getTime() >= cutoff;
    });
}

export function getMostUsedBrewingMethod(recipes) {
    const counts = new Map();

    recipes.forEach((recipe) => {
        const method = recipe.brewingMethod?.trim();

        if (!method) {
            return;
        }

        counts.set(method, (counts.get(method) ?? 0) + 1);
    });

    let topMethod = null;
    let topCount = 0;

    counts.forEach((count, method) => {
        if (count > topCount) {
            topMethod = method;
            topCount = count;
        }
    });

    if (!topMethod) {
        return { name: "—", share: 0 };
    }

    return {
        name: topMethod,
        share: Math.round((topCount / recipes.length) * 100),
    };
}

export function getAverageCuppingScore(sessionsDetails) {
    const scores = sessionsDetails
        .flatMap((session) => session.coffees ?? [])
        .map((coffee) => coffee.overallScore)
        .filter((score) => score != null && !Number.isNaN(Number(score)));

    if (scores.length === 0) {
        return null;
    }

    const sum = scores.reduce((total, score) => total + Number(score), 0);

    return (sum / scores.length).toFixed(1);
}

export function buildCoffeeSubtitle(coffee) {
    return [
        coffee.region,
        coffee.processingMethod,
        coffee.variety,
        coffee.roastery,
    ]
        .filter(Boolean)
        .join(", ");
}

export function isNotificationUnread(notification) {
    return notification.isRead !== true;
}

export function getDashboardGreeting({ profile, isLoading }) {
    if (isLoading) {
        return "Dzień dobry!";
    }

    const login = profile?.login?.trim();

    if (login) {
        return `Dzień dobry, ${login}!`;
    }

    return "Dzień dobry!";
}
