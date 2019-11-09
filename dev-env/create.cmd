docker-compose up -d
if errorlevel 1 (
pause
)

:again
timeout /t 3
docker exec -it mysql /backup/restore.sh
if errorlevel 1 (
goto again
)