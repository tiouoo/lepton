$ErrorActionPreference = "Stop"
python (Join-Path $PSScriptRoot 'normalize-lf.py')
exit $LASTEXITCODE
