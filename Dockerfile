FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ms-notification.csproj", "./"]
RUN dotnet restore "./ms-notification.csproj"

COPY . .
RUN ls -la
#Compile the project
RUN dotnet publish "ms-notification.csproj" -c Release -o /app/publish
#the contaienr will be destroyed now

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
#use previous container to copy the compiled project
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ms-notification.dll"]
