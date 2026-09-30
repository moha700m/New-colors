$ErrorActionPreference = "Stop"
dotnet restore
dotnet build -c Release --no-restore
