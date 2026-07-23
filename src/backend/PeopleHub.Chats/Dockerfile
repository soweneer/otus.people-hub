FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build
COPY ["src/backend/PeopleHub.Chats/PeopleHub.Chats.csproj", "src/backend/PeopleHub.Chats/"]
RUN dotnet restore "src/backend/PeopleHub.Chats/PeopleHub.Chats.csproj"
COPY src/backend/PeopleHub.Chats/ src/backend/PeopleHub.Chats/
WORKDIR "/build/src/backend/PeopleHub.Chats"
RUN dotnet publish "PeopleHub.Chats.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PeopleHub.Chats.dll"]
