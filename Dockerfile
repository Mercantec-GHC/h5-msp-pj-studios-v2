FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Backend
COPY PJ-studios-v2/Backend/Backend.csproj PJ-studios-v2/Backend/
RUN dotnet restore PJ-studios-v2/Backend/Backend.csproj

# Frontend
COPY PJ-studios-v2/Frontend/Frontend.csproj PJ-studios-v2/Frontend/
RUN dotnet restore PJ-studios-v2/Frontend/Frontend.csproj

# Copy all source
COPY PJ-studios-v2/Backend/ PJ-studios-v2/Backend/
COPY PJ-studios-v2/Frontend/ PJ-studios-v2/Frontend/

# Publish backend
RUN dotnet publish PJ-studios-v2/Backend/Backend.csproj -c Release -o /app/backend-publish /p:UseAppHost=false

# Publish frontend
RUN dotnet publish PJ-studios-v2/Frontend/Frontend.csproj -c Release -o /app/frontend-publish /p:UseAppHost=false

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy backend
COPY --from=build /app/backend-publish ./

# Copy frontend (entire wwwroot including _framework + ICU files)
COPY --from=build /app/frontend-publish/wwwroot ./wwwroot

EXPOSE 8080
CMD ["dotnet", "Backend.dll"]
