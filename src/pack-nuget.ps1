$outputDirectory = Join-Path -Path (Get-Location) -ChildPath "NuGetPackages"
$nuspecFiles = @{
    "FlowLite.Abstractions.nuspec" = "package\FlowLite.Core.Abstractions"
    "FlowLite.nuspec" = "package\FlowLite.Core"
    "FlowLite.Diag.nuspec" = "tools\FlowLite.Diag"
    "FlowLite.Testing.nuspec" = "package\FlowLite.Testing"
    "FlowLite.Diagnostics.nuspec" = "package\FlowLite.Diagnostics"
}
# Create a nuget package directory
if (!(Test-Path -Path $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
}
foreach ($nuspecFile in $nuspecFiles.Keys) {
    $workingDir = $nuspecFiles[$nuspecFile]
    Write-Host "Packing NuGet package: $nuspecFile (Working Directory: $workingDir) ..." -ForegroundColor Cyan   
    Push-Location $workingDir
    
    $csproj = Get-ChildItem *.csproj | Select-Object -First 1
    if ($csproj) {
       Write-Host " -> Running dotnet build $($csproj.Name) -c Release" -ForegroundColor Gray
       dotnet build $csproj.FullName -c Release
    } else {
       Write-Host " -> No .csproj found, skipping build." -ForegroundColor DarkYellow
    }
    
    Write-Host " -> Packing NuGet package from $nuspecFile ..." -ForegroundColor Cyan
    $nugetPath = "C:\Tools\nuget.exe"
    $packCommand = "& `"$nugetPath`" pack $nuspecFile -OutputDirectory `"$outputDirectory`""
    Invoke-Expression $packCommand
    Pop-Location
    if ($?) {
        Write-Host "Successfully created package for $nuspecFile in $outputDirectory" -ForegroundColor Green
    } else {
        Write-Host "Failed to create package for $nuspecFile!" -ForegroundColor Red
    }
}
Write-Host "Packing process completed!" -ForegroundColor Cyan