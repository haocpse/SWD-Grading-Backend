# ===============================
# BUILD STAGE
# ===============================
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
RUN dotnet publish "./SWD-Grading.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


# ===============================
# FINAL RUNTIME STAGE
# ===============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "SWD-Grading.dll"]