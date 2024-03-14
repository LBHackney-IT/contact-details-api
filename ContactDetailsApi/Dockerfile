FROM mcr.microsoft.com/dotnet/sdk:6.0

ENV DynamoDb_LocalMode='true'

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN

WORKDIR /app

# Copy csproj and nuget config and restore as distinct layers
COPY ./ContactDetailsApi/ContactDetailsApi.csproj ./
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./ContactDetailsApi.csproj

# Copy everything else and build
COPY . ./
RUN dotnet build -c Release -o out ./ContactDetailsApi.csproj
RUN dotnet publish -c Release -o out ./ContactDetailsApi.csproj

EXPOSE ${PORT:-3000}
CMD ASPNETCORE_URLS=http://+:${PORT:-3000} dotnet ./out/ContactDetailsApi.dll
