import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";

import MyWikiArticles from "../MyWikiArticles";

const navigateMock = vi.fn();

vi.mock("../../../api/articlesApi", () => ({
    getMyArticles: vi.fn(),
    deleteMyArticle: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useNavigate: () => navigateMock,
    };
});

import { deleteMyArticle, getMyArticles } from "../../../api/articlesApi";

const pendingArticle = {
    id: 1,
    title: "Nowa palarnia",
    module: "roastery",
    status: "Pending",
    createdAt: "2026-01-15T10:00:00Z",
};

const approvedArticle = {
    id: 2,
    title: "Kenia AA",
    module: "coffee",
    status: "Approved",
    coffeeId: 42,
    createdAt: "2026-01-16T10:00:00Z",
    publishedAt: "2026-01-17T10:00:00Z",
};

describe("MyWikiArticles", () => {
    beforeEach(() => {
        navigateMock.mockReset();
        vi.mocked(getMyArticles).mockReset();
        vi.mocked(deleteMyArticle).mockReset();
        vi.spyOn(window, "confirm").mockReturnValue(true);
    });

    it("shows loading state initially", () => {
        vi.mocked(getMyArticles).mockImplementation(() => new Promise(() => {}));

        render(
            <MemoryRouter>
                <MyWikiArticles />
            </MemoryRouter>
        );

        expect(screen.getByText("Ładowanie artykułów...")).toBeInTheDocument();
    });

    it("shows empty state when user has no articles", async () => {
        vi.mocked(getMyArticles).mockResolvedValue([]);

        render(
            <MemoryRouter>
                <MyWikiArticles />
            </MemoryRouter>
        );

        expect(
            await screen.findByText("Nie masz jeszcze żadnych artykułów.")
        ).toBeInTheDocument();
    });

    it("renders articles with statuses", async () => {
        vi.mocked(getMyArticles).mockResolvedValue([
            pendingArticle,
            approvedArticle,
        ]);

        render(
            <MemoryRouter>
                <MyWikiArticles />
            </MemoryRouter>
        );

        expect(await screen.findByText("Nowa palarnia")).toBeInTheDocument();
        expect(screen.getByText("W moderacji")).toBeInTheDocument();
        expect(screen.getByText("Opublikowany")).toBeInTheDocument();
    });

    it("navigates to pending article details", async () => {
        vi.mocked(getMyArticles).mockResolvedValue([pendingArticle]);

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <MyWikiArticles />
            </MemoryRouter>
        );

        await user.click(await screen.findByText("Nowa palarnia"));

        expect(navigateMock).toHaveBeenCalledWith("/wiki/my-articles/1");
    });

    it("navigates approved coffee article to public coffee page", async () => {
        vi.mocked(getMyArticles).mockResolvedValue([approvedArticle]);

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <MyWikiArticles />
            </MemoryRouter>
        );

        await user.click(await screen.findByText("Kenia AA"));

        expect(navigateMock).toHaveBeenCalledWith("/wiki/coffees/42");
    });

    it("deletes article after confirmation", async () => {
        vi.mocked(getMyArticles).mockResolvedValue([pendingArticle]);
        vi.mocked(deleteMyArticle).mockResolvedValue({});

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <MyWikiArticles />
            </MemoryRouter>
        );

        await screen.findByText("Nowa palarnia");
        await user.click(
            screen.getByRole("button", { name: "Usuń artykuł" })
        );

        await waitFor(() => {
            expect(deleteMyArticle).toHaveBeenCalledWith(1);
        });

        expect(screen.queryByText("Nowa palarnia")).not.toBeInTheDocument();
    });
});
