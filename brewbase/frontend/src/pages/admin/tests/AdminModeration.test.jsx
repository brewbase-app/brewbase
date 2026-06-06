import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import AdminModeration from "../AdminModeration";

vi.mock("../../../api/adminApi", () => ({
    getPendingArticles: vi.fn(),
    getReports: vi.fn(),
    approveArticle: vi.fn(),
    rejectArticle: vi.fn(),
    dismissReport: vi.fn(),
    upholdReport: vi.fn(),
}));

import {
    approveArticle,
    dismissReport,
    getPendingArticles,
    getReports,
    rejectArticle,
    upholdReport,
} from "../../../api/adminApi";

const pendingArticle = {
    id: 1,
    title: "Poradnik V60",
    content: "Krótka treść artykułu",
    author: "maria",
    category: "Brewing",
    createdAt: "2026-01-15T10:00:00Z",
};

const openReport = {
    reportId: 10,
    contentType: "recipe",
    contentId: 5,
    contentTitle: "Zła receptura",
    category: "Spam lub reklama",
    comment: "To spam",
    reportedBy: "jan",
    status: "open",
    createdAt: "2026-01-16T10:00:00Z",
};

const historyReport = {
    reportId: 11,
    contentType: "coffee",
    contentId: 7,
    contentTitle: "Stara kawa",
    category: "Inny problem",
    comment: null,
    reportedBy: "ania",
    status: "dismissed",
    resolvedAt: "2026-01-17T10:00:00Z",
    resolvedByLogin: "admin",
    createdAt: "2026-01-15T08:00:00Z",
};

function mockModerationData({
    pending = [pendingArticle],
    open = [openReport],
    history = [historyReport],
} = {}) {
    vi.mocked(getPendingArticles).mockResolvedValue(pending);
    vi.mocked(getReports).mockImplementation((scope) => {
        if (scope === "open") {
            return Promise.resolve(open);
        }

        if (scope === "history") {
            return Promise.resolve(history);
        }

        return Promise.resolve([]);
    });
}

