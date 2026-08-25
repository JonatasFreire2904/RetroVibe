const MONTHS_PT = ["jan", "fev", "mar", "abr", "mai", "jun", "jul", "ago", "set", "out", "nov", "dez"];

export function formatDate(isoDate: string): string {
  const date = new Date(isoDate);
  return `${String(date.getDate()).padStart(2, "0")} ${MONTHS_PT[date.getMonth()]} ${date.getFullYear()}`;
}

export function formatDateInput(isoDate: string): string {
  return isoDate.slice(0, 10);
}

export function formatDuration(minutes: number): string {
  return `${minutes} min`;
}
