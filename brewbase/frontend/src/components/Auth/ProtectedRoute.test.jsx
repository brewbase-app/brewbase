import { describe, expect, it } from "vitest";
import { screen } from "@testing-library/react";
import { Route, Routes } from "react-router-dom";

import ProtectedRoute from "./ProtectedRoute";
import { renderWithRoutes } from "../../test/testUtils";
import { setAuthToken } from "../../utils/auth";

describe("ProtectedRoute", () => {
    it("redirects anonymous users to login", () => {
        renderWithRoutes(
            <>
                <Route element={<ProtectedRoute />}>
                    <Route
                        path="/protected"
                        element={<p>Protected content</p>}
                    />
                </Route>
                <Route path="/login" element={<p>Login page</p>} />
            </>,
            { initialEntries: ["/protected"] }
        );

        expect(screen.getByText("Login page")).toBeInTheDocument();
        expect(screen.queryByText("Protected content")).not.toBeInTheDocument();
    });

    it("renders protected content for authenticated users", () => {
        setAuthToken("jwt-token");

        renderWithRoutes(
            <>
                <Route element={<ProtectedRoute />}>
                    <Route
                        path="/protected"
                        element={<p>Protected content</p>}
                    />
                </Route>
                <Route path="/login" element={<p>Login page</p>} />
            </>,
            { initialEntries: ["/protected"] }
        );

        expect(screen.getByText("Protected content")).toBeInTheDocument();
        expect(screen.queryByText("Login page")).not.toBeInTheDocument();
    });
});
