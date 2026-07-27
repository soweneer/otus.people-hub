FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8082

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build
COPY ["src/backend/PeopleHub.Feed/PeopleHub.Feed.csproj", "src/backend/PeopleHub.Feed/"]
COPY ["src/backend/PeopleHub.Application/PeopleHub.Application.csproj", "src/backend/PeopleHub.Application/"]
COPY ["src/backend/PeopleHub.Domain/PeopleHub.Domain.csproj", "src/backend/PeopleHub.Domain/"]
COPY ["src/backend/PeopleHub.Infrastructure/PeopleHub.Infrastructure.csproj", "src/backend/PeopleHub.Infrastructure/"]
RUN dotnet restore "src/backend/PeopleHub.Feed/PeopleHub.Feed.csproj"
COPY src/backend/ src/backend/
WORKDIR "/build/src/backend/PeopleHub.Feed"
RUN dotnet publish "PeopleHub.Feed.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PeopleHub.Feed.dll"]
