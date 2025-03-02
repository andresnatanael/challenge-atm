# Use official .NET 9 SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and project files
COPY . ./

# Restore dependencies
RUN dotnet restore

# Build and publish the application
RUN dotnet publish AtmChallenge.Api/AtmChallenge.Api.csproj -c Release -o /out --no-restore

# Use the official .NET runtime image for execution
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /out .

# 🔹 Ensure the certificates folder exists
RUN mkdir -p /app/certificates

# 🔹 Copy default RSA keys into the container
COPY certificates/public_key.pem /app/certificates/public_key.pem
COPY certificates/private_key.pem /app/certificates/private_key.pem

# Expose API ports
EXPOSE 8080
EXPOSE 8081

# Run the API
ENTRYPOINT ["dotnet", "AtmChallenge.Api.dll"]