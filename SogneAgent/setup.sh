#!/bin/bash
# Setup script for new developers (Linux/Mac)

if [ -f "appsettings.json" ]; then
    echo "appsettings.json already exists. Skipping..."
else
    cp appsettings.template.json appsettings.json
    echo "Created appsettings.json from template."
    echo "Please edit appsettings.json and add your OpenAI API key."
fi

echo ""
echo "Restoring dependencies..."
dotnet restore

echo ""
echo "Setup complete! Edit appsettings.json with your API key, then run: dotnet run"
