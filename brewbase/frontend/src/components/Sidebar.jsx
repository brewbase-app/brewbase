import { NavLink } from "react-router-dom";
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


function Sidebar() {
    return (
        <div className="sidebar">
            <div className="logo">
                <span className="logo-short">BB</span>
                <span className="logo-full">BrewBase</span>
            </div>
            <ul className="menu">
                <li className="menu-item">
                    <Home size={20} />
                    <span className="menu-text">Dashboard</span>
                </li>

                <li className="menu-item">
                    <User size={20} />
                    <span className="menu-text">Profil</span>
                </li>

                <li>
                    <NavLink to="/recipes" className="menu-item">
                        <BookOpen size={20} />
                        <span className="menu-text">Receptury</span>
                    </NavLink>
                </li>

                <li>
                    <NavLink to="/cupping" className="menu-item">
                        <FlaskConical size={20} />
                        <span className="menu-text">Cupping Sessions</span>
                    </NavLink>
                </li>

                <li>
                    <NavLink to="/wiki" className="menu-item">
                        <Book size={20} />
                        <span className="menu-text">Wikipedia</span>
                    </NavLink>
                </li>

                <li className="menu-item">
                    <Trophy size={20} />
                    <span className="menu-text">Ranking</span>
                </li>

                <li>
                    <NavLink to="/quicknotes" className="menu-item">
                        <StickyNote size={20} />
                        <span className="menu-text">Quick Notes</span>
                    </NavLink>
                </li>

                <li className="menu-item">
                    <Settings size={20} />
                    <span className="menu-text">Panel Administratora</span>
                </li>
            </ul>
        </div>
    );
}

export default Sidebar;