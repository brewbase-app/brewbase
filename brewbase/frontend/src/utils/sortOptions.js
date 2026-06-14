export function sortByName(items) {
    return [...items].sort((left, right) =>
        (left.name ?? "").localeCompare(
            right.name ?? "",
            "pl",
            { sensitivity: "base" }
        )
    );
}

export function sortStrings(items) {
    return [...items].sort((left, right) =>
        String(left ?? "").localeCompare(
            String(right ?? ""),
            "pl",
            { sensitivity: "base" }
        )
    );
}