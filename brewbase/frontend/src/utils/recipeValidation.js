const TITLE_MIN_LENGTH = 3;
const TITLE_MAX_LENGTH = 120;
const STEPS_MIN_LENGTH = 5;
const MIN_BREW_TIME_SECONDS = 60;
const MAX_BREW_TIME_SECONDS = 540;
const MIN_TEMPERATURE = 70;
const MAX_TEMPERATURE = 100;
const MAX_COFFEE_GRAMS = 1000;
const MAX_WATER_ML = 5000;

const FIELD_LABELS = {
    title: "Nazwa receptury",
    description: "Opis przygotowania",
    coffeeId: "Kawa",
    brewingMethodId: "Metoda parzenia",
    coffee: "Ilość kawy",
    water: "Ilość wody",
    temperature: "Temperatura wody",
    minutes: "Minuty",
    seconds: "Sekundy",
    brewTime: "Czas parzenia",
    form: "Formularz",
};

function parseNumber(value) {
    if (value === null || value === undefined) {
        return null;
    }

    const normalized = String(value).replace(",", ".").trim();
    if (!normalized) {
        return null;
    }

    const parsed = Number(normalized);
    return Number.isFinite(parsed) ? parsed : null;
}

function hasMeaningfulContent(formData) {
    return Boolean(
        formData.title?.trim()
        || formData.description?.trim()
        || formData.coffeeId
        || formData.brewingMethodId
        || formData.coffee?.trim()
        || formData.water?.trim()
        || formData.temperature?.trim()
        || formData.grindSize?.trim()
        || formData.minutes?.trim()
        || formData.seconds?.trim()
    );
}

function validateTitle(title, errors, required) {
    const trimmed = title?.trim() ?? "";

    if (required && !trimmed) {
        errors.title = "Nazwa receptury jest wymagana.";
        return;
    }

    if (!trimmed) {
        return;
    }

    if (required && trimmed.length < TITLE_MIN_LENGTH) {
        errors.title = `Tytuł musi mieć minimum ${TITLE_MIN_LENGTH} znaki.`;
    }

    if (trimmed.length > TITLE_MAX_LENGTH) {
        errors.title = `Tytuł może mieć maksymalnie ${TITLE_MAX_LENGTH} znaków.`;
    }
}

function validateSteps(description, errors, required) {
    const trimmed = description?.trim() ?? "";

    if (required && !trimmed) {
        errors.description = "Opis przygotowania jest wymagany.";
        return;
    }

    if (!trimmed) {
        return;
    }

    if (required && trimmed.length < STEPS_MIN_LENGTH) {
        errors.description = `Opis przygotowania musi mieć minimum ${STEPS_MIN_LENGTH} znaków.`;
    }
}

function getBrewTimeSeconds(formData) {
    const hasBrewTimeInput = Boolean(formData.minutes?.trim() || formData.seconds?.trim());
    if (!hasBrewTimeInput) {
        return null;
    }

    const minutes = parseNumber(formData.minutes) ?? 0;
    const seconds = parseNumber(formData.seconds) ?? 0;
    return (minutes * 60) + seconds;
}

function validateBrewTime(formData, errors, required) {
    const hasMinutesInput = Boolean(formData.minutes?.trim());
    const hasSecondsInput = Boolean(formData.seconds?.trim());

    if (!hasMinutesInput && !hasSecondsInput) {
        if (required) {
            errors.brewTime = "Czas parzenia jest wymagany.";
        }
        return;
    }

    if (hasMinutesInput) {
        const minutes = parseNumber(formData.minutes);
        if (minutes === null || minutes < 0 || !Number.isInteger(minutes)) {
            errors.minutes = "Podaj poprawną liczbę minut.";
        }
    }

    if (hasSecondsInput) {
        const seconds = parseNumber(formData.seconds);
        if (seconds === null || seconds < 0 || seconds > 59 || !Number.isInteger(seconds)) {
            errors.seconds = "Sekundy muszą być między 0 a 59.";
        }
    }

    if (errors.minutes || errors.seconds) {
        return;
    }

    const brewTimeSeconds = getBrewTimeSeconds(formData);
    if (brewTimeSeconds === null) {
        return;
    }

    if (brewTimeSeconds < MIN_BREW_TIME_SECONDS || brewTimeSeconds > MAX_BREW_TIME_SECONDS) {
        errors.brewTime = "Czas parzenia musi być między 1:00 a 9:00.";
    }
}

function validateOptionalParameterRanges(formData, errors) {
    const coffee = parseNumber(formData.coffee);
    const water = parseNumber(formData.water);
    const temperature = parseNumber(formData.temperature);

    if (coffee !== null) {
        if (coffee <= 0) {
            errors.coffee = "Ilość kawy musi być większa od zera.";
        } else if (coffee > MAX_COFFEE_GRAMS) {
            errors.coffee = `Ilość kawy nie może przekraczać ${MAX_COFFEE_GRAMS} g.`;
        }
    }

    if (water !== null) {
        if (water <= 0) {
            errors.water = "Ilość wody musi być większa od zera.";
        } else if (water > MAX_WATER_ML) {
            errors.water = `Ilość wody nie może przekraczać ${MAX_WATER_ML} ml.`;
        }
    }

    if (temperature !== null) {
        if (temperature < MIN_TEMPERATURE || temperature > MAX_TEMPERATURE) {
            errors.temperature = `Temperatura musi być między ${MIN_TEMPERATURE} a ${MAX_TEMPERATURE}°C.`;
        }
    }

    validateBrewTime(formData, errors, false);
}

