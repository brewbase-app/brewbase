import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";

import Recipes from "../Recipes";

const navigateMock = vi.fn();

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useNavigate: () => navigateMock,
    };
});

describe("Recipes", () => {
    beforeEach(() => {
        navigateMock.mockReset();
    });

    it("renders recipe hub header and cards", () => {
        render(
            <MemoryRouter>
                <Recipes />
            </MemoryRouter>
        );

        expect(screen.getByText("Receptury")).toBeInTheDocument();
        expect(screen.getByText("Wszystkie receptury")).toBeInTheDocument();
        expect(screen.getByText("Nowa receptura")).toBeInTheDocument();
        expect(screen.getByText("Polubione receptury")).toBeInTheDocument();
        expect(screen.getByText("Twoje receptury")).toBeInTheDocument();
    });

    it("navigates to all recipes", async () => {
        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <Recipes />
            </MemoryRouter>
        );

        await user.click(screen.getByText("Wszystkie receptury"));

        expect(navigateMock).toHaveBeenCalledWith("/recipes/all");
    });

    it("navigates to new recipe form", async () => {
        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <Recipes />
            </MemoryRouter>
        );

        await user.click(screen.getByText("Nowa receptura"));

        expect(navigateMock).toHaveBeenCalledWith("/recipes/new");
    });

    it("navigates to favorites and my recipes", async () => {
        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <Recipes />
            </MemoryRouter>
        );

        await user.click(screen.getByText("Polubione receptury"));
        expect(navigateMock).toHaveBeenCalledWith("/recipes/favorites");

        await user.click(screen.getByText("Twoje receptury"));
        expect(navigateMock).toHaveBeenCalledWith("/recipes/my");
    });
});
