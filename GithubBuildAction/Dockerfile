#FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
#WORKDIR /app
#
#FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
#WORKDIR /src
#COPY ["GithubBuildAction/GithubBuildAction.csproj", "GithubBuildAction/"]
#COPY ["BuildUtils/BuildUtils.csproj", "BuildUtils/"]
#RUN dotnet restore "GithubBuildAction/GithubBuildAction.csproj"
#COPY . .
#WORKDIR "/src/GithubBuildAction"
#RUN dotnet build "GithubBuildAction.csproj" -c Release -o /app/build
#
#FROM build AS publish
#RUN dotnet publish "GithubBuildAction.csproj" -c Release -o /app/publish
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "GithubBuildAction.dll"]
# Set the base image as the .NET 7.0 SDK (this includes the runtime)
FROM mcr.microsoft.com/dotnet/sdk:7.0 as build-env

# Copy everything and publish the release (publish implicitly restores and builds)
WORKDIR /app
COPY . ./
RUN dotnet publish "GithubBuildAction.csproj" -c Release -o out --no-self-contained

# Label the container
LABEL maintainer="Verint <verint.com>"
#LABEL repository="https://github.com/dotnet/samples"
#LABEL homepage="https://github.com/dotnet/samples"

# Label as GitHub action
LABEL com.github.actions.name="Community Build Utils"
# Limit to 160 characters
LABEL com.github.actions.description="The description of your GitHub Action."
# See branding:
# https://docs.github.com/actions/creating-actions/metadata-syntax-for-github-actions#branding
LABEL com.github.actions.icon="activity"
LABEL com.github.actions.color="orange"

# Relayer the .NET SDK, anew with the build output
FROM mcr.microsoft.com/dotnet/sdk:7.0
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "GithubBuildAction.dll"]