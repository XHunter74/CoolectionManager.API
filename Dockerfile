FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
ARG PROJECT_NAME
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG PROJECT_NAME
WORKDIR /src
COPY . . 
WORKDIR "/src/$PROJECT_NAME"
RUN dotnet restore "$PROJECT_NAME.csproj"
RUN dotnet build "$PROJECT_NAME.csproj" --no-restore -c Release -o /app/build
RUN if [ -f start-tests.sh ]; then \
        chmod +x start-tests.sh; \ 
        ./start-tests.sh; \
    fi

FROM build AS publish
ARG PROJECT_NAME
WORKDIR "/src/$PROJECT_NAME"
RUN dotnet publish "$PROJECT_NAME.csproj" --no-restore -c Release -o /app/publish
COPY CollectionManager.API/appsettings.* /app/publish/

FROM base AS final
ARG PROJECT_NAME
ENV EXE_NAME=${PROJECT_NAME}.dll
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT dotnet $EXE_NAME
# ENTRYPOINT ["tail", "-f", "/dev/null"]
