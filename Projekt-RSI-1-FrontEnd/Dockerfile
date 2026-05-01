FROM node:22-alpine

WORKDIR /app

# Install pnpm
RUN npm install -g pnpm@10.33.0

# Copy package files
COPY package.json pnpm-lock.yaml ./

# Install dependencies
RUN pnpm install --frozen-lockfile

# Copy project files
COPY . .

# Build
RUN pnpm run build

EXPOSE 3000

CMD ["pnpm", "run", "preview"]
