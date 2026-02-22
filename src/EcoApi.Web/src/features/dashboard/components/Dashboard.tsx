import { useAuth } from '../../auth/hooks/useAuth';
import { useWeatherForecastQuery } from '../../weather/hooks/useWeatherForecastQuery';
import { WeatherTable } from '../../weather/components/WeatherTable';
import { Button } from '../../../shared/ui/Button';
import { Card } from '../../../shared/ui/Card';

export function Dashboard() {
  const { user, logout } = useAuth();
  const weatherQuery = useWeatherForecastQuery();

  return (
    <div className="mx-auto flex w-full max-w-5xl flex-col gap-6 px-4 py-8 sm:px-6 lg:px-8">
      <Card className="flex flex-col items-start justify-between gap-4 sm:flex-row sm:items-center">
        <div>
          <h1 className="text-2xl font-semibold text-slate-900">Weather Dashboard</h1>
          <p className="mt-1 text-sm text-slate-500">Welcome back, {user?.username} ({user?.email})</p>
        </div>
        <Button onClick={logout} variant="secondary">Logout</Button>
      </Card>

      <Card>
        <h2 className="text-lg font-semibold text-slate-900">Forecast</h2>
        <p className="mt-1 text-sm text-slate-500">Data is fetched with access token and auto-refreshed on session expiry.</p>

        <div className="mt-4">
          {weatherQuery.isLoading && <p className="text-sm text-slate-600">Loading forecast...</p>}
          {weatherQuery.isError && <p className="text-sm text-red-600">Failed to load forecast.</p>}
          {weatherQuery.data && <WeatherTable data={weatherQuery.data} />}
        </div>
      </Card>
    </div>
  );
}
