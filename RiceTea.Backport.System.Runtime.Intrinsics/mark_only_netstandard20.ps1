$tag = "#if !NETSTANDARD2_1_OR_GREATER"
$targetPath = "." 

$files = Get-ChildItem -Path $targetPath -Filter *.cs -Recurse | Where-Object { 
    $_.FullName -notmatch "\\obj\\" -and $_.FullName -notmatch "\\bin\\" 
}

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw
    
    if ([string]::IsNullOrWhiteSpace($content)) {
        Write-Host "Skipped: $($file.Name) (File is empty)" -ForegroundColor Gray
    }
    elseif ($content.StartsWith($tag)) {
        Write-Host "Skipped: $($file.Name) (Processed)" -ForegroundColor Yellow
    }
    else {
        $newContent = "$tag`n" + $content.Replace("`r`n", "`n").Trim() + "`n#endif"
        
        Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8 -NoNewline
        Write-Host "Finished: $($file.Name)" -ForegroundColor Green
    }
}