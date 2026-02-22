import { useContext } from 'react';
import { AuthContext } from '../context/AuthContextStore';
import type { AuthContextValue } from '../context/types';

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider.');
  }

  return context;
}
