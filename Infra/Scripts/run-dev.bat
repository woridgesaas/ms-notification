@echo off
dotnet publish -c Release -o ./publish


set DOTNET_ENVIRONMENT=Development

dotnet ./publish/ms-notification.dll
