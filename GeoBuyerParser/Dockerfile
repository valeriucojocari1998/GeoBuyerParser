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

# Install Chrome and ChromeDriver
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
    libxi6

RUN wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
    && echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google-chrome.list \
    && apt-get update && apt-get install -y google-chrome-stable

# Download and install ChromeDriver
RUN CHROME_DRIVER_VERSION=`curl -sS https://chromedriver.storage.googleapis.com/LATEST_RELEASE` \
    && wget -q -O /tmp/chromedriver.zip https://chromedriver.storage.googleapis.com/$CHROME_DRIVER_VERSION/chromedriver_linux64.zip \
    && unzip /tmp/chromedriver.zip -d /usr/local/bin \
    && rm /tmp/chromedriver.zip

# Expose the port that your ASP.NET Core application listens on (e.g., 80)
EXPOSE 8081

# Specify the entry point to run your application
ENTRYPOINT ["dotnet", "GeoBuyerParser.dll"]
