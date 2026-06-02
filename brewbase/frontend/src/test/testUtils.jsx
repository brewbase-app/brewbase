import { render } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";

export function renderWithRouter(
    ui,
    {
        route = "/",
        path = route,
    } = {}
) {
    return render(
        <MemoryRouter initialEntries={[route]}>
            <Routes>
                <Route path={path} element={ui} />
            </Routes>
        </MemoryRouter>
    );
}

export function renderWithRoutes(routes, { initialEntries = ["/"] } = {}) {
    return render(
        <MemoryRouter initialEntries={initialEntries}>
            <Routes>{routes}</Routes>
        </MemoryRouter>
    );
}
