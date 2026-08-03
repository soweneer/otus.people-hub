FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8083

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build
COPY ["src/backend/PeopleHub.Counters/PeopleHub.Counters.csproj", "src/backend/PeopleHub.Counters/"]
RUN dotnet restore "src/backend/PeopleHub.Counters/PeopleHub.Counters.csproj"
COPY src/backend/ src/backend/
WORKDIR "/build/src/backend/PeopleHub.Counters"
RUN dotnet publish "PeopleHub.Counters.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PeopleHub.Counters.dll"]
