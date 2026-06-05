import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";

import RoasteryArticleRedirect from "../RoasteryArticleRedirect";

function ArticleDetailsPage() {
    return <div>Artykuł wiki</div>;
}

describe("RoasteryArticleRedirect", () => {
    it("redirects legacy roastery route to article details", async () => {
        render(
            <MemoryRouter initialEntries={["/wiki/roasteries/5"]}>
                <Routes>
                    <Route
                        path="/wiki/roasteries/:id"
                        element={<RoasteryArticleRedirect />}
                    />
                    <Route
                        path="/wiki/articles/:id"
                        element={<ArticleDetailsPage />}
                    />
                </Routes>
            </MemoryRouter>
        );

        expect(await screen.findByText("Artykuł wiki")).toBeInTheDocument();
    });
});
