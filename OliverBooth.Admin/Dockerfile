FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OliverBooth.Admin/OliverBooth.Admin.csproj", "OliverBooth.Admin/"]
COPY . .

WORKDIR "/src/OliverBooth.Admin"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OliverBooth.Admin.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OliverBooth.Admin.dll"]
