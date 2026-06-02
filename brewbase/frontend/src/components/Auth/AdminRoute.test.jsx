import { describe, expect, it, vi } from "vitest";
import { screen, waitFor } from "@testing-library/react";
import { Route, Routes } from "react-router-dom";

import AdminRoute from "./AdminRoute";
import { renderWithRoutes } from "../../test/testUtils";
import { setAuthToken, setUserRole } from "../../utils/auth";

vi.mock("../../api/profileApi", () => ({
    getProfile: vi.fn(),
}));

import { getProfile } from "../../api/profileApi";

describe("AdminRoute", () => {
    it("redirects non-admin users to home", async () => {
        setAuthToken("jwt-token");
        setUserRole("User");

        renderWithRoutes(
            <>
                <Route element={<AdminRoute />}>
                    <Route path="/admin" element={<p>Admin panel</p>} />
                </Route>
                <Route path="/home" element={<p>Home page</p>} />
            </>,
            { initialEntries: ["/admin"] }
        );

        await waitFor(() => {
            expect(screen.getByText("Home page")).toBeInTheDocument();
        });

        expect(screen.queryByText("Admin panel")).not.toBeInTheDocument();
    });

    it("allows admin users through", async () => {
        setAuthToken("jwt-token");
        setUserRole("Admin");

        renderWithRoutes(
            <>
                <Route element={<AdminRoute />}>
                    <Route path="/admin" element={<p>Admin panel</p>} />
                </Route>
                <Route path="/home" element={<p>Home page</p>} />
            </>,
            { initialEntries: ["/admin"] }
        );

        await waitFor(() => {
            expect(screen.getByText("Admin panel")).toBeInTheDocument();
        });
    });

    it("loads role from profile when role is missing in storage", async () => {
        setAuthToken("jwt-token");
        vi.mocked(getProfile).mockResolvedValue({
            userId: 2,
            login: "admin.tester",
            role: "Admin",
        });

        renderWithRoutes(
            <>
                <Route element={<AdminRoute />}>
                    <Route path="/admin" element={<p>Admin panel</p>} />
                </Route>
                <Route path="/home" element={<p>Home page</p>} />
            </>,
            { initialEntries: ["/admin"] }
        );

        await waitFor(() => {
            expect(screen.getByText("Admin panel")).toBeInTheDocument();
        });
    });
});
