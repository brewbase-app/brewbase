export function formatCoffeeSelectLabel(coffee) {
    if (!coffee?.name) {
        return `Kawa #${coffee?.id ?? "?"}`;
    }

    const details = [coffee.roastery, coffee.region].filter(Boolean);

    if (details.length === 0) {
        return coffee.name;
    }

    return `${coffee.name} — ${details.join(", ")}`;
}

export function formatBrewingMethodSelectLabel(method) {
    if (!method?.name) {
        return `Metoda #${method?.id ?? "?"}`;
    }

    return method.name;
}

export function getCoffeeSelectPlaceholder({ loading, error, isEmpty }) {
    if (loading) {
        return "Ładowanie kaw z katalogu...";
    }

    if (error) {
        return "Nie udało się załadować kaw";
    }

    if (isEmpty) {
        return "Brak kaw w katalogu — dodaj artykuł wiki";
    }

    return "Wybierz kawę z katalogu (opcjonalnie w wersji roboczej)";
}

export function getBrewingMethodSelectPlaceholder({ loading, error, isEmpty }) {
    if (loading) {
        return "Ładowanie metod parzenia...";
    }

    if (error) {
        return "Nie udało się załadować metod";
    }

    if (isEmpty) {
        return "Brak metod parzenia w katalogu — dodaj artykuł wiki";
    }

    return "Wybierz metodę parzenia (opcjonalnie w wersji roboczej)";
}
