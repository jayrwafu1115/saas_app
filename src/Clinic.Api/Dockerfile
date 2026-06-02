FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ClinicManagementSaaS.sln NuGet.Config ./
COPY src/Clinic.Api/Clinic.Api.csproj src/Clinic.Api/
COPY src/Clinic.Application/Clinic.Application.csproj src/Clinic.Application/
COPY src/Clinic.Domain/Clinic.Domain.csproj src/Clinic.Domain/
COPY src/Clinic.Infrastructure/Clinic.Infrastructure.csproj src/Clinic.Infrastructure/
RUN dotnet restore src/Clinic.Api/Clinic.Api.csproj

COPY src/ src/
RUN dotnet publish src/Clinic.Api/Clinic.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Clinic.Api.dll"]
