@echo off
echo Starting PostgreSQL database container...

REM Run docker-compose in detached (background) mode
docker-compose down -v

docker-compose up -d

echo.
echo Database container is starting.
echo You can now run the .NET application.