
$rootDirectory = (Get-Location).ToString()


$metaFileContent = ""

$modkitsRoute = (Get-Location).ToString()
$modkitsRoute+= "\modkits.txt"

echo "Modkits File" | Out-File -FilePath $modkitsRoute

Get-ChildItem -Path $rootDirectory -Filter "*.meta" -Recurse -File |
ForEach-Object {

    $metaFileContent = (Get-Content -LiteralPath $_.FullName )
    $matches = $metaFileContent | Select-String -Pattern "<id value"

    if ($matches) {

    $result = "Modkits on "+ $_.DirectoryName
    $result | Out-File -FilePath  $modkitsRoute -Append

        Write-Host $result
        foreach ($match in $matches) {
            if ($match.Line.Length -gt 100) {
                    Write-Host "Bullshit locking system impedes me from looking through this file properly"
                }
                else 
                { 
                    $result= $($match.Line.Replace('<kitName>','').Replace('</kitName>','').Replace('</Item>','').Replace('<Item>','').Replace('<id value=','').Replace('/>','').Replace(' ',''))
                    Write-Host $result

                    $result | Out-File -FilePath  $modkitsRoute -Append
                }
            }
        }
    }


$content = Get-Content $modkitsRoute

$identicalLines = $content | Group-Object | Where-Object { $_.Count -gt 1 }

Write-Host "ModkitID conflicts":


$conflictsRoute = (Get-Location).ToString()
$conflictsRoute+= "\conflicts.txt"


echo $identicalLines | Out-File -FilePath $conflictsRoute

$identicalLines

echo ""
echo ""
echo ""
echo "That's it."
pause
