FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["samples/FlowLite.Console/FlowLite.Console.csproj", "samples/FlowLite.Console/"]
RUN dotnet restore "samples/FlowLite.Console/FlowLite.Console.csproj"
COPY . .
WORKDIR "/src/samples/FlowLite.Console"
RUN dotnet build "FlowLite.Console.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FlowLite.Console.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FlowLite.Console.dll"]
