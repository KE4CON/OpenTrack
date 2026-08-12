# OpenTrack — container image for the Blazor web host.
# Multi-stage: build with the .NET SDK, run on the smaller ASP.NET runtime.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy everything and publish the web host (pulls in the UI/Infrastructure/Core project references).
COPY . .
RUN dotnet restore src/OpenTrack.Web/OpenTrack.Web.csproj
RUN dotnet publish src/OpenTrack.Web/OpenTrack.Web.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

# Listen on 8080 (plain HTTP — terminate TLS at a reverse proxy, or set OpenTrack:RequireHttps).
ENV ASPNETCORE_HTTP_PORTS=8080
# Keep the SQLite database (and scheduled backups) on a mounted volume so data survives container rebuilds.
ENV ConnectionStrings__Default="Data Source=/data/opentrack.db;Cache=Shared"
VOLUME /data
EXPOSE 8080

ENTRYPOINT ["dotnet", "OpenTrack.Web.dll"]
