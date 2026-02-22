import { useState } from 'react';
import type { LoginRequest, RegisterRequest } from '../model/types';

export type AuthMode = 'login' | 'register';

interface AuthFormState {
  username: string;
  email: string;
  password: string;
}

const initialState: AuthFormState = {
  username: '',
  email: '',
  password: '',
};

export function useAuthForm() {
  const [mode, setMode] = useState<AuthMode>('login');
  const [values, setValues] = useState<AuthFormState>(initialState);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const updateField = (field: keyof AuthFormState, value: string) => {
    setValues((current) => ({ ...current, [field]: value }));
  };

  const toLoginRequest = (): LoginRequest => ({
    email: values.email,
    password: values.password,
  });

  const toRegisterRequest = (): RegisterRequest => ({
    username: values.username,
    email: values.email,
    password: values.password,
  });

  return {
    mode,
    setMode,
    values,
    updateField,
    toLoginRequest,
    toRegisterRequest,
    isSubmitting,
    setIsSubmitting,
    error,
    setError,
  };
}
