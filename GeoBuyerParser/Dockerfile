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

# Install necessary dependencies for running ChromeDriver and Chrome
RUN apt-get update && apt-get install -y \
    wget \
    unzip \
    libgdiplus \
    libxshmfence1 \
    libfontconfig1 \
    libxrender1 \
    libfreetype6 \
    libxext6 \
    libx11-6 \
    libxcb1 \
    libxi6 \
    libnss3 \
    libatk-bridge2.0-0 \
    libgtk-3-0 \
    libgbm1

# Download and install ChromeDriver
RUN wget -q -O /tmp/chromedriver.zip https://chromedriver.storage.googleapis.com/103.0.5060.53/chromedriver_linux64.zip \
    && unzip /tmp/chromedriver.zip -d /usr/local/bin \
    && rm /tmp/chromedriver.zip

# Expose the ports that your ASP.NET Core application listens on (e.g., 8080 and 8081)
EXPOSE 8080 8081

# Specify the entry point to run your application
CMD ["dotnet", "GeoBuyerParser.dll"]
