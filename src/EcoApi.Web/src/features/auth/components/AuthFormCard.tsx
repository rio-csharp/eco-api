import type { FormEvent } from 'react';
import { useAuth } from '../hooks/useAuth';
import { useAuthForm } from '../hooks/useAuthForm';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';
import { TextInput } from '../../../shared/ui/TextInput';

export function AuthFormCard() {
  const { login, register } = useAuth();
  const {
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
  } = useAuthForm();

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      if (mode === 'register') {
        await register(toRegisterRequest());
      } else {
        await login(toLoginRequest());
      }
    } catch {
      setError('Authentication failed. Please verify your credentials and try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Card className="w-full max-w-md">
      <h1 className="text-2xl font-semibold text-slate-900">Eco API Portal</h1>
      <p className="mt-1 text-sm text-slate-500">Secure access with auto-refresh session management.</p>

      <div className="mt-5 grid grid-cols-2 gap-2 rounded-lg bg-slate-100 p-1">
        <Button variant={mode === 'login' ? 'primary' : 'ghost'} onClick={() => setMode('login')} type="button" fullWidth>
          Login
        </Button>
        <Button variant={mode === 'register' ? 'primary' : 'ghost'} onClick={() => setMode('register')} type="button" fullWidth>
          Register
        </Button>
      </div>

      <form className="mt-5 space-y-4" onSubmit={handleSubmit}>
        {mode === 'register' && (
          <TextInput
            label="Username"
            value={values.username}
            onChange={(event) => updateField('username', event.target.value)}
            required
          />
        )}
        <TextInput
          label="Email"
          type="email"
          value={values.email}
          onChange={(event) => updateField('email', event.target.value)}
          required
        />
        <TextInput
          label="Password"
          type="password"
          value={values.password}
          onChange={(event) => updateField('password', event.target.value)}
          required
        />

        {error && <p className="text-sm text-red-600">{error}</p>}

        <Button disabled={isSubmitting} fullWidth type="submit">
          {isSubmitting ? 'Submitting...' : mode === 'register' ? 'Create account' : 'Sign in'}
        </Button>
      </form>
    </Card>
  );
}
