import type { AuthResponse, LoginRequest, RefreshTokenRequest, RegisterRequest } from '../model/types';

async function postJson<TRequest, TResponse>(url: string, payload: TRequest): Promise<TResponse> {
  const response = await fetch(url, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    throw new Error(`Request failed with status ${response.status}`);
  }

  return response.json() as Promise<TResponse>;
}

export function register(request: RegisterRequest): Promise<AuthResponse> {
  return postJson<RegisterRequest, AuthResponse>('/api/auth/register', request);
}

export function login(request: LoginRequest): Promise<AuthResponse> {
  return postJson<LoginRequest, AuthResponse>('/api/auth/login', request);
}

export function refresh(request: RefreshTokenRequest): Promise<AuthResponse> {
  return postJson<RefreshTokenRequest, AuthResponse>('/api/auth/refresh', request);
}
