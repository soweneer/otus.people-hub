FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM node:22-alpine AS client-build
WORKDIR /src/frontend
COPY ["src/frontend/package.json", "src/frontend/package-lock.json", "./"]
RUN npm ci --no-fund --no-audit
COPY src/frontend/ ./
# vite build кладёт результат в ../backend/PeopleHub.Web/wwwroot (см. vite.config.ts)
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build
COPY ["src/backend/PeopleHub.Web/PeopleHub.Web.csproj", "src/backend/PeopleHub.Web/"]
COPY ["src/backend/PeopleHub.Application/PeopleHub.Application.csproj", "src/backend/PeopleHub.Application/"]
COPY ["src/backend/PeopleHub.Domain/PeopleHub.Domain.csproj", "src/backend/PeopleHub.Domain/"]
COPY ["src/backend/PeopleHub.Infrastructure/PeopleHub.Infrastructure.csproj", "src/backend/PeopleHub.Infrastructure/"]
RUN dotnet restore "src/backend/PeopleHub.Web/PeopleHub.Web.csproj"
COPY . .
WORKDIR "/build/src/backend/PeopleHub.Web"
RUN dotnet build "PeopleHub.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PeopleHub.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=client-build /src/backend/PeopleHub.Web/wwwroot ./wwwroot
ENTRYPOINT ["dotnet", "PeopleHub.Web.dll"]