describe("AdminModeration", () => {
    beforeEach(() => {
        vi.mocked(getPendingArticles).mockReset();
        vi.mocked(getReports).mockReset();
        vi.mocked(approveArticle).mockReset();
        vi.mocked(rejectArticle).mockReset();
        vi.mocked(dismissReport).mockReset();
        vi.mocked(upholdReport).mockReset();
    });

    it("shows loading state initially", () => {
        vi.mocked(getPendingArticles).mockImplementation(
            () => new Promise(() => {})
        );
        vi.mocked(getReports).mockImplementation(() => new Promise(() => {}));

        render(<AdminModeration />);

        expect(screen.getByText("Ładowanie moderacji...")).toBeInTheDocument();
    });

    it("shows error when moderation data cannot be loaded", async () => {
        vi.mocked(getPendingArticles).mockRejectedValue(new Error("fail"));

        render(<AdminModeration />);

        expect(await screen.findByText("fail")).toBeInTheDocument();
    });

    it("renders moderation stats and items from api", async () => {
        mockModerationData();

        render(<AdminModeration />);

        expect(await screen.findByText("Treści do moderacji")).toBeInTheDocument();
        expect(screen.getByText("Poradnik V60")).toBeInTheDocument();
        expect(screen.getByText("Zła receptura")).toBeInTheDocument();

        const statValues = document.querySelectorAll(".admin-stat-card h2");
        expect(statValues[0]).toHaveTextContent("1");
        expect(statValues[1]).toHaveTextContent("1");
    });

    it("filters items by moderation type", async () => {
        mockModerationData();
        const user = userEvent.setup();

        render(<AdminModeration />);

        await screen.findByText("Poradnik V60");

        await user.click(screen.getByRole("button", { name: "Do akceptacji" }));
        expect(screen.getByText("Poradnik V60")).toBeInTheDocument();
        expect(screen.queryByText("Zła receptura")).not.toBeInTheDocument();

        await user.click(screen.getByRole("button", { name: "Zgłoszenia" }));
        expect(screen.getByText("Zła receptura")).toBeInTheDocument();
        expect(screen.queryByText("Poradnik V60")).not.toBeInTheDocument();

        await user.click(screen.getByRole("button", { name: "Historia zgłoszeń" }));
        expect(screen.getByText("Stara kawa")).toBeInTheDocument();
        expect(screen.queryByText("Zła receptura")).not.toBeInTheDocument();
    });

    it("approves pending article", async () => {
        mockModerationData();
        vi.mocked(approveArticle).mockResolvedValue({});

        const user = userEvent.setup();

        render(<AdminModeration />);

        await user.click(await screen.findByText("Poradnik V60"));
        await user.click(
            screen.getByRole("button", { name: /Zatwierdź treść/i })
        );

        await waitFor(() => {
            expect(approveArticle).toHaveBeenCalledWith(1);
        });

        expect(
            screen.getByText("Wybierz treść do podglądu")
        ).toBeInTheDocument();
    });

    it("rejects pending article with moderation comment", async () => {
        mockModerationData();
        vi.mocked(rejectArticle).mockResolvedValue({});

        const user = userEvent.setup();

        render(<AdminModeration />);

        await user.click(await screen.findByText("Poradnik V60"));
        await user.click(screen.getByRole("button", { name: /Odrzuć treść/i }));

        const textarea = screen.getByPlaceholderText(
            "Wyjaśnij, dlaczego treść została odrzucona..."
        );
        await user.type(textarea, "Treść nie spełnia wymagań wiki.");

        await user.click(
            screen.getByRole("button", { name: /Potwierdź odrzucenie/i })
        );

        await waitFor(() => {
            expect(rejectArticle).toHaveBeenCalledWith(
                1,
                "Treść nie spełnia wymagań wiki."
            );
        });
    });

    it("dismisses open report", async () => {
        mockModerationData();
        vi.mocked(dismissReport).mockResolvedValue({});

        const user = userEvent.setup();

        render(<AdminModeration />);

        await screen.findByText("Treści do moderacji");
        await user.click(screen.getByRole("button", { name: "Zgłoszenia" }));
        await user.click(await screen.findByText("Zła receptura"));
        await user.click(
            screen.getByRole("button", { name: /Odrzuć zgłoszenie/i })
        );

        await waitFor(() => {
            expect(dismissReport).toHaveBeenCalledWith(10);
        });
    });

    it("requires moderation comment before upholding report", async () => {
        mockModerationData();

        const user = userEvent.setup();

        render(<AdminModeration />);

        await user.click(await screen.findByText("Zła receptura"));
        await user.click(
            screen.getByRole("button", {
                name: /Zatwierdź zgłoszenie i usuń treść/i,
            })
        );

        expect(
            screen.getByText(
                "Komentarz moderacji jest wymagany przy usuwaniu treści."
            )
        ).toBeInTheDocument();
        expect(upholdReport).not.toHaveBeenCalled();
    });

    it("upholds open report after confirmation", async () => {
        mockModerationData();
        vi.mocked(upholdReport).mockResolvedValue({});

        const user = userEvent.setup();

        render(<AdminModeration />);

        await screen.findByText("Treści do moderacji");
        await user.click(screen.getByRole("button", { name: "Zgłoszenia" }));
        await user.click(await screen.findByText("Zła receptura"));

        const textarea = screen.getByPlaceholderText(
            "Wyjaśnij autorowi, dlaczego treść została usunięta..."
        );
        await user.type(textarea, "Treść narusza regulamin.");

        await user.click(
            screen.getByRole("button", {
                name: /Zatwierdź zgłoszenie i usuń treść/i,
            })
        );

        expect(
            screen.getByRole("dialog", {
                name: "Zatwierdzić zgłoszenie i usunąć treść?",
            })
        ).toBeInTheDocument();

        await user.click(
            screen.getByRole("button", { name: "Zatwierdź i usuń" })
        );

        await waitFor(() => {
            expect(upholdReport).toHaveBeenCalledWith(
                10,
                "Treść narusza regulamin."
            );
        });
    });
});
