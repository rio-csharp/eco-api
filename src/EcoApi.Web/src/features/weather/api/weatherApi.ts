import type { WeatherForecast } from '../model/types';

export async function getWeatherForecast(authorizedFetch: (input: RequestInfo | URL, init?: RequestInit) => Promise<Response>) {
  const response = await authorizedFetch('/api/weatherforecast');
  if (!response.ok) {
    throw new Error(`Weather request failed with status ${response.status}`);
  }

  return response.json() as Promise<WeatherForecast[]>;
}
