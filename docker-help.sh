#!/bin/bash
# Docker helper script for Projekt RSI-1

if [ -z "$1" ]; then
    echo ""
    echo "Usage: ./docker-help.sh [command]"
    echo ""
    echo "Commands:"
    echo "  up              - Start all services"
    echo "  down            - Stop all services"
    echo "  logs            - Show logs from all services"
    echo "  logs-backend    - Show backend logs"
    echo "  logs-exchanger  - Show exchanger logs"
    echo "  logs-frontend   - Show frontend logs"
    echo "  logs-db         - Show database logs"
    echo "  rebuild         - Rebuild all images"
    echo "  restart         - Restart all services"
    echo "  clean           - Remove all containers and volumes"
    echo "  ps              - Show running containers"
    echo "  shell-backend   - Access backend container shell"
    echo "  shell-exchanger - Access exchanger container shell"
    echo "  shell-frontend  - Access frontend container shell"
    echo ""
    exit 0
fi

case "$1" in
    up)
        docker-compose up -d
        sleep 2
        docker-compose ps
        ;;
    down)
        docker-compose down
        ;;
    logs)
        docker-compose logs -f
        ;;
    logs-backend)
        docker-compose logs -f backend
        ;;
    logs-exchanger)
        docker-compose logs -f exchanger
        ;;
    logs-frontend)
        docker-compose logs -f frontend
        ;;
    logs-db)
        docker-compose logs -f mssql
        ;;
    rebuild)
        docker-compose build --no-cache
        ;;
    restart)
        docker-compose restart
        ;;
    clean)
        docker-compose down -v
        echo "All containers and volumes removed"
        ;;
    ps)
        docker-compose ps
        ;;
    shell-backend)
        docker-compose exec backend /bin/bash
        ;;
    shell-exchanger)
        docker-compose exec exchanger /bin/bash
        ;;
    shell-frontend)
        docker-compose exec frontend /bin/sh
        ;;
    *)
        echo "Unknown command: $1"
        echo "Run './docker-help.sh' for usage information"
        ;;
esac
