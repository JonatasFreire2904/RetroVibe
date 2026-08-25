import type { ReactNode } from "react";
import { Navigate } from "react-router-dom";
import { useCurrentUserQuery } from "@/modules/user/api/queries";

export function RequireAdmin({ children }: { children: ReactNode }) {
  const { data: currentUser, isLoading } = useCurrentUserQuery();

  if (isLoading) return null;
  if (currentUser?.accessLevel !== "ADMIN") {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
}