function validatePublishRequiredFields(formData, errors) {
    if (!formData.coffeeId) {
        errors.coffeeId = "Wybór kawy jest wymagany.";
    }

    if (!formData.brewingMethodId) {
        errors.brewingMethodId = "Metoda parzenia jest wymagana.";
    }

    if (parseNumber(formData.coffee) === null) {
        errors.coffee = "Ilość kawy jest wymagana.";
    }

    if (parseNumber(formData.water) === null) {
        errors.water = "Ilość wody jest wymagana.";
    }

    if (parseNumber(formData.temperature) === null) {
        errors.temperature = "Temperatura wody jest wymagana.";
    }

    validateBrewTime(formData, errors, true);
}

export function buildRecipeParameters(formData) {
    const parameters = {};

    if (formData.coffee?.trim()) {
        parameters.coffee = `${formData.coffee.trim()}g`;
    }

    if (formData.water?.trim()) {
        parameters.water = `${formData.water.trim()}ml`;
    }

    if (formData.temperature?.trim()) {
        parameters.temperature = `${formData.temperature.trim()}°C`;
    }

    if (formData.grindSize?.trim()) {
        parameters.grindSize = formData.grindSize.trim();
    }

    if (formData.minutes?.trim() || formData.seconds?.trim()) {
        parameters.brewTime = `${formData.minutes?.trim() || "0"}:${formData.seconds?.trim() || "0"}`;
    }

    return parameters;
}

export function validateRecipeDraft(formData) {
    const errors = {};

    if (!hasMeaningfulContent(formData)) {
        errors.form = "Uzupełnij przynajmniej jedno pole, aby zapisać wersję roboczą.";
    }

    validateTitle(formData.title, errors, false);
    validateSteps(formData.description, errors, false);
    validateOptionalParameterRanges(formData, errors);

    return errors;
}

export function validateRecipePublish(formData) {
    const errors = {};

    validateTitle(formData.title, errors, true);
    validateSteps(formData.description, errors, true);
    validatePublishRequiredFields(formData, errors);
    validateOptionalParameterRanges(formData, errors);

    return errors;
}

export function mapBackendErrors(apiErrors) {
    const mapped = {};

    Object.entries(apiErrors).forEach(([field, messages]) => {
        const message = Array.isArray(messages) ? messages[0] : messages;

        switch (field) {
            case "Title":
                mapped.title = translateBackendMessage(message, "Nazwa receptury jest wymagana.");
                break;
            case "Steps":
                mapped.description = translateBackendMessage(message, "Opis przygotowania jest wymagany.");
                break;
            case "BrewingMethodId":
                mapped.brewingMethodId = "Metoda parzenia jest wymagana.";
                break;
            case "CoffeeId":
                mapped.coffeeId = "Wybór kawy jest wymagany.";
                break;
            case "Parameters.Coffee":
                mapped.coffee = translateBackendMessage(message, "Ilość kawy jest wymagana.");
                break;
            case "Parameters.Water":
                mapped.water = translateBackendMessage(message, "Ilość wody jest wymagana.");
                break;
            case "Parameters.Temperature":
                mapped.temperature = translateBackendMessage(message, "Temperatura wody jest wymagana.");
                break;
            case "Parameters.BrewTime":
                mapped.brewTime = translateBackendMessage(
                    message,
                    "Czas parzenia musi być między 1:00 a 9:00."
                );
                break;
            case "":
                mapped.form = translateBackendMessage(
                    message,
                    "Uzupełnij przynajmniej jedno pole, aby zapisać wersję roboczą."
                );
                break;
            default:
                mapped.form = message;
                break;
        }
    });

    return mapped;
}

function translateBackendMessage(message, fallback) {
    if (!message) {
        return fallback;
    }

    if (message.includes("Temperature must be between")) {
        return `Temperatura musi być między ${MIN_TEMPERATURE} a ${MAX_TEMPERATURE}°C.`;
    }

    if (message.includes("Title must be at least")) {
        return `Tytuł musi mieć minimum ${TITLE_MIN_LENGTH} znaki.`;
    }

    if (message.includes("Steps are required")) {
        return "Opis przygotowania jest wymagany.";
    }

    if (message.includes("Steps must be at least")) {
        return `Opis przygotowania musi mieć minimum ${STEPS_MIN_LENGTH} znaków.`;
    }

    if (message.includes("Brew time must be greater than 0")) {
        return "Czas parzenia jest wymagany.";
    }

    if (message.includes("Brew time must be between")) {
        return "Czas parzenia musi być między 1:00 a 9:00.";
    }

    if (message.includes("at least one field")) {
        return "Uzupełnij przynajmniej jedno pole, aby zapisać wersję roboczą.";
    }

    return fallback;
}

export function getFieldLabel(fieldName) {
    return FIELD_LABELS[fieldName] ?? fieldName;
}

export function hasValidationErrors(errors) {
    return Object.keys(errors).length > 0;
}
