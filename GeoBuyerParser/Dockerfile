# Use a Windows Server Core-based image
FROM mcr.microsoft.com/windows/servercore:ltsc2019

# Create a directory for your application
WORKDIR C:\\app

# Copy the necessary files into the container
COPY . .

# Set the working directory for your application
WORKDIR C:\\app

# Add an ENTRYPOINT to specify the command to run when the container starts
ENTRYPOINT ["dotnet", "GeoBuyerParser.dll"]