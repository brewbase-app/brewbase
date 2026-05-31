import { useCallback, useEffect, useMemo, useState } from "react";

import {
    Link,
    useParams
} from "react-router-dom";

import {
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

import { getProfile } from "../api/profileApi";
import { ApiError } from "../api/apiClient";
import { getRecipes } from "../api/recipeApi";
import {
    followUser,
    getFollowers,
    getFollowing,
    getUserProfile,
    getUserProfileByLogin,
    unfollowUser,
} from "../api/communityApi";
import { getUserRanking } from "../api/rankingApi";

import "../styles/profile.css";

function resolveUserId(entity) {
    if (!entity) {
        return null;
    }

    const rawId = entity.userId ?? entity.UserId;
    const parsed = Number(rawId);

    return Number.isFinite(parsed) ? parsed : null;
}

function formatRelativeTime(dateString) {
    if (!dateString) {
        return "";
    }

    const date = new Date(dateString);

    if (Number.isNaN(date.getTime())) {
        return "";
    }

    const diffMs = Date.now() - date.getTime();
    const days = Math.floor(diffMs / (1000 * 60 * 60 * 24));

    if (days <= 0) {
        return "Dziś";
    }

    if (days === 1) {
        return "Wczoraj";
    }

    if (days < 7) {
        return `${days} dni temu`;
    }

    if (days < 30) {
        const weeks = Math.floor(days / 7);
        return weeks === 1 ? "1 tydzień temu" : `${weeks} tygodnie temu`;
    }

    return date.toLocaleDateString("pl-PL");
}

function sortRecipesByNewest(recipes) {
    return [...recipes].sort((left, right) => {
        const leftTime = new Date(left.createdAt ?? 0).getTime();
        const rightTime = new Date(right.createdAt ?? 0).getTime();

        return rightTime - leftTime;
    });
}

function ProfilePage() {
    const { username } = useParams();

    const [currentUser, setCurrentUser] = useState(null);
    const [viewedProfile, setViewedProfile] = useState(null);
    const [rankingPosition, setRankingPosition] = useState(null);
    const [discoverSource, setDiscoverSource] = useState([]);
    const [searchQuery, setSearchQuery] = useState("");
    const [followersList, setFollowersList] = useState([]);
    const [followingList, setFollowingList] = useState([]);
    const [userRecipes, setUserRecipes] = useState([]);
    const [remoteDiscoverUser, setRemoteDiscoverUser] = useState(null);
    const [isFollowing, setIsFollowing] = useState(false);
    const [followLoading, setFollowLoading] = useState(false);

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    const [openModal, setOpenModal] = useState(null);

    const loadProfile = useCallback(async () => {
        try {
            setLoading(true);
            setError("");

            const me = await getProfile();
            setCurrentUser(me);

            const currentUserId = resolveUserId(me);

            if (currentUserId == null) {
                setError("Nie udało się ustalić ID użytkownika.");
                setViewedProfile(null);
                return;
            }

            let publicProfile;

            if (username && username !== me.login) {
                publicProfile = await getUserProfileByLogin(username);

                if (!publicProfile) {
                    setError("Nie znaleziono użytkownika.");
                    setViewedProfile(null);
                    return;
                }
            } else {
                publicProfile = await getUserProfile(currentUserId);
            }

            const ranking = await getUserRanking(100);

            let myFollowing = [];

            try {
                myFollowing = await getFollowing(currentUserId);
            } catch (followingError) {
                console.error(followingError);
            }

            const viewedUserId = resolveUserId(publicProfile);

            if (viewedUserId == null) {
                setError("Nie udało się pobrać profilu użytkownika.");
                setViewedProfile(null);
                return;
            }

            let followers = [];

            try {
                followers = (await getFollowers(viewedUserId)) ?? [];
            } catch (followersError) {
                console.error(followersError);
            }

            let recipes = [];

            try {
                recipes = sortRecipesByNewest(
                    (await getRecipes({ userId: viewedUserId })) ?? []
                );
            } catch (recipesError) {
                console.error(recipesError);
            }

            setViewedProfile({
                ...publicProfile,
                userId: viewedUserId,
            });
            setFollowingList(myFollowing);
            setFollowersList(followers);
            setUserRecipes(recipes);
            setRemoteDiscoverUser(null);

            const rankingEntry = ranking.find(
                (entry) => resolveUserId(entry) === viewedUserId
            );
            setRankingPosition(rankingEntry?.position ?? null);

            setDiscoverSource(
                ranking.filter(
                    (entry) =>
                        resolveUserId(entry) !== viewedUserId &&
                        entry.login !== me.login
                )
            );
            setSearchQuery("");

            const isOwnProfileView =
                !username || username === me.login;

            setIsFollowing(
                isOwnProfileView ? false : Boolean(publicProfile.isFollowing)
            );
        } catch (loadError) {
            console.error(loadError);
            setError("Nie udało się pobrać profilu.");
        } finally {
            setLoading(false);
        }
    }, [username]);

    useEffect(() => {
        loadProfile();
    }, [loadProfile]);

    useEffect(() => {
        const query = searchQuery.trim();

        if (query.length < 2) {
            setRemoteDiscoverUser(null);
            return undefined;
        }

        const hasLocalMatch = discoverSource.some((user) =>
            user.login.toLowerCase().includes(query.toLowerCase())
        );

        if (hasLocalMatch) {
            setRemoteDiscoverUser(null);
            return undefined;
        }

        const timeoutId = setTimeout(async () => {
            try {
                const profile = await getUserProfileByLogin(query);
                const profileUserId = resolveUserId(profile);

                if (
                    !profile ||
                    profileUserId == null ||
                    profileUserId === resolveUserId(currentUser) ||
                    profileUserId === resolveUserId(viewedProfile)
                ) {
                    setRemoteDiscoverUser(null);
                    return;
                }

                setRemoteDiscoverUser({
                    userId: profileUserId,
                    login: profile.login,
                    followersCount: profile.followersCount ?? 0,
                });
            } catch {
                setRemoteDiscoverUser(null);
            }
        }, 400);

        return () => clearTimeout(timeoutId);
    }, [searchQuery, discoverSource, currentUser, viewedProfile]);

    const viewedUsername = viewedProfile?.login ?? username ?? currentUser?.login;
    const viewedUserId = resolveUserId(viewedProfile);
    const currentUserId = resolveUserId(currentUser);
    const isOwnProfile =
        viewedUserId != null &&
        currentUserId != null &&
        currentUserId === viewedUserId;

    const isUserFollowed = (userId) =>
        followingList.some(
            (user) => resolveUserId(user) === resolveUserId({ userId })
        );

    const discoverUsers = useMemo(() => {
        const query = searchQuery.trim().toLowerCase();

        let users = query
            ? discoverSource.filter((user) =>
                  user.login.toLowerCase().includes(query)
              )
            : discoverSource.slice(0, 5);

        if (
            remoteDiscoverUser &&
            !users.some(
                (user) =>
                    resolveUserId(user) === resolveUserId(remoteDiscoverUser)
            )
        ) {
            users = [...users, remoteDiscoverUser];
        }

        return users;
    }, [discoverSource, searchQuery, remoteDiscoverUser]);

    const recentRecipes = userRecipes.slice(0, 3);

    const updateDiscoverFollowersCount = (targetUserId, delta) => {
        setDiscoverSource((previous) =>
            previous.map((user) =>
                resolveUserId(user) === targetUserId
                    ? {
                          ...user,
                          followersCount: Math.max(
                              0,
                              (user.followersCount ?? 0) + delta
                          ),
                      }
                    : user
            )
        );
    };

    const handleFollowToggle = async (targetUserId, targetLogin, targetLabel) => {
        const normalizedTargetId = resolveUserId({ userId: targetUserId });

        if (
            normalizedTargetId == null ||
            currentUserId == null ||
            normalizedTargetId === currentUserId ||
            followLoading
        ) {
            return;
        }

        const alreadyFollowing = isUserFollowed(normalizedTargetId);

        try {
            setFollowLoading(true);
            setError("");

            if (alreadyFollowing) {
                await unfollowUser(normalizedTargetId);
                setFollowingList((previous) =>
                    previous.filter(
                        (user) =>
                            resolveUserId(user) !== normalizedTargetId
                    )
                );
                updateDiscoverFollowersCount(normalizedTargetId, -1);

                if (isOwnProfile) {
                    setViewedProfile((previous) => ({
                        ...previous,
                        followingCount: Math.max(
                            0,
                            (previous.followingCount ?? 0) - 1
                        ),
                    }));
                }

                if (viewedUserId === normalizedTargetId) {
                    setIsFollowing(false);
                    setViewedProfile((previous) => ({
                        ...previous,
                        isFollowing: false,
                        followersCount: Math.max(
                            0,
                            (previous.followersCount ?? 0) - 1
                        ),
                    }));
                }
            } else {
                await followUser(normalizedTargetId);
                setFollowingList((previous) => [
                    ...previous,
                    {
                        userId: normalizedTargetId,
                        login: targetLogin,
                        label: targetLabel ?? null,
                    },
                ]);
                updateDiscoverFollowersCount(normalizedTargetId, 1);

                if (isOwnProfile) {
                    setViewedProfile((previous) => ({
                        ...previous,
                        followingCount: (previous.followingCount ?? 0) + 1,
                    }));
                }

                if (viewedUserId === normalizedTargetId) {
                    setIsFollowing(true);
                    setViewedProfile((previous) => ({
                        ...previous,
                        isFollowing: true,
                        followersCount: (previous.followersCount ?? 0) + 1,
                    }));
                }
            }
        } catch (followError) {
            console.error(followError);
            setError(
                followError instanceof ApiError
                    ? followError.message
                    : "Nie udało się zaktualizować obserwowania."
            );
        } finally {
            setFollowLoading(false);
        }
    };

    const modalUsers =
        openModal === "followers" ? followersList : followingList;

    const getRecipeSubtitle = (recipe) => {
        const parts = [recipe.brewingMethod, recipe.coffee].filter(Boolean);
        return parts.length > 0 ? parts.join(" • ") : "Receptura";
    };

    if (loading) {
        return (
            <div className="profile-page">
                <div
                    style={{
                        color: "white",
                        padding: "40px",
                        fontSize: "20px",
                    }}
                >
                    Ładowanie profilu...
                </div>
            </div>
        );
    }

    if (error && !viewedProfile) {
        return (
            <div className="profile-page">
                <div
                    style={{
                        padding: "40px",
                        fontSize: "18px",
                        color: "#111",
                    }}
                >
                    {error}
                </div>
            </div>
        );
    }

    return (
        <div className="profile-page">
            <div className="profile-layout">
                <main className="profile-main">
                    {error && (
                        <div
                            style={{
                                marginBottom: "16px",
                                padding: "12px 16px",
                                borderRadius: "14px",
                                background: "#fff3f3",
                                border: "1px solid #f0d4d4",
                                color: "#8f3f3f",
                                fontSize: "14px",
                            }}
                        >
                            {error}
                        </div>
                    )}

                    <div className="profile-topbar">
                        <div className="profile-heading">
                            <h1 className="profile-username">
                                @{viewedUsername}
                            </h1>

                            {viewedProfile?.label && (
                                <p
                                    style={{
                                        marginTop: "10px",
                                        fontSize: "16px",
                                        color: "#666",
                                        maxWidth: "640px",
                                    }}
                                >
                                    {viewedProfile.label}
                                </p>
                            )}
                        </div>

                        <div className="profile-actions">
                            {isOwnProfile ? (
                                <Link
                                    to="/profile/edit"
                                    className="edit-profile-btn"
                                >
                                    <Pencil size={16} />
                                    Edytuj profil
                                </Link>
                            ) : (
                                <>
                                    <button
                                        type="button"
                                        className={
                                            isFollowing
                                                ? "follow-profile-btn follow-profile-btn--following"
                                                : "follow-profile-btn"
                                        }
                                        onClick={() =>
                                            handleFollowToggle(
                                                viewedUserId,
                                                viewedProfile.login,
                                                viewedProfile.label
                                            )
                                        }
                                        disabled={followLoading}
                                    >
                                        {!isFollowing && (
                                            <UserPlus size={16} />
                                        )}
                                        {followLoading
                                            ? "..."
                                            : isFollowing
                                              ? "Obserwujesz"
                                              : "Obserwuj"}
                                    </button>

                                    <button className="settings-btn">
                                        <MoreVertical size={16} />
                                    </button>
                                </>
                            )}
                        </div>
                    </div>

                    <div className="ranking-box">
                        <div className="ranking-item">
                            <div className="ranking-icon">
                                <Trophy size={16} />
                            </div>

                            <div>
                                <p>Ranking</p>
                                <h3>
                                    {rankingPosition != null
                                        ? `#${rankingPosition}`
                                        : "—"}
                                </h3>
                            </div>
                        </div>

                        <div className="ranking-divider" />

                        <div className="ranking-item">
                            <div className="ranking-icon">
                                <Star size={16} />
                            </div>

                            <div>
                                <p>Punkty</p>
                                <h3>
                                    {viewedProfile?.activityPoints ?? 0}
                                </h3>
                            </div>
                        </div>
                    </div>

                    <div className="stats-grid">
                        <button
                            type="button"
                            className="stat-card clickable"
                            onClick={() => setOpenModal("recipes")}
                        >
                            <div className="stat-icon">
                                <FileText size={18} />
                            </div>

                            <div>
                                <h2>
                                    {viewedProfile?.recipesCount ??
                                        userRecipes.length}
                                </h2>
                                <p>Przepisy</p>
                            </div>
                        </button>

                        <button
                            type="button"
                            className={
                                isOwnProfile
                                    ? "stat-card clickable"
                                    : "stat-card"
                            }
                            onClick={() => {
                                if (isOwnProfile) {
                                    setOpenModal("followers");
                                }
                            }}
                        >
                            <div className="stat-icon">
                                <Users size={18} />
                            </div>

                            <div>
                                <h2>
                                    {viewedProfile?.followersCount ?? 0}
                                </h2>
                                <p>Obserwujący</p>
                            </div>
                        </button>

                        <button
                            type="button"
                            className={
                                isOwnProfile
                                    ? "stat-card clickable"
                                    : "stat-card"
                            }
                            onClick={() => {
                                if (isOwnProfile) {
                                    setOpenModal("following");
                                }
                            }}
                        >
                            <div className="stat-icon">
                                <UserPlus size={18} />
                            </div>

                            <div>
                                <h2>
                                    {viewedProfile?.followingCount ?? 0}
                                </h2>
                                <p>Obserwowani</p>
                            </div>
                        </button>
                    </div>

                    <div className="activity-section">
                        <div className="activity-header">
                            <h2>OSTATNIA AKTYWNOŚĆ</h2>
                        </div>

                        {recentRecipes.length === 0 ? (
                            <p className="activity-empty">
                                Brak aktywności.
                            </p>
                        ) : (
                            <div className="activity-list">
                                {recentRecipes.map((recipe) => (
                                    <Link
                                        to={`/recipes/${recipe.id}`}
                                        key={recipe.id}
                                        className="activity-card"
                                    >
                                        <div className="activity-left">
                                            <div className="activity-icon">
                                                <Coffee size={22} />
                                            </div>

                                            <div className="activity-info">
                                                <h3>{recipe.title}</h3>
                                                <p>
                                                    {getRecipeSubtitle(recipe)}
                                                </p>
                                            </div>
                                        </div>

                                        <div className="activity-right">
                                            <p>
                                                {formatRelativeTime(
                                                    recipe.createdAt
                                                )}
                                            </p>
                                        </div>
                                    </Link>
                                ))}
                            </div>
                        )}
                    </div>
                </main>

                <aside className="profile-sidebar">
                    <div className="discover-card">
                        <div className="discover-header">
                            <h2>ODKRYWAJ</h2>
                        </div>

                        <div className="discover-search">
                            <Search size={16} />
                            <input
                                type="text"
                                placeholder="Szukaj użytkowników..."
                                value={searchQuery}
                                onChange={(event) =>
                                    setSearchQuery(event.target.value)
                                }
                            />
                        </div>

                        <div className="discover-users">
                            {discoverUsers.length === 0 ? (
                                <p className="discover-empty">
                                    {searchQuery.trim()
                                        ? "Nie znaleziono użytkowników."
                                        : "Brak użytkowników do odkrycia."}
                                </p>
                            ) : (
                            discoverUsers.map((user) => {
                                const discoverUserId = resolveUserId(user);
                                const discoverFollowed =
                                    isUserFollowed(discoverUserId);

                                return (
                                <div
                                    key={discoverUserId ?? user.login}
                                    className="discover-user"
                                >
                                    <Link
                                        to={`/profile/${user.login}`}
                                        className="discover-user-left"
                                    >
                                        <div className="discover-avatar">
                                            {user.login
                                                .substring(0, 2)
                                                .toUpperCase()}
                                        </div>

                                        <div className="discover-user-info">
                                            <h3>@{user.login}</h3>
                                            <p>
                                                {user.followersCount}{" "}
                                                obserwujących
                                            </p>
                                        </div>
                                    </Link>

                                    <button
                                        type="button"
                                        className={
                                            discoverFollowed
                                                ? "observe-btn observe-btn--following"
                                                : "observe-btn"
                                        }
                                        disabled={followLoading}
                                        onClick={() =>
                                            handleFollowToggle(
                                                discoverUserId,
                                                user.login
                                            )
                                        }
                                    >
                                        {discoverFollowed
                                            ? "Obserwujesz"
                                            : "Obserwuj"}
                                    </button>
                                </div>
                                );
                            })
                            )}
                        </div>
                    </div>
                </aside>
            </div>

            {openModal && (
                <div
                    className="followers-modal-overlay"
                    onClick={() => setOpenModal(null)}
                >
                    <div
                        className="followers-modal"
                        onClick={(event) => event.stopPropagation()}
                    >
                        <div className="followers-modal-header">
                            <h2>
                                {openModal === "followers"
                                    ? "Obserwujący"
                                    : openModal === "following"
                                      ? "Obserwowani"
                                      : "Przepisy"}
                            </h2>

                            <button onClick={() => setOpenModal(null)}>
                                ✕
                            </button>
                        </div>

                        <div className="followers-list">
                            {openModal === "recipes" ? (
                                userRecipes.length === 0 ? (
                                    <p className="modal-empty">
                                        Brak przepisów do wyświetlenia.
                                    </p>
                                ) : (
                                    userRecipes.map((recipe) => (
                                        <Link
                                            key={recipe.id}
                                            to={`/recipes/${recipe.id}`}
                                            className="followers-user profile-recipe-item"
                                            onClick={() => setOpenModal(null)}
                                        >
                                            <span className="profile-recipe-title">
                                                {recipe.title}
                                            </span>
                                            <span className="profile-recipe-meta">
                                                {getRecipeSubtitle(recipe)}
                                                {" · "}
                                                {formatRelativeTime(
                                                    recipe.createdAt
                                                )}
                                            </span>
                                        </Link>
                                    ))
                                )
                            ) : modalUsers.length === 0 ? (
                                <p className="modal-empty">
                                    {openModal === "followers"
                                        ? "Brak obserwujących."
                                        : "Brak obserwowanych użytkowników."}
                                </p>
                            ) : (
                                modalUsers.map((user) => (
                                    <Link
                                        key={user.userId ?? user.login}
                                        to={`/profile/${user.login}`}
                                        className="followers-user"
                                        onClick={() => setOpenModal(null)}
                                    >
                                        @{user.login}
                                    </Link>
                                ))
                            )}
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default ProfilePage;
