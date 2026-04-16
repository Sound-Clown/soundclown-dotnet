#!/usr/bin/env bash
# =============================================================================
# Script tạo các file test cho TC-02, TC-06, TC-07
# Chạy: bash test/create_test_files.sh
# =============================================================================

set -e

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
FILES="$DIR/files"

mkdir -p "$FILES"

echo "=== Tạo file test cho SoundClown QA ==="
echo ""

# 1. malicious.exe → đổi tên thành .mp3 (TC-02)
echo "[1/4] Tạo malicious.mp3 (file .exe đổi tên) — TC-02"
dd if=/dev/urandom of="$FILES/malicious_temp.bin" bs=1 count=2048 2>/dev/null
mv "$FILES/malicious_temp.bin" "$FILES/malicious.mp3"
echo "    ✓ $FILES/malicious.mp3 ($(du -sh $FILES/malicious.mp3 | cut -f1))"

# 2. File đúng 10MB (TC-06)
echo "[2/4] Tạo exact_10mb.mp3 (chính xác 10,485,760 bytes) — TC-06"
dd if=/dev/zero of="$FILES/exact_10mb.mp3" bs=1 count=10485760 2>/dev/null
echo "    ✓ $FILES/exact_10mb.mp3 ($(du -sh $FILES/exact_10mb.mp3 | cut -f1))"

# 3. File 10.5MB (TC-07)
echo "[3/4] Tạo over_10mb.mp3 (~10.5MB) — TC-07"
dd if=/dev/zero of="$FILES/over_10mb.mp3" bs=1 count=11010048 2>/dev/null
echo "    ✓ $FILES/over_10mb.mp3 ($(du -sh $FILES/over_10mb.mp3 | cut -f1))"

# 4. File 5MB (TC-01)
echo "[4/4] Tạo sample_5mb.mp3 (5MB) — TC-01"
dd if=/dev/zero of="$FILES/sample_5mb.mp3" bs=1 count=5242880 2>/dev/null
echo "    ✓ $FILES/sample_5mb.mp3 ($(du -sh $FILES/sample_5mb.mp3 | cut -f1))"

echo ""
echo "=== Hoàn tất! ==="
echo "Các file đã được tạo trong: $FILES/"
ls -lh "$FILES/"
