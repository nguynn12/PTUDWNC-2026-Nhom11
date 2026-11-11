# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Sao chép các tệp props và solution
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["CulinaryBlog.slnx", "./"]

# Sao chép csproj của từng layer để tận dụng Docker layer caching
COPY ["src/CulinaryBlog.Domain/CulinaryBlog.Domain.csproj", "src/CulinaryBlog.Domain/"]
COPY ["src/CulinaryBlog.Application/CulinaryBlog.Application.csproj", "src/CulinaryBlog.Application/"]
COPY ["src/CulinaryBlog.Infrastructure/CulinaryBlog.Infrastructure.csproj", "src/CulinaryBlog.Infrastructure/"]
COPY ["src/CulinaryBlog.API/CulinaryBlog.API.csproj", "src/CulinaryBlog.API/"]

RUN dotnet restore "src/CulinaryBlog.API/CulinaryBlog.API.csproj"

# Sao chép toàn bộ mã nguồn backend
COPY . .
WORKDIR "/src/src/CulinaryBlog.API"
RUN dotnet publish "CulinaryBlog.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Chạy với user non-root bảo mật theo NFR-SEC
USER app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CulinaryBlog.API.dll"]
