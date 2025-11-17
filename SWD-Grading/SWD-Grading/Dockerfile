FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SWD-Grading/SWD-Grading.csproj", "SWD-Grading/"]
COPY ["BLL/BLL.csproj", "BLL/"]
COPY ["DAL/DAL.csproj", "DAL/"]
COPY ["Model/Model.csproj", "Model/"]
RUN dotnet restore "SWD-Grading/SWD-Grading.csproj"
COPY . .
WORKDIR "/src/SWD-Grading"
RUN dotnet build "./SWD-Grading.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SWD-Grading.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Install Tesseract OCR and Leptonica dependencies
RUN apt-get update && apt-get install -y \
    tesseract-ocr \
    libtesseract-dev \
    libleptonica-dev \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=publish /app/publish .

# Copy tessdata directory for OCR
COPY SWD-Grading/tessdata ./tessdata

# Set environment to Production
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "SWD-Grading.dll"]
