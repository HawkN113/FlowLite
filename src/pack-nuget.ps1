$outputDirectory = Join-Path -Path (Get-Location) -ChildPath "NuGetPackages"
$nuspecFiles = @{
    "FlowLite.nuspec" = "package\FlowLite"
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
    $packCommand = "dotnet pack -p:NuspecFile=$nuspecFile --output `"$outputDirectory`""
    Invoke-Expression $packCommand
    Pop-Location
    if ($?) {
        Write-Host "Successfully created package for $nuspecFile in $outputDirectory" -ForegroundColor Green
    } else {
        Write-Host "Failed to create package for $nuspecFile!" -ForegroundColor Red
    }
}
Write-Host "Packing process completed!" -ForegroundColor Cyan