FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build
COPY ["src/PeopleHub.Web/PeopleHub.Web.csproj", "src/PeopleHub.Web/"]
COPY ["src/PeopleHub.Domain/PeopleHub.Domain.csproj", "src/PeopleHub.Domain/"]
COPY ["src/PeopleHub.Infrastructure/PeopleHub.Infrastructure.csproj", "src/PeopleHub.Infrastructure/"]
COPY ["src/PeopleHub.Shared/PeopleHub.Shared.csproj", "src/PeopleHub.Shared/"]
RUN dotnet restore "src/PeopleHub.Web/PeopleHub.Web.csproj"
COPY . .
WORKDIR "/build/src/PeopleHub.Web"
RUN dotnet build "PeopleHub.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PeopleHub.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PeopleHub.Web.dll"]
