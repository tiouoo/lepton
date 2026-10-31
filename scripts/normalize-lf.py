"""Normalize UTF-8 project text to LF; leave binary files and submodules alone."""

from pathlib import Path
import subprocess

root = Path(__file__).resolve().parent.parent
result = subprocess.run(
    ["git", "ls-files", "--cached", "--others", "--exclude-standard", "-z"],
    cwd=root, check=True, stdout=subprocess.PIPE,
)
for name in dict.fromkeys(result.stdout.split(b"\0")):
    path = root / name.decode("utf-8")
    if not name or path.is_symlink() or not path.is_file():
        continue
    data = path.read_bytes()
    if b"\0" in data:
        continue
    try:
        data.decode("utf-8-sig")
    except UnicodeDecodeError:
        continue
    normalized = data.replace(b"\r\n", b"\n").replace(b"\r", b"\n")
    if normalized != data:
        path.write_bytes(normalized)
        print(f"LF: {path.relative_to(root)}")
