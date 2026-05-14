FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Web/RudFitAI.Web.csproj", "Web/"]
COPY ["Application/RudFitAI.Application.csproj", "Application/"]
COPY ["Infrastructure/RudFitAI.Infrastructure.csproj", "Infrastructure/"]
COPY ["Domain/RudFitAI.Domain.csproj", "Domain/"]

RUN dotnet restore "Web/RudFitAI.Web.csproj"

COPY . .
RUN dotnet publish "Web/RudFitAI.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "RudFitAI.Web.dll"]
