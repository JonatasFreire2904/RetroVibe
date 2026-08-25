import React from "react";
import ReactDOM from "react-dom/client";
import { App } from "@/app/App";
import { ErrorBoundary } from "@/app/ErrorBoundary";
import { ToastContainer } from "@/shared/ui/ToastContainer";
import "@/index.css";

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <ErrorBoundary>
      <ToastContainer />
      <App />
    </ErrorBoundary>
  </React.StrictMode>
);
