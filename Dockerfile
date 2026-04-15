# -- Stage 1 : Build -----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet publish src/KairuFocus.Api/KairuFocus.Api.csproj \
      -c Release -r linux-x64 --self-contained false \
      -o /app/api

RUN dotnet publish src/KairuFocus.Web/KairuFocus.Web.csproj \
      -c Release \
      -o /app/web

# Copier Blazor wwwroot dans l'API (meme pattern que deploy-linux.ps1)
RUN cp -r /app/web/wwwroot /app/api/wwwroot

# -- Stage 2 : Runtime ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/api .

ENV HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "KairuFocus.Api.dll"]
