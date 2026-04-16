# report-latex

Thư mục này chứa mã nguồn LaTeX của báo cáo và các script hỗ trợ build/clean trên macOS và Windows.

## Yêu cầu

- Cài đặt TeX distribution có `latexmk`, `xelatex` và `biber`.
- Khuyến nghị dùng TeX Live, MiKTeX hoặc MacTeX bản mới.
- Trên Windows nếu dùng MiKTeX, cần cài thêm Perl cho `latexmk` chạy được. Strawberry Perl là lựa chọn phổ biến.

## Build

File cấu hình `latexmkrc` đã đặt output vào thư mục `build/`, nên PDF sau khi build sẽ nằm tại `build/main.pdf`.

macOS

```bash
cd docs/report-latex
./build.sh
```

Windows PowerShell

```powershell
cd docs/report-latex
.\build.ps1
```

Windows CMD

```cmd
cd docs\report-latex
build.bat
```

## Clean

Các lệnh clean chỉ xóa file phụ, giữ lại `build/main.pdf`.

macOS

```bash
cd docs/report-latex
./clean.sh
```

Windows PowerShell

```powershell
cd docs/report-latex
.\clean.ps1
```

Windows CMD

```cmd
cd docs\report-latex
clean.bat
```

## Ghi chú

- Nếu build lỗi do thiếu gói LaTeX, hãy cài thêm package tương ứng trong TeX distribution đang dùng.
- Nếu báo lỗi về `perl`, hãy cài Perl rồi mở lại terminal trước khi build lại.
- Nếu muốn xóa toàn bộ file sinh ra, dùng `latexmk -C main.tex` thay vì script `clean` hiện tại.

## File test tạm thời

Đã thêm file [temp-build-test.tex](temp-build-test.tex) để bạn kiểm tra nhanh pipeline build.

Build file này bằng cách truyền tên file vào script:

```bash
./build.sh temp-build-test.tex
```

```powershell
./build.ps1 temp-build-test.tex
```

```cmd
build.bat temp-build-test.tex
```
