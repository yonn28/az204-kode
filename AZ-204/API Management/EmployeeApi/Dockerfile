# Step 1: Use the ASP.NET Core runtime image as the base for the final container
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 8080

# Step 2: Use the .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the .csproj file and restore any dependencies (via `dotnet restore`)
COPY ["EmployeeApi.csproj", "./"]
RUN dotnet restore "EmployeeApi.csproj"

# Copy the rest of the source code
COPY . . 
WORKDIR "/src/"

# Build the application in Release mode
RUN dotnet build "EmployeeApi.csproj" -c Release -o /app/build

# Step 3: Publish the application
FROM build AS publish
RUN dotnet publish "EmployeeApi.csproj" -c Release -o /app/publish

# Step 4: Set the base image and copy the published app to the final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set the entry point to the application
ENTRYPOINT ["dotnet", "EmployeeApi.dll"]