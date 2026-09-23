import type { ReactNode } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { useAuthToken } from "@/shared/lib/useAuthToken";

export function RequireAuth({ children }: { children: ReactNode }) {
  const token = useAuthToken();
  const location = useLocation();

  if (!token) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }

  return <>{children}</>;
}
