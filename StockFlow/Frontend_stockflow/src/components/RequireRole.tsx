import type { ReactNode } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";
import { hasAnyRole } from "../utils/auth";

type RequireRoleProps = {
  allowed: string[];
  children: ReactNode;
  title?: string;
};

export function RequireRole({ allowed, children, title = "Access denied" }: RequireRoleProps) {
  const auth = useAuth();

  if (!auth.isAuthenticated) {
    return (
      <main className="page">
        <div className="card">
          <h1>Sign in required</h1>
          <p>Please log in to view this page.</p>
          <Link to="/login" className="btn btn-primary">
            Go to login
          </Link>
        </div>
      </main>
    );
  }

  if (!hasAnyRole(auth.roles, allowed)) {
    return (
      <main className="page">
        <div className="card">
          <h1>{title}</h1>
          <p>You do not have permission to view this section.</p>
          <Link to="/" className="btn btn-primary">
            Back to dashboard
          </Link>
        </div>
      </main>
    );
  }

  return children;
}
