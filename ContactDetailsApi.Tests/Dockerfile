FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ENV DynamoDb_LocalMode='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./ContactDetailsApi.sln ./
COPY ./ContactDetailsApi/ContactDetailsApi.csproj ./ContactDetailsApi/
COPY ./ContactDetailsApi.Tests/ContactDetailsApi.Tests.csproj ./ContactDetailsApi.Tests/

RUN dotnet restore ./ContactDetailsApi/ContactDetailsApi.csproj
RUN dotnet restore ./ContactDetailsApi.Tests/ContactDetailsApi.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out ContactDetailsApi.Tests/ContactDetailsApi.Tests.csproj

CMD dotnet test
