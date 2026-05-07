FROM node:22-alpine
WORKDIR /app

RUN corepack enable

# Kopiujemy pliki zależności
COPY package.json pnpm-lock.yaml ./

# Instalujemy zależności
RUN pnpm i

# Kopiujemy resztę plików projektu (będą potrzebne do działania w trybie dev)
COPY . ./

# Nuxt domyślnie odpali się na porcie 3000 w trybie dev
ENV PORT=3000
ENV HOST=0.0.0.0

EXPOSE 3000

# Uruchamiamy tryb deweloperski
CMD ["pnpm", "run", "dev"]