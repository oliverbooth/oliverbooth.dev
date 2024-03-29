FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM node:alpine as build-deps
WORKDIR /src
COPY package.json package-lock.json ./
RUN npm i -g gulp-cli
RUN npm i
COPY . ./
RUN gulp

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
COPY --from=build-deps /src/OliverBooth/wwwroot /src/OliverBooth/wwwroot
WORKDIR /src
COPY ["OliverBooth/OliverBooth.csproj", "OliverBooth/"]
RUN dotnet restore "OliverBooth/OliverBooth.csproj"
COPY . .

WORKDIR "/src/OliverBooth"
RUN dotnet build "OliverBooth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OliverBooth.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OliverBooth.dll"]
