@echo off
REM Docker helper script for Projekt RSI-1

if "%1%"=="" (
    echo.
    echo Usage: docker-help.bat [command]
    echo.
    echo Commands:
    echo   up              - Start all services
    echo   down            - Stop all services
    echo   logs            - Show logs from all services
    echo   logs-backend    - Show backend logs
    echo   logs-exchanger  - Show exchanger logs
    echo   logs-frontend   - Show frontend logs
    echo   logs-db         - Show database logs
    echo   rebuild         - Rebuild all images
    echo   restart         - Restart all services
    echo   clean           - Remove all containers and volumes
    echo   ps              - Show running containers
    echo   shell-backend   - Access backend container shell
    echo   shell-exchanger - Access exchanger container shell
    echo   shell-frontend  - Access frontend container shell
    echo.
    goto :eof
)

if "%1%"=="up" (
    docker-compose up -d
    timeout /t 2 /nobreak
    docker-compose ps
    goto :eof
)

if "%1%"=="down" (
    docker-compose down
    goto :eof
)

if "%1%"=="logs" (
    docker-compose logs -f
    goto :eof
)

if "%1%"=="logs-backend" (
    docker-compose logs -f backend
    goto :eof
)

if "%1%"=="logs-exchanger" (
    docker-compose logs -f exchanger
    goto :eof
)

if "%1%"=="logs-frontend" (
    docker-compose logs -f frontend
    goto :eof
)

if "%1%"=="logs-db" (
    docker-compose logs -f mssql
    goto :eof
)

if "%1%"=="rebuild" (
    docker-compose build --no-cache
    goto :eof
)

if "%1%"=="restart" (
    docker-compose restart
    goto :eof
)

if "%1%"=="clean" (
    docker-compose down -v
    echo All containers and volumes removed
    goto :eof
)

if "%1%"=="ps" (
    docker-compose ps
    goto :eof
)

if "%1%"=="shell-backend" (
    docker-compose exec backend /bin/bash
    goto :eof
)

if "%1%"=="shell-exchanger" (
    docker-compose exec exchanger /bin/bash
    goto :eof
)

if "%1%"=="shell-frontend" (
    docker-compose exec frontend /bin/sh
    goto :eof
)

echo Unknown command: %1%
echo Run "docker-help.bat" for usage information
