param(
    [string]$RepoPath = (Get-Location).Path
)

Set-Location $RepoPath

Write-Host "Atualizando repositorio..."
git pull

Write-Host "Subindo containers com build..."
docker compose up -d --build

Write-Host "Pronto. Swagger em http://localhost:8080/swagger"