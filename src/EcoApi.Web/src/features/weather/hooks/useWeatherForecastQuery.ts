import { useQuery } from '@tanstack/react-query';
import { getWeatherForecast } from '../api/weatherApi';
import { useAuth } from '../../auth/hooks/useAuth';

export function useWeatherForecastQuery() {
  const { user, authorizedFetch } = useAuth();

  return useQuery({
    queryKey: ['weather-forecast'],
    enabled: !!user,
    queryFn: () => getWeatherForecast(authorizedFetch),
  });
}
