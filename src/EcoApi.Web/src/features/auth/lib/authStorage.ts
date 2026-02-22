import type { AuthUser } from '../model/types';

const AUTH_STORAGE_KEY = 'eco-auth';

export interface AuthSession {
  accessToken: string;
  refreshToken: string;
  user: AuthUser;
}

export function readStoredSession(): AuthSession | null {
  const raw = localStorage.getItem(AUTH_STORAGE_KEY);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as AuthSession;
  } catch {
    localStorage.removeItem(AUTH_STORAGE_KEY);
    return null;
  }
}

export function writeStoredSession(session: AuthSession | null) {
  if (!session) {
    localStorage.removeItem(AUTH_STORAGE_KEY);
    return;
  }

  localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(session));
}
