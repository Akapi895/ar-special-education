#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$SCRIPT_DIR"

if [[ -d build ]]; then
	patterns=(
		"*.aux" "*.log" "*.out" "*.toc" "*.lof" "*.lot"
		"*.fls" "*.fdb_latexmk" "*.synctex.gz"
		"*.bcf" "*.blg" "*.bbl" "*.run.xml" "*.xdv"
		"*.acn" "*.acr" "*.alg" "*.glg" "*.glo" "*.gls" "*.ist" "*.nav" "*.snm"
	)

	for pattern in "${patterns[@]}"; do
		find build -type f -name "$pattern" -delete
	done
fi

echo "Cleaned auxiliary files."