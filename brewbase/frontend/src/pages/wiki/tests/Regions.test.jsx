import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";

import Regions from "../Regions";

const navigateMock = vi.fn();

vi.mock("../../../api/articlesApi", () => ({
    getArticles: vi.fn(),
}));

vi.mock("../../../api/countryApi", () => ({
    getCountries: vi.fn(),
}));

vi.mock("../../../api/flavorProfileApi", () => ({
    getFlavorProfiles: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useNavigate: () => navigateMock,
    };
});

import { getArticles } from "../../../api/articlesApi";
import { getCountries } from "../../../api/countryApi";
import { getFlavorProfiles } from "../../../api/flavorProfileApi";

const countryArticle = {
    id: 8,
    title: "Etiopia",
    content: "Region: Yirgacheffe\nProfil smakowy: kwiatowy, cytrusowy\n\nOpis kraju.",
    region: "Yirgacheffe",
    flavorProfiles: ["kwiatowy", "cytrusowy"],
};

describe("Regions", () => {
    beforeEach(() => {
        navigateMock.mockReset();
        vi.mocked(getArticles).mockReset();
        vi.mocked(getCountries).mockReset();
        vi.mocked(getFlavorProfiles).mockReset();
    });

    it("shows loading state", () => {
        vi.mocked(getArticles).mockImplementation(() => new Promise(() => {}));
        vi.mocked(getCountries).mockImplementation(() => new Promise(() => {}));
        vi.mocked(getFlavorProfiles).mockImplementation(() => new Promise(() => {}));

        render(
            <MemoryRouter>
                <Regions />
            </MemoryRouter>
        );

        expect(screen.getByText("Ładowanie krajów...")).toBeInTheDocument();
    });

    it("shows error when articles cannot be loaded", async () => {
        vi.mocked(getArticles).mockRejectedValue(new Error("fail"));

        render(
            <MemoryRouter>
                <Regions />
            </MemoryRouter>
        );

        expect(
            await screen.findByText("Nie udało się pobrać artykułów o krajach.")
        ).toBeInTheDocument();
    });

    it("shows empty state when there are no articles", async () => {
        vi.mocked(getArticles).mockResolvedValue([]);
        vi.mocked(getCountries).mockResolvedValue([]);
        vi.mocked(getFlavorProfiles).mockResolvedValue([]);

        render(
            <MemoryRouter>
                <Regions />
            </MemoryRouter>
        );

        expect(
            await screen.findByText("Brak opublikowanych artykułów o krajach.")
        ).toBeInTheDocument();
    });

    it("renders country articles and navigates on click", async () => {
        vi.mocked(getArticles).mockResolvedValue([countryArticle]);
        vi.mocked(getCountries).mockResolvedValue([{ id: 1, name: "Etiopia" }]);
        vi.mocked(getFlavorProfiles).mockResolvedValue([
            { id: 1, name: "kwiatowy" },
        ]);

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <Regions />
            </MemoryRouter>
        );

        await screen.findByRole("heading", { name: "Etiopia" });
        await user.click(screen.getByRole("heading", { name: "Etiopia" }));

        expect(navigateMock).toHaveBeenCalledWith("/wiki/articles/8");
    });

    it("filters countries by search query", async () => {
        vi.mocked(getArticles).mockResolvedValue([
            countryArticle,
            { ...countryArticle, id: 9, title: "Kenia", region: "Nyeri" },
        ]);
        vi.mocked(getCountries).mockResolvedValue([]);
        vi.mocked(getFlavorProfiles).mockResolvedValue([]);

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <Regions />
            </MemoryRouter>
        );

        await screen.findByRole("heading", { name: "Etiopia" });

        await user.type(
            screen.getByPlaceholderText("Szukaj krajów..."),
            "Kenia"
        );

        expect(screen.getByRole("heading", { name: "Kenia" })).toBeInTheDocument();
        expect(screen.queryByRole("heading", { name: "Etiopia" })).not.toBeInTheDocument();
    });
});
