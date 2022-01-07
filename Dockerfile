FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY CountingBot.sln ./
COPY CountingBot ./CountingBot
COPY CountingBotData ./CountingBotData
COPY CountingBotLogic ./CountingBotLogic

RUN dotnet restore CountingBot.sln
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=build /app/CountingBot/bin/Release/net6.0/ .
ENTRYPOINT ["dotnet", "CountingBot.dll"]