FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MapClient/MapClient.csproj", "MapClient/"]
COPY ["MapService/MapService.csproj", "MapService/"]

RUN dotnet restore "MapClient/MapClient.csproj"

COPY . .

WORKDIR "/src/MapClient"
RUN dotnet build "MapClient.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MapClient.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "MapClient.dll"]