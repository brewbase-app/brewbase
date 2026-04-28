import Sidebar from "../components/Sidebar";

function Home() {
    return (
        <div style={{ display: "flex" }}>
            <Sidebar />

            <div style={{ padding: "40px" }}>
                <h1>HOME</h1>
            </div>
        </div>
    );
}

export default Home;