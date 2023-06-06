
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


# SIG # Begin signature block
# MIIFdgYJKoZIhvcNAQcCoIIFZzCCBWMCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUOOB+onfK/yIHWl2FmE8NNN1k
# Z/ygggMOMIIDCjCCAfKgAwIBAgIQTbB+ZYXJy7ZCoJIR+m+jUDANBgkqhkiG9w0B
# AQsFADAdMRswGQYDVQQDDBJDb2RlIFNpZ25pbmcgLSBFZGQwHhcNMjMwNjA2MTg0
# OTA5WhcNMjQwNjA2MTkwOTA5WjAdMRswGQYDVQQDDBJDb2RlIFNpZ25pbmcgLSBF
# ZGQwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDjEcn0Ub+OOG1JlovR
# ojTw7DVjBJN5ulf2NnRhAEtNFtD53uvBFvBqnqxLyko4yVl7YP8uEKJ2PtSMwDsZ
# tiPSvnIIcrpPcdYMmMonuoMtTYelGaOo0szTUHYGKE0D7ejiwICqtptUJF1I4MMH
# ZQXDnaJGKyzal6Ttzz+6k8AF0PtNDoD1ulmhHMC8R2eM8H4t4TRkR8rg52I1bcTn
# KLkQkM2IEbrK3vVl5BhvGZDBlq9qK91FG5BBbJaKvKlfZ37kla3fgXgrXoL4idM3
# TP+nmxafEjZyeJ0+7JQsJy1AyDswwX5GFrp/9iunlD9rhWvd1lfq9smUB2vaZGg6
# g50NAgMBAAGjRjBEMA4GA1UdDwEB/wQEAwIHgDATBgNVHSUEDDAKBggrBgEFBQcD
# AzAdBgNVHQ4EFgQUfHYYVIKuPAaEu2MBllaomUEW6XYwDQYJKoZIhvcNAQELBQAD
# ggEBADukpVEIGEM4qqPVlC19ga/qP4kl51aPOZV+UbgIF/JqRPvxjSSNTbp/mMQy
# MxVD+pg34xPjX6AKLgCsu+7aZOECOLBeJ9QxVEJiDb/1pfFbXp9/jQP9yxXyGhAF
# Ua2VVf+QAhVGrHfcfVQxKcNC6YlJV2NT9bGWSLd30tCve7wVSBLmNNq/W+hXO09R
# 5N3o2oBK54r9CAbM0lnEe6qR1ScdrTXSEAG3UUcQqNM+QeJD01qUkoEY1pRlFDr3
# dpD8yaZqSR1c/te1MUT/dxu6s8ie2Wn75mPxe2Ll5FyGyVERnPTid1e/2qrFwbnA
# FGisigtO5sy82hCE6QeTp6qqgl8xggHSMIIBzgIBATAxMB0xGzAZBgNVBAMMEkNv
# ZGUgU2lnbmluZyAtIEVkZAIQTbB+ZYXJy7ZCoJIR+m+jUDAJBgUrDgMCGgUAoHgw
# GAYKKwYBBAGCNwIBDDEKMAigAoAAoQKAADAZBgkqhkiG9w0BCQMxDAYKKwYBBAGC
# NwIBBDAcBgorBgEEAYI3AgELMQ4wDAYKKwYBBAGCNwIBFTAjBgkqhkiG9w0BCQQx
# FgQUbUU+4pPD6BYFeeJcxz0Q5i+GfuEwDQYJKoZIhvcNAQEBBQAEggEA3B8Zbbse
# hXIOuGoln6ragVm3ENsb9fHfRg4ixzXLyzKrp3m3IoOSeVQSVUDv1RsQaoYONoQM
# KNzl0Ggtqzp1RjZBoyCW+l+85T0Ssl6DpBLPdEl1xYjI/6SN7jXEmJ8qez18KjWn
# liGoX8+CQZ1ytdIB4MYKhZdN/fFX4Bs6Sc2XKqYQTOJwV+BRa0XZVZaR13gRYtUH
# hDn0usPMPOLQeFcth0gkisrbzHd/kO5pLZKKVDW56jOaGYviHdxBxgQeEkTC52yp
# 1sRIr//LQRSZM4VTdKDiILQxZ2W3I9ieaEaCJ29GM9KZUnHHiWUX4tsuzDdD0w1q
# axKnQteL047Gpg==
# SIG # End signature block
