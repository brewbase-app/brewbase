function readLineValue(content, prefix) {
    const line = content
        .split("\n")
        .find((entry) => entry.startsWith(prefix));

    return line ? line.slice(prefix.length).trim() : null;
}

function parseFlavorProfiles(content) {
    const rawValue = readLineValue(content, "Profil smakowy: ");

    if (!rawValue) {
        return [];
    }

    return rawValue
        .split(",")
        .map((value) => value.trim())
        .filter(Boolean);
}

export function parseCoffeeArticleMetadata(content) {
    if (!content) {
        return {
            beanOriginCountry: null,
            variety: null,
            processingMethod: null,
            roastery: null,
            flavorProfiles: [],
        };
    }

    return {
        beanOriginCountry: readLineValue(
            content,
            "Kraj pochodzenia ziaren: "
        ),
        variety: readLineValue(content, "Odmiana: "),
        processingMethod: readLineValue(content, "Obróbka ziaren: "),
        roastery: readLineValue(content, "Palarnia: "),
        flavorProfiles: parseFlavorProfiles(content),
    };
}

function parseCommaSeparatedList(rawValue) {
    if (!rawValue) {
        return [];
    }

    return rawValue
        .split(",")
        .map((value) => value.trim())
        .filter(Boolean);
}

export function parseRoasteryArticleMetadata(content) {
    if (!content) {
        return {
            roastingStyles: [],
            description: "",
        };
    }

    const roastingStyles = parseCommaSeparatedList(
        readLineValue(content, "Styl palenia: ")
    );

    const metadataPrefix = "Styl palenia: ";
    const metadataLine = content
        .split("\n")
        .find((entry) => entry.startsWith(metadataPrefix));

    let description = content.trim();

    if (metadataLine) {
        const metadataIndex = content.indexOf(metadataLine);
        const afterMetadata = content.slice(
            metadataIndex + metadataLine.length
        );
        description = afterMetadata.replace(/^\s*\n+/, "").trim();
    }

    return {
        roastingStyles,
        description,
    };
}

export function parseCountryArticleMetadata(content) {
    if (!content) {
        return {
            region: null,
            flavorProfiles: [],
        };
    }

    return {
        region: readLineValue(content, "Region: "),
        flavorProfiles: parseFlavorProfiles(content),
    };
}

export function buildFilterOptions(staticOptions, ...valueLists) {
    return [
        ...new Set([
            ...staticOptions,
            ...valueLists.flat().filter(Boolean),
        ]),
    ].sort((left, right) =>
        left.localeCompare(right, "pl")
    );
}
