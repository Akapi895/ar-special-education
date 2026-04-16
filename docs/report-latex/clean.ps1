$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

$buildDir = Join-Path $PSScriptRoot "build"
if (Test-Path $buildDir) {
	$patterns = @(
		"*.aux", "*.log", "*.out", "*.toc", "*.lof", "*.lot",
		"*.fls", "*.fdb_latexmk", "*.synctex.gz",
		"*.bcf", "*.blg", "*.bbl", "*.run.xml", "*.xdv",
		"*.acn", "*.acr", "*.alg", "*.glg", "*.glo", "*.gls", "*.ist", "*.nav", "*.snm"
	)

	foreach ($pattern in $patterns) {
		Get-ChildItem -Path $buildDir -Recurse -File -Filter $pattern -ErrorAction SilentlyContinue |
			Remove-Item -Force -ErrorAction SilentlyContinue
	}
}
Write-Host "Cleaned auxiliary files."