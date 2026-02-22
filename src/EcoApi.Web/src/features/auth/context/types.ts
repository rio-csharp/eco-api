import type { AuthUser, LoginRequest, RegisterRequest } from '../model/types';

export interface AuthContextValue {
  user: AuthUser | null;
  accessToken: string | null;
  isInitializing: boolean;
  register: (request: RegisterRequest) => Promise<void>;
  login: (request: LoginRequest) => Promise<void>;
  logout: () => void;
  authorizedFetch: (input: RequestInfo | URL, init?: RequestInit) => Promise<Response>;
}
