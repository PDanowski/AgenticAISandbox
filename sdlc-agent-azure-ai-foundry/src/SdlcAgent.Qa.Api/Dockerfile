FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY ["src/SdlcAgent.Qa.Api/SdlcAgent.Qa.Api.csproj", "src/SdlcAgent.Qa.Api/"]
COPY ["src/SdlcAgent.Shared.Contracts/SdlcAgent.Shared.Contracts.csproj", "src/SdlcAgent.Shared.Contracts/"]
RUN dotnet restore "src/SdlcAgent.Qa.Api/SdlcAgent.Qa.Api.csproj"

COPY . .
RUN dotnet publish "src/SdlcAgent.Qa.Api/SdlcAgent.Qa.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SdlcAgent.Qa.Api.dll"]
