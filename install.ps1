# Build latest code
dotnet publish -c Release .\tikz-to-pdf\tikz-to-pdf.csproj

# Create install folder if not exists, else clear its contents
$installFolder = "$env:LOCALAPPDATA\Programs\tikz-to-pdf";
if( -not (Test-Path $installFolder) ) {
    New-Item $installFolder -ItemType Directory
}
else {
    Remove-Item -Recurse "$installFolder\*"
}

# Copy build to install folder
Copy-Item -Recurse .\tikz-to-pdf\bin\Release\net8.0\publish\* $env:LOCALAPPDATA\Programs\tikz-to-pdf

# If install folder is not in path, add it
if(-not ($env:Path -contains $installFolder)) {
    $env:Path += ";$installFolder"
    [System.Environment]::SetEnvironmentVariable("Path", $env:Path, [EnvironmentVariableTarget]::User)
}

Write-Host "Done."