import { Navigate, useParams } from "react-router-dom";

function RoasteryArticleRedirect() {
    const { id } = useParams();

    return <Navigate to={`/wiki/articles/${id}`} replace />;
}

export default RoasteryArticleRedirect;
