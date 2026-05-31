import {
    NavLink,
    useNavigate
} from "react-router-dom";

import "../styles/sidebar.css";

import {
    Home,
    User,
    BookOpen,
    FlaskConical,
    Book,
    Trophy,
    StickyNote,
    Settings
} from "lucide-react";

function Sidebar({
                     sidebarExpanded,
                     setSidebarExpanded
                 }) {

    const navigate = useNavigate();

    return (

        <div
            className={`sidebar ${
                sidebarExpanded ? "expanded" : ""
            }`}
            onMouseEnter={() =>
                setSidebarExpanded(true)
            }
            onMouseLeave={() =>
                setSidebarExpanded(false)
            }
        >

            <div
                className="logo"
                onClick={() => navigate("/home")}
            >

                <span className="logo-short">
                    BB
                </span>

                <span className="logo-full">
                    BrewBase
                </span>

            </div>

            <ul className="menu">

                {/* DASHBOARD */}
                <li>

                    <NavLink
                        to="/home"
                        className="menu-item"
                    >

                        <Home size={20} />

                        <span className="menu-text">
                            Dashboard
                        </span>

                    </NavLink>

                </li>

                {/* PROFILE */}
                <li>

                    <NavLink
                        to="/profile"
                        className="menu-item"
                    >

                        <User size={20} />

                        <span className="menu-text">
                            Profil
                        </span>

                    </NavLink>

                </li>

                {/* RECIPES */}
                <li>

                    <NavLink
                        to="/recipes"
                        className="menu-item"
                    >

                        <BookOpen size={20} />

                        <span className="menu-text">
                            Receptury
                        </span>

                    </NavLink>

                </li>

                {/* CUPPING */}
                <li>

                    <NavLink
                        to="/cupping"
                        className="menu-item"
                    >

                        <FlaskConical size={20} />

                        <span className="menu-text">
                            Cupping Sessions
                        </span>

                    </NavLink>

                </li>

                {/* WIKI */}
                <li>

                    <NavLink
                        to="/wiki"
                        className="menu-item"
                    >

                        <Book size={20} />

                        <span className="menu-text">
                            Wikipedia
                        </span>

                    </NavLink>

                </li>

                {/* RANKING */}
                <li>

                    <NavLink
                        to="/ranking"
                        className="menu-item"
                    >

                        <Trophy size={20} />

                        <span className="menu-text">
                            Ranking
                        </span>

                    </NavLink>

                </li>

                {/* QUICK NOTES */}
                <li>

                    <NavLink
                        to="/quicknotes"
                        className="menu-item"
                    >

                        <StickyNote size={20} />

                        <span className="menu-text">
                            Szybkie notatki
                        </span>

                    </NavLink>

                </li>

                {/* ADMIN */}
                <li>

                    <NavLink
                        to="/admin"
                        className="menu-item"
                    >

                        <Settings size={20} />

                        <span className="menu-text">
                            Panel Administratora
                        </span>

                    </NavLink>

                </li>

            </ul>

        </div>

    );
}

export default Sidebar;