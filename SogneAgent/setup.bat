@echo off
REM Setup script for new developers

if exist appsettings.json (
    echo appsettings.json already exists. Skipping...
) else (
    copy appsettings.template.json appsettings.json
    echo Created appsettings.json from template.
    echo Please edit appsettings.json and add your OpenAI API key.
)

echo.
echo Restoring dependencies...
dotnet restore

echo.
echo Setup complete! Edit appsettings.json with your API key, then run: dotnet run
