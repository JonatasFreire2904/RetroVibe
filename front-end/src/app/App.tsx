import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { ActionItemsPage } from "@/pages/ActionItemsPage";
import { HomePage } from "@/pages/HomePage";
import { JoinSessionPage } from "@/pages/JoinSessionPage";
import { LoginPage } from "@/pages/LoginPage";
import { ParticipantSessionPage } from "@/pages/ParticipantSessionPage";
import { SessionBoardPage } from "@/pages/SessionBoardPage";
import { SessionHistoryPage } from "@/pages/SessionHistoryPage";
import { SettingsPage } from "@/pages/SettingsPage";
import { TeamDashboardPage } from "@/pages/TeamDashboardPage";
import { AppLayout } from "./AppLayout";
import { AppProviders } from "./providers";
import { RequireAuth } from "./RequireAuth";

export function App() {
  return (
    <AppProviders>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />

          <Route path="/entrar/:sessionId" element={<JoinSessionPage />} />
          <Route path="/participar/:sessionId" element={<ParticipantSessionPage />} />
          <Route
            path="/sessoes/:sessionId"
            element={
              <RequireAuth>
                <SessionBoardPage />
              </RequireAuth>
            }
          />

          <Route
            element={
              <RequireAuth>
                <AppLayout />
              </RequireAuth>
            }
          >
            <Route path="/" element={<HomePage />} />
            <Route path="/historico" element={<SessionHistoryPage />} />
            <Route path="/itens-de-acao" element={<ActionItemsPage />} />
            <Route path="/dashboard" element={<TeamDashboardPage />} />
            <Route path="/configuracoes" element={<SettingsPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AppProviders>
  );
}
