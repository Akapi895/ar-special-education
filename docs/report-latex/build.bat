@echo off
setlocal

cd /d "%~dp0"
set "TARGET=%~1"
if "%TARGET%"=="" set "TARGET=main.tex"
set "XELATEX_OPTS=-interaction=nonstopmode -file-line-error -synctex=1 -output-directory=build"

if not exist "%TARGET%" (
	echo Target file not found: "%TARGET%"
	exit /b 1
)

if not exist build mkdir build

for %%I in ("%TARGET%") do set "BASE_NAME=%%~nI"
set "PDF_PATH=%~dp0build\%BASE_NAME%.pdf"

call :run_xelatex || exit /b %errorlevel%
call :run_bibtex_if_needed || exit /b %errorlevel%
call :run_xelatex || exit /b %errorlevel%
call :run_xelatex || exit /b %errorlevel%

if not exist "%PDF_PATH%" (
	echo Build finished without creating PDF: "%PDF_PATH%"
	exit /b 1
)
echo Done: %PDF_PATH%
exit /b 0

:run_xelatex
xelatex %XELATEX_OPTS% "%TARGET%"
exit /b %errorlevel%

:run_bibtex_if_needed
if not exist "build\%BASE_NAME%.aux" exit /b 0

findstr /r "\\citation" "build\%BASE_NAME%.aux" >nul
if errorlevel 1 exit /b 0

where bibtex >nul 2>nul
if errorlevel 1 exit /b 0

set "OLD_BIBINPUTS=%BIBINPUTS%"
pushd build
set "BIBINPUTS=%~dp0;"
bibtex "%BASE_NAME%"
set "BIBTEX_EXIT=%errorlevel%"
popd
set "BIBINPUTS=%OLD_BIBINPUTS%"
exit /b %BIBTEX_EXIT%