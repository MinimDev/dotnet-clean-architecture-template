# Publish Script
# Usage: .\publish.ps1 -ApiKey "your-nuget-api-key"

param(
    [Parameter(Mandatory = $true)]
    [string]$ApiKey
)

$Source = "https://api.nuget.org/v3/index.json"

Write-Host "🔨 Packing template..." -ForegroundColor Cyan
dotnet pack Minimdev.CleanArchitecture.Template.csproj -o .

if ($?) {
    $package = Get-ChildItem *.nupkg | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($package) {
        Write-Host "📦 Package: $($package.Name)" -ForegroundColor Yellow
        $confirm = Read-Host "Push to NuGet? (y/n)"
        if ($confirm -eq 'y') {
            Write-Host "🚀 Pushing to NuGet..." -ForegroundColor Cyan
            dotnet nuget push $package.FullName --source $Source --api-key $ApiKey
            if ($?) {
                Write-Host "✅ Published successfully!" -ForegroundColor Green
            }
            else {
                Write-Host "❌ Push failed." -ForegroundColor Red
            }
        }
        else {
            Write-Host "⏭️  Push cancelled." -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "❌ No .nupkg found." -ForegroundColor Red
    }
}
else {
    Write-Host "❌ Packing failed." -ForegroundColor Red
}
