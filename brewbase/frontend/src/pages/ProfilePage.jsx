import {
    Link,
    useParams
} from "react-router-dom";

import {
    Settings,
    Search,
    FileText,
    Users,
    UserPlus,
    Star,
    Coffee,
    Trophy,
    Pencil,
    MoreVertical
} from "lucide-react";

import "../styles/profile.css";

function ProfilePage() {

    // LOGGED USER

    const currentLoggedUser =
        "kontotestowe";

    // URL PARAM

    const { username } = useParams();

    // CURRENT PROFILE

    const viewedUsername =
        username || currentLoggedUser;

    // USERS

    const users = [
        {
            username: "kontotestowe",
            ranking: 27,
            points: 1248,
            recipes: 34,
            followers: 248,
            following: 96,
        },

        {
            username: "dailybrew",
            ranking: 11,
            points: 2488,
            recipes: 54,
            followers: 120,
            following: 44,
        },

        {
            username: "coffeenerd",
            ranking: 4,
            points: 4120,
            recipes: 89,
            followers: 430,
            following: 112,
        },

        {
            username: "javaholic",
            ranking: 18,
            points: 1644,
            recipes: 29,
            followers: 87,
            following: 31,
        },

        {
            username: "brew_king",
            ranking: 8,
            points: 3210,
            recipes: 67,
            followers: 156,
            following: 72,
        },

        {
            username: "coffee.flow",
            ranking: 13,
            points: 2012,
            recipes: 48,
            followers: 213,
            following: 61,
        },
    ];

    // PROFILE DATA

    const currentUser =
        users.find(
            (u) =>
                u.username === viewedUsername
        ) || users[0];

    // OWN PROFILE

    const isOwnProfile =
        viewedUsername ===
        currentLoggedUser;

    // DISCOVER USERS

    const discoverUsers =
        users.filter(
            (u) =>
                u.username !== viewedUsername
        );

    // ACTIVITIES

    const activities = [
        {
            id: 1,
            title: "Poranna jasność",
            method: "V60",
            coffee: "Ethiopia Yirgacheffe",
            likes: 24,
            rating: 4.8,
            time: "2 dni temu",
            icon: <Coffee size={22} />,
        },

        {
            id: 2,
            title: "Czekoladowy nastrój",
            method: "Chemex",
            coffee: "Brazil Santos",
            likes: 18,
            rating: 4.6,
            time: "5 dni temu",
            icon: <Coffee size={22} />,
        },

        {
            id: 3,
            title: "Niedzielny komfort",
            method: "French Press",
            coffee: "Colombia",
            likes: 31,
            rating: 4.7,
            time: "1 tydzień temu",
            icon: <Coffee size={22} />,
        },
    ];

    return (

        <div className="profile-page">

            <div className="profile-layout">

                {/* MAIN */}

                <main className="profile-main">

                    {/* TOPBAR */}

                    <div className="profile-topbar">

                        <div className="profile-heading">
                            

                            <h1 className="profile-username">
                                @{viewedUsername}
                            </h1>

                        </div>

                        <div className="profile-actions">

                            {isOwnProfile ? (

                                <>
                                    <button className="edit-profile-btn">

                                        <Pencil size={16} />

                                        Edytuj profil

                                    </button>

                                    <button className="settings-btn">

                                        <Settings size={16} />

                                    </button>
                                </>

                            ) : (

                                <>
                                    <button className="follow-profile-btn">

                                        <UserPlus size={16} />

                                        Obserwuj

                                    </button>

                                    <button className="settings-btn">

                                        <MoreVertical size={16} />

                                    </button>
                                </>

                            )}

                        </div>

                    </div>

                    {/* RANKING */}

                    <div className="ranking-box">

                        <div className="ranking-item">

                            <div className="ranking-icon">

                                <Trophy size={16} />

                            </div>

                            <div>

                                <p>
                                    Ranking
                                </p>

                                <h3>
                                    #{currentUser.ranking}
                                </h3>

                            </div>

                        </div>

                        <div className="ranking-divider" />

                        <div className="ranking-item">

                            <div className="ranking-icon">

                                <Star size={16} />

                            </div>

                            <div>

                                <p>
                                    Punkty
                                </p>

                                <h3>
                                    {currentUser.points}
                                </h3>

                            </div>

                        </div>

                    </div>

                    {/* STATS */}

                    <div className="stats-grid">

                        <div className="stat-card">

                            <div className="stat-icon">

                                <FileText size={18} />

                            </div>

                            <div>

                                <h2>
                                    {currentUser.recipes}
                                </h2>

                                <p>
                                    Przepisy
                                </p>

                            </div>

                        </div>

                        <div className="stat-card">

                            <div className="stat-icon">

                                <Users size={18} />

                            </div>

                            <div>

                                <h2>
                                    {currentUser.followers}
                                </h2>

                                <p>
                                    Obserwujący
                                </p>

                            </div>

                        </div>

                        <div className="stat-card">

                            <div className="stat-icon">

                                <UserPlus size={18} />

                            </div>

                            <div>

                                <h2>
                                    {currentUser.following}
                                </h2>

                                <p>
                                    Obserwowani
                                </p>

                            </div>

                        </div>

                    </div>

                    {/* ACTIVITY */}

                    <div className="activity-section">

                        <div className="activity-header">

                            <h2>
                                OSTATNIA AKTYWNOŚĆ
                            </h2>

                            <button>
                                Zobacz wszystkie
                            </button>

                        </div>

                        <div className="activity-list">

                            {activities.map((activity) => (

                                <Link
                                    to={`/recipes/${activity.id}`}
                                    key={activity.id}
                                    className="activity-card"
                                >

                                    <div className="activity-left">

                                        <div className="activity-icon">

                                            {activity.icon}

                                        </div>

                                        <div className="activity-info">

                                            <h3>
                                                {activity.title}
                                            </h3>

                                            <p>

                                                {activity.method}
                                                {" • "}
                                                {activity.coffee}

                                            </p>

                                            <span>

                                                ♥ {activity.likes}

                                            </span>

                                        </div>

                                    </div>

                                    <div className="activity-right">

                                        <div className="activity-rating">

                                            <Star size={14} />

                                            {activity.rating}

                                        </div>

                                        <p>
                                            {activity.time}
                                        </p>

                                    </div>

                                </Link>

                            ))}

                        </div>

                    </div>

                </main>

                {/* SIDEBAR */}

                <aside className="profile-sidebar">

                    <div className="discover-card">

                        <div className="discover-header">

                            <h2>
                                ODKRYWAJ
                            </h2>

                        </div>

                        {/* SEARCH */}

                        <div className="discover-search">

                            <Search size={16} />

                            <input
                                type="text"
                                placeholder="Szukaj użytkowników..."
                            />

                        </div>

                        {/* USERS */}

                        <div className="discover-users">

                            {discoverUsers.map((user) => (

                                <div
                                    key={user.username}
                                    className="discover-user"
                                >

                                    <Link
                                        to={`/profile/${user.username}`}
                                        className="discover-user-left"
                                    >

                                        <div className="discover-avatar">

                                            {user.username
                                                .substring(0, 2)
                                                .toUpperCase()}

                                        </div>

                                        <div className="discover-user-info">

                                            <h3>
                                                @{user.username}
                                            </h3>

                                            <p>

                                                {user.followers}
                                                {" "}
                                                obserwujących

                                            </p>

                                        </div>

                                    </Link>

                                    <button className="observe-btn">

                                        Obserwuj

                                    </button>

                                </div>

                            ))}

                        </div>

                    </div>

                </aside>

            </div>

        </div>

    );
}

export default ProfilePage;