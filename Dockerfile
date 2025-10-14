# ===== Build Stage =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy all files and publish
COPY . ./
RUN dotnet publish -c Release -o out

# ===== Runtime Stage =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/out ./

# Set Render environment port
ENV PORT=8080
EXPOSE $PORT

ENV ASPNETCORE_URLS=http://+:8080
# Run the application
ENTRYPOINT ["dotnet", "BillingAPI.dll"]
