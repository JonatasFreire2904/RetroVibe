import { getToken, setToken } from "./tokenStore";
import { pushToast } from "./toastStore";

const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:3333/api";

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly code: string,
    message: string,
    public readonly details?: unknown
  ) {
    super(message);
    this.name = "ApiError";
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const token = getToken();
  let response: Response;
  try {
    response = await fetch(`${API_URL}${path}`, {
      ...init,
      headers: {
        "Content-Type": "application/json",
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...init?.headers,
      },
    });
  } catch {
    pushToast("Não foi possível conectar ao servidor. Verifique sua conexão.");
    throw new ApiError(0, "NETWORK_ERROR", "Network request failed");
  }

  if (!response.ok) {
    const body = await response.json().catch(() => ({}));

    if (response.status === 401 && !path.startsWith("/auth/")) {
      setToken(null);
      pushToast("Sua sessão expirou. Faça login novamente.", "info");
    } else if (response.status >= 500) {
      pushToast(body.message || "Erro inesperado no servidor. Tente novamente.");
    }

    throw new ApiError(response.status, body.error ?? "UNKNOWN", body.message ?? response.statusText, body.details);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

function toQueryString(params?: Record<string, string | undefined>): string {
  if (!params) return "";
  const entries = Object.entries(params).filter((entry): entry is [string, string] => Boolean(entry[1]));
  if (entries.length === 0) return "";
  return `?${new URLSearchParams(entries).toString()}`;
}

export const httpClient = {
  get<T>(path: string, params?: Record<string, string | undefined>): Promise<T> {
    return request<T>(`${path}${toQueryString(params)}`);
  },
  post<T>(path: string, body?: unknown): Promise<T> {
    return request<T>(path, { method: "POST", body: body ? JSON.stringify(body) : undefined });
  },
  patch<T>(path: string, body?: unknown): Promise<T> {
    return request<T>(path, { method: "PATCH", body: body ? JSON.stringify(body) : undefined });
  },
};
