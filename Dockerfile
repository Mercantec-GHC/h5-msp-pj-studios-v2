FROM mcr.microsoft.com/dotnet/sdk:9.0 AS frontend-build
WORKDIR /src

COPY PJ-studios-v2/Frontend/Frontend.csproj PJ-studios-v2/Frontend/
RUN dotnet restore PJ-studios-v2/Frontend/Frontend.csproj

COPY PJ-studios-v2/Frontend/ PJ-studios-v2/Frontend/
WORKDIR /src/PJ-studios-v2/Frontend
RUN dotnet publish Frontend.csproj -c Release -o /app/frontend-publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /src

COPY PJ-studios-v2/Backend/Backend.csproj PJ-studios-v2/Backend/
RUN dotnet restore PJ-studios-v2/Backend/Backend.csproj

COPY PJ-studios-v2/Backend/ PJ-studios-v2/Backend/
WORKDIR /src/PJ-studios-v2/Backend
RUN dotnet publish Backend.csproj -c Release -o /app/backend-publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=backend-build /app/backend-publish ./
COPY --from=frontend-build /app/frontend-publish/wwwroot ./wwwroot

EXPOSE 8080
CMD ["dotnet", "Backend.dll"]
