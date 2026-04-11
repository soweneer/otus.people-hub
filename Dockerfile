FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build
COPY ["src/PeopleHub.Web/PeopleHub.Web.csproj", "src/PeopleHub.Web/"]
COPY ["src/PeopleHub.Lib/PeopleHub.Lib.csproj", "src/PeopleHub.Lib/"]
COPY ["src/PeopleHub.Dal/PeopleHub.Dal.csproj", "src/PeopleHub.Dal/"]
COPY ["src/PeopleHub.Security/PeopleHub.Security.csproj", "src/PeopleHub.Security/"]
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
