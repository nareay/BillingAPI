# ===== Build Stage =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copy csproj and restore
COPY *.csproj ./
RUN dotnet restore

# Copy everything else
COPY . ./

# Publish
RUN dotnet publish -c Release -o out

# ===== Runtime Stage =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Set Render port
ENV ASPNETCORE_URLS=http://+:8080

# Copy published files
COPY --from=build /app/out ./

# Copy Excel template into the runtime container
COPY TaxInvoiceFormat.xlsx ./

# Expose port 8080
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "BillingAPI.dll"]
