# ==========================================
# Giai đoạn 1: Base - Chạy ứng dụng (Runtime)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
# Render cần biết container chạy ở port nào. .NET 8+ mặc định là 8080.
EXPOSE 8080
EXPOSE 8081

# ==========================================
# Giai đoạn 2: Build - Biên dịch code
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Cậu cần copy tất cả các file .csproj vào đúng thư mục để phục hồi thư viện (Restore)
# Sửa lại tên các dự án (.csproj) cho đúng với cấu trúc thư mục của cậu nhé:
COPY ["SmartCarWash.Api/SmartCarWash.Api.csproj", "SmartCarWash.Api/"]
COPY ["SmartCarWash.Application/SmartCarWash.Application.csproj", "SmartCarWash.Application/"]
COPY ["SmartCarWash.Domain/SmartCarWash.Domain.csproj", "SmartCarWash.Domain/"]
COPY ["SmartCarWash.Infrastructure/SmartCarWash.Infrastructure.csproj", "SmartCarWash.Infrastructure/"]

# Khôi phục các gói NuGet
RUN dotnet restore "SmartCarWash.Api/SmartCarWash.Api.csproj"

# Copy toàn bộ code còn lại vào container
COPY . .
WORKDIR "/src/SmartCarWash.Api"
RUN dotnet build "SmartCarWash.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ==========================================
# Giai đoạn 3: Publish - Đóng gói ứng dụng sạch sẽ
# ==========================================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SmartCarWash.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ==========================================
# Giai đoạn 4: Final - Chạy ứng dụng từ bản Publish
# ==========================================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartCarWash.Api.dll"]