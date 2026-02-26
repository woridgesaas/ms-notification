FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ms-notification.csproj", "./"]
RUN dotnet restore "./ms-notification.csproj"

COPY . .

# DEBUG TEMPORAIRE
RUN echo "==== LISTING FILES ===="
RUN find /src -type d
RUN find /src -type f

RUN dotnet publish "ms-notification.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ms-notification.dll"]