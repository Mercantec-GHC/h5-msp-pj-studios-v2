FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY PJ-studios-v2/Frontend/Frontend.csproj PJ-studios-v2/Frontend/
RUN dotnet restore PJ-studios-v2/Frontend/Frontend.csproj

COPY PJ-studios-v2/Frontend/ PJ-studios-v2/Frontend/
WORKDIR /src/PJ-studios-v2/Frontend
RUN dotnet publish Frontend.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM nginx:1.27-alpine
ENV PORT=8080

WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot/ ./
COPY nginx/default.conf.template /etc/nginx/templates/default.conf.template

EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]
