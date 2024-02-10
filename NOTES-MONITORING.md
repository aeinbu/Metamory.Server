# Monitoring

## dotnet-counters
You can look at OpenTelemetry data from Metamory.Server at the command line using dotnet-counters.

To start dotnet-counters in the dev-container:
```bash
dotnet dotnet-counters monitor ps
dotnet dotnet-counters monitor -p 44609 --counters Metamory.WebApi
```
* replace 44609 with the process id of the Metamory.WebApi. You can find this by running `dotnet dotnet-counters ps` in the dev-container

Scraping endpoint in Metamory.Server is `http://locahost:5000`

## Prometheus
To start prometheus in the dev-container:
```bash
cd /workspaces/Metamory.Server/
prometheus
```
Prometheus url is `http://localhost:9090`

## grafana
Grafana is used for creating dashboards and visualizations with Prometheus as the data source, so Prometheus must allready be running to use Grafana.

To start grafana in a seperate docker container
```bash
docker run -d --name=grafana -v /Users/arjan/data/grafana/:/var/lib/grafana -p 3000:3000 grafana/grafana
```
* Connect to the Prometheus data source inside Grafana

Grafana url is `http://localhost:3000`