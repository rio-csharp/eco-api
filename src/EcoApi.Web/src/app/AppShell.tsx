import { useAuth } from '../features/auth/hooks/useAuth';
import { AuthPage } from '../features/auth/components/AuthPage';
import { Dashboard } from '../features/dashboard/components/Dashboard';

export function AppShell() {
  const { user, isInitializing } = useAuth();

  if (isInitializing) {
    return (
      <main className="flex min-h-screen items-center justify-center">
        <p className="text-sm text-slate-500">Initializing secure session...</p>
      </main>
    );
  }

  if (!user) {
    return <AuthPage />;
  }

  return <Dashboard />;
}
