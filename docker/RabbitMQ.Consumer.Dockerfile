FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build

COPY RabbitMQ.Consumer/RabbitMQ.Consumer.csproj /RabbitMQ.Consumer/
RUN dotnet restore /RabbitMQ.Consumer/RabbitMQ.Consumer.csproj

COPY RabbitMQ.Consumer /RabbitMQ.Consumer
RUN cd /RabbitMQ.Consumer && dotnet publish --no-restore -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS runtime
WORKDIR /RabbitMQ.Consumer
COPY --from=build /RabbitMQ.Consumer/out .
ENTRYPOINT ["dotnet", "./RabbitMQ.Consumer.dll"]