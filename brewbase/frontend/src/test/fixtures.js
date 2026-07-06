export const sampleRecipe = {
    id: 1,
    title: "Poranna V60",
    userId: 2,
    isPublic: true,
    parameters: JSON.stringify({
        coffee: "18g",
        water: "300ml",
        temperature: "92°C",
        brewTime: "3:00",
        grindSize: "średnio drobne",
    }),
    steps: "Zalej kawę wodą.",
    brewingMethod: "V60",
    coffee: "Etiopia Yirgacheffe",
    coffeeId: 10,
    averageRating: null,
    ratingCount: 0,
    isFavorite: false,
};

export const sampleCoffee = {
    id: 10,
    name: "Etiopia Yirgacheffe",
    createdByUserId: 2,
    region: "Etiopia",
    roastery: "Test Roastery",
    averageRating: 4.2,
    ratingCount: 3,
    isFavorite: false,
};

export const sampleProfile = {
    userId: 1,
    login: "maria",
    role: "User",
};
