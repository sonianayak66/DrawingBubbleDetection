# Remove old Layout components (now replaced by modular layouts)
Remove-Item "src/components/Layout" -Recurse -Force

# Remove empty Views folder (if it's now empty)
if ((Get-ChildItem "src/components/Views" -Force | Measure-Object).Count -eq 0) {
    Remove-Item "src/components/Views" -Force
}

# Remove empty components folder if completely empty
if ((Get-ChildItem "src/components" -Force | Measure-Object).Count -eq 0) {
    Remove-Item "src/components" -Force
}

Write-Host "Cleanup completed!" -ForegroundColor Green