FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["RelieftManagement.Crawler.csproj", "./"]
RUN dotnet restore "RelieftManagement.Crawler.csproj"

COPY . .
RUN dotnet publish "RelieftManagement.Crawler.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y \
    chromium \
    fonts-liberation \
    fonts-noto-cjk \
    fonts-freefont-ttf \
    libatk-bridge2.0-0 \
    libgtk-3-0 \
    libnss3 \
    libxss1 \
    libasound2 \
    libgbm1 \
    libxshmfence1 \
    libdrm2 \
    libxkbcommon0 \
    --no-install-recommends \
    && rm -rf /var/lib/apt/lists/* \
    && apt-get clean

ENV PUPPETEER_EXECUTABLE_PATH=/usr/bin/chromium
ENV PUPPETEER_SKIP_CHROMIUM_DOWNLOAD=true

# Create non-root user for security
RUN useradd --create-home --shell /bin/bash crawler
USER crawler

# Copy published app
COPY --from=build --chown=crawler:crawler /app/publish .

# Entry point - NOTE: DLL name matches project name
ENTRYPOINT ["dotnet", "RelieftManagement.Crawler.dll"]
