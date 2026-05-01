# Projekt RSI-1 Docker Setup

## Uruchomienie aplikacji

### Wymagania
- Docker Desktop zainstalowany i uruchomiony
- Docker Compose w wersji 3.8+

### Szybki start

1. **Przejdź do głównego katalogu projektu:**
```bash
cd g:\Projects\Projekt-RSI-1
```

2. **Uruchom wszystkie serwisy:**
```bash
docker-compose up -d
```

3. **Poczekaj aż baza danych będzie gotowa (30-60 sekund):**
```bash
docker-compose logs -f mssql
```

### Dostęp do aplikacji

- **Frontend**: http://localhost:3000
- **Backend API**: https://localhost:8181
- **Exchanger**: https://localhost:8180
- **SQL Server**: Server=localhost,1433; User Id=sa; Password=YourPassword123!

## Przydatne komendy

### Pokaż logi
```bash
docker-compose logs -f [service-name]
```

### Zatrzymaj aplikację
```bash
docker-compose down
```

### Usunięcia dane bazy danych (WARNING!)
```bash
docker-compose down -v
```

### Przebuduj obrazy
```bash
docker-compose build --no-cache
```

### Restart serwisu
```bash
docker-compose restart [service-name]
```

### Wejdź do terminala kontenera
```bash
docker-compose exec [service-name] /bin/bash
```

## Struktura serwisów

| Serwis | Port | Wersja | Opis |
|--------|------|--------|------|
| mssql | 1433 | 2022 | Baza danych SQL Server |
| backend | 8181 | .NET 10 | API backend z WCF services |
| exchanger | 8180 | .NET 10 | Serwis wymiany danych |
| frontend | 3000 | Nuxt 4 | Aplikacja frontendowa |

## Zmienne środowiskowe

Edytuj plik `.env` aby zmienić:
- Hasło do SQL Server (`MSSQL_SA_PASSWORD`)
- Środowisko (`ASPNETCORE_ENVIRONMENT`, `NODE_ENV`)
- API Key (`API_KEY`)

## Troubleshooting

### Port już w użyciu
Zmień port w `docker-compose.yml` lub zwolnij istniejący proces:
```bash
# Windows
netstat -ano | findstr :8181
taskkill /PID [PID] /F

# Linux/Mac
lsof -i :8181
kill -9 [PID]
```

### Baza danych nie inicjuje się
```bash
docker-compose down -v
docker-compose up -d mssql
docker-compose logs mssql
```

### Certyfikaty SSL
Aplikacje .NET automatycznie generują self-signed certyfikaty dla localhost.

## Wymogi systemowe
- RAM: minimum 4GB
- Disk: 10GB wolnego miejsca
- OS: Windows 10+, macOS, Linux
