export const getFavoriteCoffees = () => {

    return JSON.parse(
        localStorage.getItem("favoriteCoffees") || "[]"
    );
};

export const toggleFavoriteCoffee = (coffeeId) => {

    const favorites = getFavoriteCoffees();

    const exists = favorites.includes(coffeeId);

    let updated;

    if (exists) {

        updated = favorites.filter(
            (id) => id !== coffeeId
        );

    } else {

        updated = [...favorites, coffeeId];
    }

    localStorage.setItem(
        "favoriteCoffees",
        JSON.stringify(updated)
    );

    return updated;
};