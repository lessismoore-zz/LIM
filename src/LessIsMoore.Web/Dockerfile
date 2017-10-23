FROM microsoft/aspnetcore:1.1
ARG source
WORKDIR /app
EXPOSE 80
COPY . .
ENTRYPOINT ["dotnet", "LessIsMoore.Web.dll"]
