FROM mcr.microsoft.com/dotnet/sdk:6.0

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ENV DynamoDb_LocalMode='true'
ENV Sns_LocalMode="true"

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN

ARG SONAR_TOKEN
ENV SONAR_TOKEN=$SONAR_TOKEN

WORKDIR /app

# Install and run sonar cloud scanner
RUN apt-get update && apt-get install -y openjdk-17-jdk
RUN dotnet tool install --global dotnet-sonarscanner --version 5.6.0
ENV PATH="$PATH:/root/.dotnet/tools"

RUN dotnet sonarscanner begin /k:"LBHackney-IT_contact-details-api" /o:"lbhackney-it" /d:sonar.host.url=https://sonarcloud.io /d:sonar.login="${SONAR_TOKEN}"

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
RUN dotnet sonarscanner end /d:sonar.login="${SONAR_TOKEN}"
