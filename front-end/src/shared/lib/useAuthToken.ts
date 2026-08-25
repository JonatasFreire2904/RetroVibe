import { useSyncExternalStore } from "react";
import { getToken, subscribeToken } from "./tokenStore";

export function useAuthToken(): string | null {
  return useSyncExternalStore(subscribeToken, getToken);
}
