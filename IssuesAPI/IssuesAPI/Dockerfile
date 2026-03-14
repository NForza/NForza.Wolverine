FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files for restore
COPY Wolverine.slnx ./
COPY Wolverine.ValueTypes/src/NForza.Wolverine.ValueTypes.csproj Wolverine.ValueTypes/src/
COPY Wolverine.Generators/NForza.Wolverine.Generators.csproj Wolverine.Generators/
COPY Wolverine.ValueTypes/Tests/NForza.Wolverine.ValueTypes.Tests.csproj Wolverine.ValueTypes/Tests/
COPY IssuesAPI/IssuesAPI/Wolverine.Issues.csproj IssuesAPI/IssuesAPI/
COPY IssuesAPI/IssuesAPI.Contracts/Wolverine.Issues.Contracts.csproj IssuesAPI/IssuesAPI.Contracts/
COPY IssuesAPI/IssuesAPI.Tests/Wolverine.Issues.Tests.csproj IssuesAPI/IssuesAPI.Tests/
COPY IssuesAPI/IssuesAPI.TypeScriptGeneration/Wolverine.Issues.TypeScriptGeneration.csproj IssuesAPI/IssuesAPI.TypeScriptGeneration/
COPY IssuesAPI.Reporting/IssuesAPI.Reporting/Wolverine.Reporting.csproj IssuesAPI.Reporting/IssuesAPI.Reporting/
COPY IssuesAPI.Reporting/IssuesAPI.Reporting.Tests/Wolverine.Reporting.Tests.csproj IssuesAPI.Reporting/IssuesAPI.Reporting.Tests/

RUN dotnet restore Wolverine.slnx

# Copy source and build
COPY . .
RUN dotnet publish IssuesAPI/IssuesAPI/Wolverine.Issues.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Wolverine.Issues.dll"]
