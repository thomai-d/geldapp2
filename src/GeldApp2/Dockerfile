FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
RUN apt-get update && apt-get install -y curl
RUN curl -sL https://deb.nodesource.com/setup_10.x | bash -
RUN apt-get install -y nodejs

COPY ["GeldApp2/GeldApp2.csproj", "GeldApp2/"]
RUN dotnet restore "GeldApp2/GeldApp2.csproj"
COPY . .
WORKDIR "/src/GeldApp2"
RUN dotnet build "GeldApp2.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "GeldApp2.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "GeldApp2.dll"]
