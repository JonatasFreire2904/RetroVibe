import { Component } from "react";
import type { ErrorInfo, ReactNode } from "react";
import { Button } from "@/shared/ui/Button";

interface ErrorBoundaryProps {
  children: ReactNode;
}

interface ErrorBoundaryState {
  error: Error | null;
}

export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  state: ErrorBoundaryState = { error: null };

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { error };
  }

  componentDidCatch(error: Error, info: ErrorInfo): void {
    // eslint-disable-next-line no-console
    console.error("Unhandled render error", error, info.componentStack);
  }

  private handleReload = () => {
    this.setState({ error: null });
    window.location.reload();
  };

  render() {
    if (this.state.error) {
      return (
        <div className="flex min-h-screen items-center justify-center bg-violet-50/40 px-4">
          <div className="w-full max-w-sm rounded-2xl border border-slate-100 bg-white p-6 text-center shadow-sm">
            <div className="mx-auto mb-3 flex h-12 w-12 items-center justify-center rounded-2xl bg-rose-50 text-2xl">
              ⚠️
            </div>
            <h1 className="text-lg font-bold text-slate-900">Algo deu errado</h1>
            <p className="mt-1 text-sm text-slate-500">
              A tela encontrou um erro inesperado. Você pode tentar recarregar a página.
            </p>
            <Button className="mt-5 w-full" onClick={this.handleReload}>
              Recarregar
            </Button>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}
