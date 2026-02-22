import { useCallback, useEffect, useMemo, useRef, useState, type ReactNode } from 'react';
import * as authApi from '../api/authApi';
import { readStoredSession, writeStoredSession, type AuthSession } from '../lib/authStorage';
import type { AuthResponse, LoginRequest, RegisterRequest } from '../model/types';
import { AuthContext } from './AuthContextStore';
import type { AuthContextValue } from './types';

function mapAuthResponse(response: AuthResponse): AuthSession {
  return {
    accessToken: response.accessToken,
    refreshToken: response.refreshToken,
    user: {
      username: response.username,
      email: response.email,
    },
  };
}

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [session, setSession] = useState<AuthSession | null>(() => readStoredSession());
  const [isInitializing, setIsInitializing] = useState(true);
  const refreshInFlight = useRef<Promise<boolean> | null>(null);

  const saveSession = useCallback((nextSession: AuthSession | null) => {
    setSession(nextSession);
    writeStoredSession(nextSession);
  }, []);

  const register = useCallback(async (request: RegisterRequest) => {
    const response = await authApi.register(request);
    saveSession(mapAuthResponse(response));
  }, [saveSession]);

  const login = useCallback(async (request: LoginRequest) => {
    const response = await authApi.login(request);
    saveSession(mapAuthResponse(response));
  }, [saveSession]);

  const logout = useCallback(() => {
    saveSession(null);
  }, [saveSession]);

  const refreshSession = useCallback(async (refreshToken: string): Promise<boolean> => {
    try {
      const response = await authApi.refresh({ refreshToken });
      saveSession(mapAuthResponse(response));
      return true;
    } catch {
      saveSession(null);
      return false;
    }
  }, [saveSession]);

  useEffect(() => {
    const initialize = async () => {
      const existing = readStoredSession();
      if (!existing) {
        setIsInitializing(false);
        return;
      }

      await refreshSession(existing.refreshToken);
      setIsInitializing(false);
    };

    void initialize();
  }, [refreshSession]);

  const authorizedFetch = useCallback(async (input: RequestInfo | URL, init?: RequestInit): Promise<Response> => {
    if (!session?.accessToken) {
      throw new Error('User is not authenticated.');
    }

    const requestWithToken = (token: string) => {
      const headers = new Headers(init?.headers);
      headers.set('Authorization', `Bearer ${token}`);
      return fetch(input, { ...init, headers });
    };

    let response = await requestWithToken(session.accessToken);
    if (response.status !== 401) {
      return response;
    }

    if (!refreshInFlight.current) {
      refreshInFlight.current = refreshSession(session.refreshToken).finally(() => {
        refreshInFlight.current = null;
      });
    }

    const refreshed = await refreshInFlight.current;
    if (!refreshed) {
      return response;
    }

    const latestSession = readStoredSession();
    if (!latestSession?.accessToken) {
      return response;
    }

    response = await requestWithToken(latestSession.accessToken);
    return response;
  }, [refreshSession, session]);

  const value = useMemo<AuthContextValue>(() => ({
    user: session?.user ?? null,
    accessToken: session?.accessToken ?? null,
    isInitializing,
    register,
    login,
    logout,
    authorizedFetch,
  }), [authorizedFetch, isInitializing, login, logout, register, session]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
