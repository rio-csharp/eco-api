import { AuthFormCard } from './AuthFormCard';

export function AuthPage() {
  return (
    <main className="relative flex min-h-screen items-center justify-center overflow-hidden px-4 py-8">
      <div className="absolute inset-0 bg-gradient-to-br from-indigo-100 via-slate-100 to-purple-100" />
      <div className="absolute -top-24 -left-24 h-72 w-72 rounded-full bg-brand-500/20 blur-3xl" />
      <div className="absolute -right-20 bottom-0 h-72 w-72 rounded-full bg-purple-400/20 blur-3xl" />
      <div className="relative w-full max-w-md">
        <AuthFormCard />
      </div>
    </main>
  );
}
