#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["API.Scrapping/API.Scrapping.csproj", "API.Scrapping/"]
COPY ["Web.Domain/Web.Domain.csproj", "Web.Domain/"]
COPY ["Web.Persistance/Web.Persistance.csproj", "Web.Persistance/"]
RUN dotnet restore "API.Scrapping/API.Scrapping.csproj"
COPY . .
WORKDIR "/src/API.Scrapping"
RUN dotnet build "API.Scrapping.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "API.Scrapping.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.Scrapping.dll"]