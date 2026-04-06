FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY . .
RUN dotnet publish ./sdlc-agent-app-dotnet/SdlcAgentApp.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/runtime:10.0-preview
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "SdlcAgentApp.dll"]

