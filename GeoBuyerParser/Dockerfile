# Use the official .NET SDK image as the base image for building your application
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy the .csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY . ./
RUN dotnet publish -c Release -o out

# Build the runtime image using the ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Expose the port that your ASP.NET Core application listens on (e.g., 80)
EXPOSE 8081

# Specify the entry point to run your application
ENTRYPOINT ["dotnet", "GeoBuyerParser.dll"]
