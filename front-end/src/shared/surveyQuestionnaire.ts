export interface SurveyQuestion {
  number: number;
  text: string;
  facilitatorText?: string;
}

export const susQuestions: SurveyQuestion[] = [
  { number: 1, text: "Considero que utilizaria o RetroVibe com frequência nas retrospectivas." },
  { number: 2, text: "O RetroVibe apresenta complexidade desnecessária." },
  { number: 3, text: "O RetroVibe é fácil de utilizar." },
  { number: 4, text: "Seria necessário o apoio de uma pessoa com conhecimento técnico para utilizar o RetroVibe." },
  { number: 5, text: "As funcionalidades do RetroVibe estão bem integradas." },
  { number: 6, text: "O RetroVibe apresenta muitas inconsistências." },
  { number: 7, text: "A maioria das pessoas aprenderia a utilizar o RetroVibe rapidamente." },
  { number: 8, text: "O RetroVibe é confuso de utilizar." },
  { number: 9, text: "Senti-me confiante ao utilizar o RetroVibe." },
  { number: 10, text: "Foi necessário aprender muitos conceitos antes de conseguir utilizar o RetroVibe." },
];

export const uesQuestions: SurveyQuestion[] = [
  { number: 11, text: "Fiquei completamente imerso(a) nesta experiência." },
  { number: 12, text: "O tempo que dediquei à utilização do RetroVibe passou sem que eu percebesse." },
  { number: 13, text: "Estive envolvido(a) por esta experiência.", facilitatorText: "Estive absorvido(a) por esta experiência." },
  { number: 14, text: "O RetroVibe é atraente." },
  { number: 15, text: "O RetroVibe é esteticamente agradável." },
  { number: 16, text: "O RetroVibe despertou meus sentidos.", facilitatorText: "O RetroVibe estimulou os meus sentidos." },
  { number: 17, text: "A utilização do RetroVibe se revelou de grande utilidade.", facilitatorText: "A utilização do RetroVibe valeu a pena." },
  { number: 18, text: "A minha experiência foi recompensadora." },
  { number: 19, text: "Senti interesse por esta experiência." },
];

export const teamQuestions: SurveyQuestion[] = [
  { number: 20, text: "A equipe esteve mais engajada na retrospectiva com o uso do RetroVibe." },
  { number: 21, text: "O tema utilizado despertou o interesse da equipe pela retrospectiva." },
  { number: 22, text: "A retrospectiva com o RetroVibe foi menos repetitiva do que o formato habitual." },
  { number: 23, text: "O RetroVibe facilitou a troca de ideias entre os membros da equipe." },
  { number: 24, text: "A retrospectiva com o RetroVibe contribuiu para a definição de ações de melhoria para a equipe." },
];

export function questionText(question: SurveyQuestion, facilitator: boolean) {
  return facilitator ? question.facilitatorText ?? question.text : question.text;
}
