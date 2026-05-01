FROM mcr.microsoft.com/dotnet/sdk:10.0 as build

WORKDIR /app

# Copy project files
COPY Projekt-RSI-1-BackEnd/Projekt-RSI-1-BackEnd.csproj ./

# Restore dependencies
RUN dotnet restore

# Copy all files
COPY Projekt-RSI-1-BackEnd/ ./

# Build
RUN dotnet build -c Release -o /app/build

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

# Copy build artifacts
COPY --from=build /app/build .

# Install certificates for HTTPS
RUN apt-get update && apt-get install -y openssl && rm -rf /var/lib/apt/lists/*

# Generate self-signed certificate for development
RUN openssl req -x509 -newkey rsa:4096 -keyout aspnetapp.key -out aspnetapp.crt -days 365 -nodes -subj "/CN=localhost"
RUN openssl pkcs12 -export -in aspnetapp.crt -inkey aspnetapp.key -out aspnetapp.pfx -passout pass:

ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/app/aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=

EXPOSE 8181

ENTRYPOINT ["dotnet", "Projekt-RSI-1-BackEnd.dll"]
