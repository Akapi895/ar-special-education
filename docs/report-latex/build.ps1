param(
    [Parameter(Position = 0)]
    [string]$Target = "main.tex"
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

$xelatexArgs = @(
    "-interaction=nonstopmode",
    "-file-line-error",
    "-synctex=1",
    "-output-directory=build"
)

function Invoke-XeLaTeX {
    param([string]$InputFile)

    & xelatex @xelatexArgs $InputFile
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

function Invoke-BibTeXIfNeeded {
    param(
        [string]$BuildDirectory,
        [string]$BaseName,
        [string]$ProjectRoot
    )

    $auxPath = Join-Path $BuildDirectory "$BaseName.aux"
    if (-not (Test-Path $auxPath)) { return }

    $bibtexCmd = Get-Command bibtex -ErrorAction SilentlyContinue
    if (-not $bibtexCmd) { return }

    $auxContent = Get-Content $auxPath -Raw
    if ($auxContent -notmatch "\\citation") { return }

    $previousBibInputs = $env:BIBINPUTS
    Push-Location $BuildDirectory
    try {
        $env:BIBINPUTS = "$ProjectRoot;"
        & bibtex $BaseName
        if ($LASTEXITCODE -ne 0) {
            exit $LASTEXITCODE
        }
    }
    finally {
        if ($null -eq $previousBibInputs) {
            Remove-Item Env:BIBINPUTS -ErrorAction SilentlyContinue
        }
        else {
            $env:BIBINPUTS = $previousBibInputs
        }
        Pop-Location
    }
}

if (-not (Test-Path $Target)) {
    throw "Target file not found: $Target"
}

$buildDir = Join-Path $PSScriptRoot "build"
if (-not (Test-Path $buildDir)) {
    New-Item -ItemType Directory -Path $buildDir | Out-Null
}

Invoke-XeLaTeX -InputFile $Target

$baseName = [System.IO.Path]::GetFileNameWithoutExtension($Target)
Invoke-BibTeXIfNeeded -BuildDirectory $buildDir -BaseName $baseName -ProjectRoot $PSScriptRoot

Invoke-XeLaTeX -InputFile $Target
Invoke-XeLaTeX -InputFile $Target

$pdfName = "$baseName.pdf"
$pdfPath = Join-Path $PSScriptRoot "build\$pdfName"
if (-not (Test-Path $pdfPath)) {
    throw "Build finished without creating PDF: $pdfPath"
}
Write-Host "Done: $pdfPath"