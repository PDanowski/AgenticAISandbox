FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY . .
RUN dotnet publish ./sdlc-agent-app-dotnet/SdlcAgentApp.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/runtime:10.0-preview
WORKDIR /workspace
COPY --from=build /out .
COPY --from=build /src/.github /workspace/.github
COPY --from=build /src/Azure-and-dotnet-sdlc-agents /workspace/Azure-and-dotnet-sdlc-agents
COPY --from=build /src/GitHub-and-dotnet-sdlc-agents /workspace/GitHub-and-dotnet-sdlc-agents
ENTRYPOINT ["dotnet", "/workspace/SdlcAgentApp.dll"]
