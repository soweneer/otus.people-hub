FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM node:22-alpine AS client-build
WORKDIR /client
COPY ["src/PeopleHub.Web/ClientApp/package.json", "src/PeopleHub.Web/ClientApp/package-lock.json", "./"]
RUN npm ci --no-fund --no-audit
COPY src/PeopleHub.Web/ClientApp/ ./
# vite build кладёт результат в ../wwwroot (см. vite.config.ts) -> /wwwroot
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build
COPY ["src/PeopleHub.Web/PeopleHub.Web.csproj", "src/PeopleHub.Web/"]
COPY ["src/PeopleHub.Domain/PeopleHub.Domain.csproj", "src/PeopleHub.Domain/"]
COPY ["src/PeopleHub.Infrastructure/PeopleHub.Infrastructure.csproj", "src/PeopleHub.Infrastructure/"]
RUN dotnet restore "src/PeopleHub.Web/PeopleHub.Web.csproj"
COPY . .
WORKDIR "/build/src/PeopleHub.Web"
RUN dotnet build "PeopleHub.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PeopleHub.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=client-build /wwwroot ./wwwroot
ENTRYPOINT ["dotnet", "PeopleHub.Web.dll"]
