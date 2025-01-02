FROM mcr.microsoft.com/dotnet/sdk:8.0

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ENV DynamoDb_LocalMode='true'

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN

WORKDIR /app

# Copy csproj and nuget config and restore as distinct layers
COPY ./ContactDetailsApi.sln ./
COPY ./ContactDetailsApi/ContactDetailsApi.csproj ./ContactDetailsApi/
COPY ./ContactDetailsApi.Tests/ContactDetailsApi.Tests.csproj ./ContactDetailsApi.Tests/
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./ContactDetailsApi/ContactDetailsApi.csproj
RUN dotnet restore ./ContactDetailsApi.Tests/ContactDetailsApi.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c Release -o out ContactDetailsApi/ContactDetailsApi.csproj
RUN dotnet build -c debug -o out ContactDetailsApi.Tests/ContactDetailsApi.Tests.csproj

CMD dotnet test
