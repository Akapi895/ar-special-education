#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$SCRIPT_DIR"

TARGET="${1:-main.tex}"

if [[ ! -f "$TARGET" ]]; then
	echo "Target file not found: $TARGET"
	exit 1
fi

mkdir -p build

BASE_NAME="$(basename "${TARGET%.*}")"
PDF_PATH="$SCRIPT_DIR/build/$BASE_NAME.pdf"
XELATEX_ARGS=(-interaction=nonstopmode -file-line-error -synctex=1 -output-directory=build)

run_xelatex() {
	xelatex "${XELATEX_ARGS[@]}" "$TARGET"
}

run_bibtex_if_needed() {
	local aux_path="build/$BASE_NAME.aux"
	if [[ ! -f "$aux_path" ]]; then
		return
	fi

	if ! grep -q '\\citation' "$aux_path"; then
		return
	fi

	if ! command -v bibtex >/dev/null 2>&1; then
		return
	fi

	(
		cd build
		BIBINPUTS="$SCRIPT_DIR;" bibtex "$BASE_NAME"
	)
}

run_xelatex
run_bibtex_if_needed
run_xelatex
run_xelatex

if [[ ! -f "$PDF_PATH" ]]; then
	echo "Build finished without creating PDF: $PDF_PATH"
	exit 1
fi

echo "Done: $PDF_PATH"