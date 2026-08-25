import type { ReactNode } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { getParticipantMeta } from "@/shared/lib/participantMeta";
import { useAuthToken } from "@/shared/lib/useAuthToken";

export function RequireAuth({ children }: { children: ReactNode }) {
  const token = useAuthToken();
  const location = useLocation();

  if (!token) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }

  const participantMeta = getParticipantMeta();
  if (participantMeta) {
    return <Navigate to={`/participar/${participantMeta.sessionId}`} replace />;
  }

  return <>{children}</>;
}
