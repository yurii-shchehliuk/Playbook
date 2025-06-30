@echo off
REM Build the solution

echo Building the solution...
dotnet build WebScraping.sln

REM Change directory to API.Scrapping and run the app
cd API.Scrapping
echo Starting the API.Scrapping application...
dotnet run

pause