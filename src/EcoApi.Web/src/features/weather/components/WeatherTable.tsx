import type { WeatherForecast } from '../model/types';

interface WeatherTableProps {
  data: WeatherForecast[];
}

export function WeatherTable({ data }: WeatherTableProps) {
  return (
    <div className="overflow-hidden rounded-xl border border-slate-200 bg-white">
      <table className="min-w-full divide-y divide-slate-200 text-sm">
        <thead className="bg-slate-50">
          <tr>
            <th className="px-4 py-3 text-left font-medium text-slate-600">Date</th>
            <th className="px-4 py-3 text-left font-medium text-slate-600">Temp. (C)</th>
            <th className="px-4 py-3 text-left font-medium text-slate-600">Temp. (F)</th>
            <th className="px-4 py-3 text-left font-medium text-slate-600">Summary</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {data.map((forecast) => (
            <tr key={forecast.date} className="hover:bg-slate-50">
              <td className="px-4 py-3">{forecast.date}</td>
              <td className="px-4 py-3">{forecast.temperatureC}</td>
              <td className="px-4 py-3">{forecast.temperatureF}</td>
              <td className="px-4 py-3">{forecast.summary}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
