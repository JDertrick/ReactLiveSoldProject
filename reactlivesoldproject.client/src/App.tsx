import { useEffect, useState } from 'react';
import './App.css';

interface Forecast {
    date: string;
    temperatureC: number;
    temperatureF: number;
    summary: string;
}

function App() {
    const [forecasts, setForecasts] = useState<Forecast[]>();

    useEffect(() => {
        populateWeatherData();
    }, []);

    const contents = forecasts === undefined
        ? <p><em>Cargando... Por favor, actualice una vez que el backend de ASP.NET se haya iniciado. Consulte <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> para obtener más detalles.</em></p>
        : <table className="table table-striped" aria-labelledby="tableLabel">
            <thead>
                <tr>
                    <th>Fecha</th>
                    <th>Temp. (C)</th>
                    <th>Temp. (F)</th>
                    <th>Resumen</th>
                </tr>
            </thead>
            <tbody>
                {forecasts.map(forecast =>
                    <tr key={forecast.date}>
                        <td>{forecast.date}</td>
                        <td>{forecast.temperatureC}</td>
                        <td>{forecast.temperatureF}</td>
                        <td>{forecast.summary}</td>
                    </tr>
                )}
            </tbody>
        </table>;

    return (
        <div>
            <h1 id="tableLabel">Pronóstico del tiempo</h1>
            <p>Este componente demuestra la obtención de datos del servidor.</p>
            {contents}
        </div>
    );

    async function populateWeatherData() {
        const response = await fetch('weatherforecast');
        const data = await response.json();
        setForecasts(data);
    }
}

export default App;