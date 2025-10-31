FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MapServer/MapServer.csproj", "MapServer/"]
COPY ["MapLib/MapLib.csproj", "MapLib/"]
COPY ["ObjectsLib/ObjectsLib.csproj", "ObjectsLib/"]
COPY ["MapService/MapService.csproj", "MapService/"]

RUN dotnet restore "MapServer/MapServer.csproj"

COPY . .

WORKDIR "/src/MapServer"
RUN dotnet build "MapServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MapServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5000

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "MapServer.dll"]