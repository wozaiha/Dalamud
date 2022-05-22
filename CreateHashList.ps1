$hashes = [ordered]@{}

Set-Location bin\\Debug

Get-ChildItem -File -Recurse -Exclude dalamud.txt,*.zip,*.pdb,*.ipdb | Foreach-Object {
	$key = ($_.FullName | Resolve-Path -Relative).TrimStart(".\\")
	$val = (Get-FileHash $_.FullName -Algorithm MD5).Hash
    $hashes.Add($key, $val)
}

$hashes | ConvertTo-Json | Out-File -FilePath "hashes.json"