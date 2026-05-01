# Docker Setup - Projekt RSI-1

## 📋 Stworzone pliki:

### 1. **docker-compose.yml** (katalog główny)
Plik orchestracji dla wszystkich 3 usług + baza danych SQL Server.

Usługi:
- `mssql` - SQL Server 2022 (port 1433)
- `backend` - .NET 10 Backend (port 8181, HTTPS)
- `exchanger` - .NET 10 Exchanger (port 8180, HTTPS)
- `frontend` - Nuxt.js Frontend (port 3000)

### 2. **Dockerfiles**
```
Projekt-RSI-1-BackEnd/Dockerfile
Projekt-RSI-1-Exchanger/Dockerfile
Projekt-RSI-1-FrontEnd/Dockerfile
```
Multi-stage buildery z obsługą certyfikatów SSL dla serwisów .NET.

### 3. **.dockerignore** pliki
```
Projekt-RSI-1-BackEnd/.dockerignore
Projekt-RSI-1-Exchanger/.dockerignore
Projekt-RSI-1-FrontEnd/.dockerignore
```
Optymalizacja rozmiaru obrazów Docker.

### 4. **.env** (katalog główny)
Plik z zmiennymi środowiskowymi:
- Hasło SQL Server: `YourPassword123!` ⚠️ **ZMIEŃ NA PRODUKCJI**
- API Key: `GIGATAJNYKLUCZDOAPI`

### 5. **DOCKER_README.md**
Szczegółowa dokumentacja z instrukcjami użytkowania.

### 6. Skrypty pomocnicze
```
docker-help.bat  (dla Windows)
docker-help.sh   (dla Linux/Mac)
```

## 🚀 Szybki start

```bash
# Windows
cd g:\Projects\Projekt-RSI-1
docker-compose up -d

# Linux/Mac
cd /Projects/Projekt-RSI-1
docker-compose up -d
```

## 🌐 Dostęp do aplikacji

| Serwis | URL |
|--------|-----|
| Frontend | http://localhost:3000 |
| Backend API | https://localhost:8181 |
| Exchanger | https://localhost:8180 |
| SQL Server | localhost:1433 (User: sa) |

## 📝 Przydatne komendy

```bash
# Windows - używaj docker-help.bat
docker-help.bat up              # Uruchom
docker-help.bat logs            # Pokaż logi
docker-help.bat down            # Zatrzymaj
docker-help.bat clean           # Usuń kontenery i dane

# Linux/Mac - używaj docker-help.sh
./docker-help.sh up
./docker-help.sh logs
./docker-help.sh down
./docker-help.sh clean

# Bezpośrednio docker-compose
docker-compose ps               # Status kontenerów
docker-compose logs -f backend  # Logi backend'u
docker-compose restart          # Restart
```

## ⚙️ Konfiguracja bazy danych

Backend będzie automatycznie:
1. Łączyć się z SQL Server (kontener `mssql`)
2. Tworzyć bazę danych `PROJEKT-RSI-1`
3. Uruchamiać migracje

Connection string w kontenerze:
```
Server=mssql,1433;Database=PROJEKT-RSI-1;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;
```

## 🔐 Certyfikaty SSL

Certyfikaty HTTPS dla .NET serwisów są generowane automatycznie w kontenerach (self-signed).

## ⚠️ Ważne uwagi

1. **Hasło SQL Server** - zmień `YourPassword123!` w `.env` przed produkcją
2. **CORS** - Frontend może się łączyć z Backend tylko na `https://localhost:3000` (skonfigurowane w Kestrel)
3. **Porty** - upewnij się, że porty 1433, 3000, 8180, 8181 są dostępne
4. **RAM** - rekomendowane minimum 4GB dla Docker Desktop
5. **Volumen SQL** - dane są przechowywane w wolumenie `mssql_data`

## 🧹 Czyszczenie

```bash
# Usuń wszystkie kontenery i volumen (WARNING - będą usunięte dane!)
docker-compose down -v

# Rebuild obrazów
docker-compose build --no-cache
```

## 📞 Troubleshooting

Jeśli coś nie działa:
1. Sprawdź logi: `docker-compose logs [service-name]`
2. Sprawdź status: `docker-compose ps`
3. Restartuj usługę: `docker-compose restart [service-name]`
4. Przebuduj obraz: `docker-compose build --no-cache [service-name]`

---

**Autor**: Docker Configuration Generator
**Data**: 2026-05-01
