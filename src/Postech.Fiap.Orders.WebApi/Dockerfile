FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Postech.Fiap.Orders.WebApi/Postech.Fiap.Orders.WebApi.csproj", "src/Postech.Fiap.Orders.WebApi/"]
RUN dotnet restore "src/Postech.Fiap.Orders.WebApi/Postech.Fiap.Orders.WebApi.csproj"
COPY . .
WORKDIR "/src/src/Postech.Fiap.Orders.WebApi"
RUN dotnet build "Postech.Fiap.Orders.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Postech.Fiap.Orders.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Postech.Fiap.Orders.WebApi.dll"]