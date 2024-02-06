FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ./DBG.MSSQLWorker ./DBG.MSSQLWorker
COPY ./DBG.Infrastructure ./DBG.Infrastructure
RUN dotnet publish -o /app DBG.MSSQLWorker -c Release

FROM mcr.microsoft.com/dotnet/aspnet:7.0
EXPOSE 80
WORKDIR /app
COPY --from=build /app ./
COPY ./DBG.MSSQLWorker/s.sh .