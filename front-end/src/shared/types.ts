export interface TemplateColumn {
  key: string;
  label: string;
  icon: string;
}

export interface Template {
  id: string;
  key: string;
  label: string;
  icon: string;
  description: string;
  isCustom: boolean;
  active: boolean;
  columns: TemplateColumn[];
}

export interface Theme {
  id: string;
  key: string;
  label: string;
  emoji: string;
  active: boolean;
}

export interface Squad {
  id: string;
  name: string;
}

export type SessionStatus = "ACTIVE" | "PAUSED" | "COMPLETED";
export type SessionPhase = "COLLECTING" | "VOTING" | "DISCUSSING";
export type PrivacyMode = "ANONYMOUS" | "IDENTIFIED";

export interface SessionSummary {
  id: string;
  title: string | null;
  status: SessionStatus;
  template: Template;
  theme: Theme;
  squad: { id: string; name: string };
  date: string;
  participantsCount: number;
  actionItemsCount: number;
  durationMinutes: number;
}

export interface SessionCard {
  id: string;
  text: string;
  authorId: string | null;
  isMine: boolean;
  votes: number;
  commentsCount: number;
  createdAt: string;
}

export interface SessionColumn {
  id: string;
  key: string;
  label: string;
  icon: string;
  cards: SessionCard[];
}

export interface SessionBoard {
  id: string;
  title: string | null;
  status: SessionStatus;
  phase: SessionPhase;
  sequentialFlow: boolean;
  activeColumnIndex: number;
  privacyMode: PrivacyMode;
  actionCardsEnabled: boolean;
  template: Template;
  theme: Theme;
  squad: { id: string; name: string };
  createdAt: string;
  closedAt: string | null;
  durationMinutes: number;
  participantsCount: number;
  actionItemsCount: number;
  columns: SessionColumn[];
}

export type ActionItemStatus = "PLANNED" | "BACKLOG" | "IN_PROGRESS" | "DONE" | "DISCARDED";

export interface ActionItemAssignee {
  id: string;
  name: string;
  avatarColor: string;
}

export interface ActionItem {
  id: string;
  sessionId: string;
  description: string;
  status: ActionItemStatus;
  assignee: ActionItemAssignee | null;
  dueDate: string | null;
  commentsCount: number;
  createdAt: string;
}

export interface ActionItemSessionSummary {
  sessionId: string;
  templateLabel: string;
  templateIcon: string;
  themeIcon: string;
  date: string;
  totalItems: number;
  completedItems: number;
}

export interface TeamDashboard {
  averageParticipants: number;
  averageFeedbackScore: number;
  averageDurationMinutes: number;
  sessionsInPeriod: number;
  phaseAverages: {
    collectMinutes: number;
    voteMinutes: number;
    discussMinutes: number;
  };
  templatesUsage: { label: string; count: number }[];
  themesUsage: { label: string; count: number }[];
  participantsSeries: { sessionLabel: string; participants: number }[];
}

export type AccessLevel = "ADMIN" | "FACILITATOR" | "PARTICIPANT";

export interface CurrentUser {
  id: string;
  name: string;
  role: string;
  squad: string | null;
  avatarColor: string;
  accessLevel: AccessLevel;
}

export interface Comment {
  id: string;
  authorId: string | null;
  authorName: string | null;
  text: string;
  createdAt: string;
}

export interface HomeData {
  recentSessions: SessionSummary[];
  quickTemplates: Template[];
}
