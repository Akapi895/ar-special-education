@echo off
setlocal

cd /d "%~dp0"
if not exist build (
	echo Cleaned auxiliary files.
	exit /b 0
)

set "PATTERNS=*.aux *.log *.out *.toc *.lof *.lot *.fls *.fdb_latexmk *.synctex.gz *.bcf *.blg *.bbl *.run.xml *.xdv *.acn *.acr *.alg *.glg *.glo *.gls *.ist *.nav *.snm"

for %%P in (%PATTERNS%) do (
	del /q "build\%%P" 2>nul
	for /r build %%F in (%%P) do del /q "%%F" 2>nul
)

echo Cleaned auxiliary files.