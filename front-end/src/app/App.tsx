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
import { SquadsPage } from "@/pages/SquadsPage";
import { SurveyPage } from "@/pages/SurveyPage";
import { AdminResearchPage } from "@/pages/AdminResearchPage";
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
          <Route path="/participar/:sessionId/pesquisa" element={<SurveyPage guest />} />
          <Route path="/pesquisa/:sessionId" element={<RequireAuth><SurveyPage /></RequireAuth>} />
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
            <Route path="/squads" element={<SquadsPage />} />
            <Route path="/admin/pesquisa" element={<AdminResearchPage />} />
            <Route path="/configuracoes" element={<SettingsPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AppProviders>
  );
}
