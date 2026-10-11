#Requires -Version 5.1
<#
.SYNOPSIS
    游戏更新后跑一次,检查存档格式是否仍与本修改器兼容。

.DESCRIPTION
    全程只读,不会改动任何存档:
      1. 从 SaveCrypto.cs / SaveStore.cs 读出本工具预期的 AES 密钥与文件名前缀
         (直接解析源码,改了代码这里自动跟着变,不用手动同步);
      2. 定位存档目录,确认预期的 v*_ 文件还在;
      3. 解密 index / config / device 以及所有 userdata,校验能还原成合法 JSON;
      4. 用 MainForm.cs 里的字段清单核对当前存档字段是否齐全;
      5. 核对 index 元数据与 userdata 的同步映射是否仍然成立。

.PARAMETER SaveDir
    存档目录。默认自动定位 %USERPROFILE%\AppData\LocalLow\Neotro Inc*\MagicalPrincess。
    也可以指向一份备份出来的存档副本,用来离线验证老档。

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts\check-game-compat.ps1

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts\check-game-compat.ps1 -SaveDir "D:\backup\MagicalPrincess"

.NOTES
    本文件必须保存为 UTF-8 (带 BOM)。Windows PowerShell 5.1 会把无 BOM 的 UTF-8
    文件按 ANSI 解码,中文会乱码并直接导致语法错误。
    退出码: 0 = 全部通过, 1 = 有检查项失败。
#>
[CmdletBinding()]
param(
    [string]$SaveDir
)

$ErrorActionPreference = 'Stop'

# ----------------------------------------------------------------- 结果收集
$script:failures = @()
$script:warnings = @()

function Write-Section([string]$Title) {
    Write-Host ''
    Write-Host ('=' * 68)
    Write-Host $Title
    Write-Host ('=' * 68)
}
function Write-Pass([string]$Message) { Write-Host ('  [OK]   ' + $Message) }
function Write-Fail([string]$Message) { Write-Host ('  [FAIL] ' + $Message); $script:failures += $Message }
function Write-Warn([string]$Message) { Write-Host ('  [WARN] ' + $Message); $script:warnings += $Message }
function Write-Info([string]$Message) { Write-Host ('         ' + $Message) }

function Complete-Check {
    Write-Section '汇总'
    if ($script:failures.Count -eq 0) {
        Write-Host '  结论: 通过 —— 当前存档格式与本工具兼容。' -ForegroundColor Green
        if ($script:warnings.Count -gt 0) {
            Write-Host ("  另有 $($script:warnings.Count) 条提醒,见上面 [WARN]。") -ForegroundColor Yellow
        }
        exit 0
    }
    Write-Host ("  结论: 失败 $($script:failures.Count) 项 —— 大概率是游戏更新改了存档格式,需要同步更新本工具。") -ForegroundColor Red
    $script:failures | ForEach-Object { Write-Host ('    - ' + $_) -ForegroundColor Red }
    exit 1
}

# ----------------------------------------------------------------- 定位仓库
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path (Join-Path $repoRoot 'src'))) {
    # 允许脚本被单独拷出去运行
    $repoRoot = $PSScriptRoot
}
$cryptoFile = Join-Path $repoRoot 'src\MagicalPrincess.SaveEditor\Core\SaveCrypto.cs'
$storeFile  = Join-Path $repoRoot 'src\MagicalPrincess.SaveEditor\Core\SaveStore.cs'
$formFile   = Join-Path $repoRoot 'src\MagicalPrincess.SaveEditor\UI\MainForm.cs'
$headFile   = Join-Path $repoRoot 'src\MagicalPrincess.SaveEditor\Core\HeadlessTest.cs'

Write-Section '0) 读取本工具的预期值(来自源码)'
foreach ($f in @($cryptoFile, $storeFile, $formFile, $headFile)) {
    if (-not (Test-Path $f)) { Write-Fail "源码缺失: $f"; }
}
if ($script:failures.Count -gt 0) { exit 1 }

$cryptoText = Get-Content -LiteralPath $cryptoFile -Raw -Encoding UTF8
$storeText  = Get-Content -LiteralPath $storeFile  -Raw -Encoding UTF8

$aesKey = [regex]::Match($cryptoText, 'AesKey\s*=\s*"([^"]+)"').Groups[1].Value
$aesIV  = [regex]::Match($cryptoText, 'AesIV\s*=\s*"([^"]+)"').Groups[1].Value
$indexFile = [regex]::Match($storeText, 'IndexFile\s*=\s*"([^"]+)"').Groups[1].Value
$configFile = [regex]::Match($storeText, 'ConfigFile\s*=\s*"([^"]+)"').Groups[1].Value
$deviceFile = [regex]::Match($storeText, 'DeviceFile\s*=\s*"([^"]+)"').Groups[1].Value
$slotTotal  = [regex]::Match($storeText, 'SlotTotal\s*=\s*(\d+)').Groups[1].Value

if (-not $aesKey -or -not $aesIV -or -not $indexFile) {
    Write-Fail '无法从源码解析出密钥或文件名,SaveCrypto.cs / SaveStore.cs 结构可能变了。'
    exit 1
}
$prefix = ($indexFile -split '_')[0] + '_'          # 例如 v10_
Write-Pass ("文件名前缀 $prefix   (index=$indexFile  config=$configFile  device=$deviceFile  slots=$slotTotal)")
Write-Info ("AES-128-CBC  key=$aesKey  iv=$aesIV")

# ----------------------------------------------------------------- 字段清单
$fieldRx = [regex]'\("([^"]+)"\s*,\s*"([A-Za-z0-9_]+)"\s*,\s*(true|false)\)'
$fields = @{}
foreach ($src in @($formFile, $headFile)) {
    $text = Get-Content -LiteralPath $src -Raw -Encoding UTF8
    foreach ($m in $fieldRx.Matches($text)) {
        $label = $m.Groups[1].Value
        $key   = $m.Groups[2].Value
        $isG   = $m.Groups[3].Value -eq 'true'
        # 同一个 key 在 gstatus 和 status 里都可能出现(例如 ap),所以按 key+区域 去重
        $id = "$key|$isG"
        if (-not $fields.ContainsKey($id)) {
            $fields[$id] = [pscustomobject]@{ Key = $key; Label = $label; IsGStatus = $isG }
        }
    }
}
if ($fields.Count -lt 10) {
    Write-Fail ("字段清单解析异常,只拿到 $($fields.Count) 个,MainForm.cs 的字段定义格式可能变了。")
} else {
    Write-Pass ("字段清单: 从源码解析出 $($fields.Count) 个受管字段")
}

$listKeys = @('itemDataParamList', 'friendDataParamList', 'skillDataParamList', 'battleArtsDataParamList',
              'activityDataParamList', 'curriculumDataParamList', 'statusDataHistory', 'flagEventReaded')

# ----------------------------------------------------------------- 解密函数
function Get-PlainJson {
    param([string]$Path)
    $raw = [System.IO.File]::ReadAllText($Path)
    $bytes = [Convert]::FromBase64String($raw.Trim())
    $aes = [System.Security.Cryptography.Aes]::Create()
    $aes.Mode = [System.Security.Cryptography.CipherMode]::CBC
    $aes.Padding = [System.Security.Cryptography.PaddingMode]::Zeros
    $aes.BlockSize = 128
    $aes.KeySize = 128
    $aes.Key = [System.Text.Encoding]::UTF8.GetBytes($aesKey)
    $aes.IV = [System.Text.Encoding]::UTF8.GetBytes($aesIV)
    $ms = New-Object System.IO.MemoryStream(, $bytes)
    $cs = New-Object System.Security.Cryptography.CryptoStream($ms, $aes.CreateDecryptor(), [System.Security.Cryptography.CryptoStreamMode]::Read)
    $out = New-Object System.IO.MemoryStream
    $cs.CopyTo($out)
    $plain = $out.ToArray()
    # 明文尾部是 Zeros 填充的 NUL,必须去掉,否则 JSON 解析会报错
    $end = $plain.Length - 1
    while ($end -ge 0 -and $plain[$end] -eq 0) { $end-- }
    if ($end -lt 0) { return '' }
    $trimmed = New-Object byte[] ($end + 1)
    [Array]::Copy($plain, $trimmed, $end + 1)
    return [System.Text.Encoding]::UTF8.GetString($trimmed)
}

function Read-SaveJson {
    param([string]$Path, [switch]$Quiet)
    try {
        $text = Get-PlainJson -Path $Path
        return ($text | ConvertFrom-Json)
    } catch {
        if (-not $Quiet) { Write-Fail ("解密/解析失败: " + (Split-Path -Leaf $Path) + ' -> ' + $_.Exception.Message) }
        return $null
    }
}

# ----------------------------------------------------------------- 定位存档目录
Write-Section '1) 定位存档目录'
if (-not $SaveDir) {
    $localLow = Join-Path $env:USERPROFILE 'AppData\LocalLow'
    if (Test-Path $localLow) {
        foreach ($dir in Get-ChildItem $localLow -Directory -ErrorAction SilentlyContinue) {
            if ($dir.Name -like 'Neotro Inc*') {
                $candidate = Join-Path $dir.FullName 'MagicalPrincess'
                if (Test-Path $candidate) { $SaveDir = $candidate; break }
            }
        }
    }
}
if (-not $SaveDir -or -not (Test-Path $SaveDir)) {
    Write-Fail '没有找到存档目录,请用 -SaveDir 指定(或先运行一次游戏)。'
    Complete-Check
}
Write-Pass "存档目录: $SaveDir"

# ----------------------------------------------------------------- 文件名格式
Write-Section '2) 存档文件名格式'
$allFiles = @(Get-ChildItem -LiteralPath $SaveDir -File -ErrorAction SilentlyContinue)
$expected = @($indexFile, $configFile, $deviceFile)
$missingExpected = @($expected | Where-Object { -not (Test-Path (Join-Path $SaveDir $_)) })
$unexpected = @($allFiles | Where-Object { $_.Name -like 'v*_*' -and $_.Name -notlike "$prefix*" } | ForEach-Object { $_.Name })

if ($missingExpected.Count -eq 0) {
    Write-Pass "预期文件都在: $($expected -join ', ')"
} else {
    Write-Fail ("缺少预期文件: " + ($missingExpected -join ', '))
    if ($unexpected.Count -gt 0) {
        Write-Info ("目录里出现了非 $prefix 开头的存档文件: " + ($unexpected -join ', '))
        Write-Info '这基本可以确定是游戏更新改了存档格式,需要同步更新本工具的常量。'
    }
}
if ($unexpected.Count -gt 0 -and $missingExpected.Count -eq 0) {
    Write-Warn ("目录里有额外的存档文件: " + ($unexpected -join ', '))
}

$userFiles = @($allFiles | Where-Object { $_.Name -like "$prefix" + 'userdata*.dat' } |
    Sort-Object { [int]([regex]::Match($_.Name, 'userdata(\d+)').Groups[1].Value) })
if ($userFiles.Count -eq 0) { Write-Fail "没有找到 $($prefix)userdata*.dat 槽位文件。" }
else { Write-Pass ("找到 $($userFiles.Count) 个槽位文件 (共 $slotTotal 个槽位)") }

# ----------------------------------------------------------------- 逐个解密
Write-Section '3) 解密与 JSON 还原'
$index = $null
if (Test-Path (Join-Path $SaveDir $indexFile)) {
    $index = Read-SaveJson (Join-Path $SaveDir $indexFile)
    if ($index) {
        $slotCount = @($index.dataList).Count
        Write-Pass "index 解密成功,dataList 有 $slotCount 条"
    }
}
foreach ($name in @($configFile, $deviceFile)) {
    $p = Join-Path $SaveDir $name
    if (Test-Path $p) {
        if (Read-SaveJson $p) { Write-Pass "$name 解密成功" }
    }
}

$decoded = @{}
foreach ($f in $userFiles) {
    $json = Read-SaveJson $f.FullName -Quiet
    if ($json) {
        $decoded[$f.Name] = [pscustomobject]@{ File = $f; Json = $json }
    } else {
        Write-Fail ("槽位文件解密/解析失败: " + $f.Name)
    }
}
if ($decoded.Count -gt 0) {
    Write-Pass ("槽位存档解密成功 $($decoded.Count)/$($userFiles.Count)")
}

# ----------------------------------------------------------------- 字段核对
Write-Section '4) 字段核对(用进度最靠后的那个档)'
if ($decoded.Count -gt 0) {
    $newest = $decoded.Values | Sort-Object { $_.File.LastWriteTime } -Descending | Select-Object -First 1
    $json = $newest.Json
    Write-Info ("取样: " + $newest.File.Name + '  最后写入 ' + $newest.File.LastWriteTime.ToString('yyyy-MM-dd HH:mm'))
    $gs = $json.gstatus
    $st = $json.status
    if (-not $gs -or -not $st) {
        Write-Fail '这个档里没有 gstatus / status 对象,存档结构可能变了。'
    } else {
        $missingFields = @()
        foreach ($f in $fields.Values) {
            $obj = if ($f.IsGStatus) { $gs } else { $st }
            if ($null -eq $obj.PSObject.Properties[$f.Key]) { $missingFields += ($f.Label + ' (' + $f.Key + ')') }
        }
        if ($missingFields.Count -eq 0) {
            Write-Pass ("$($fields.Count)/$($fields.Count) 个受管字段全部存在")
        } else {
            Write-Fail ("缺失 $($missingFields.Count) 个字段: " + ($missingFields -join ', '))
        }
        $missingLists = @($listKeys | Where-Object { $null -eq $json.PSObject.Properties[$_] })
        if ($missingLists.Count -eq 0) { Write-Pass "8 个列表数组都在" }
        else { Write-Fail ("缺少列表数组: " + ($missingLists -join ', ')) }
    }
}

# ----------------------------------------------------------------- 索引同步
Write-Section '5) index 元数据同步映射'
if ($index -and $decoded.Count -gt 0) {
    $dl = @($index.dataList)
    $checkedCount = 0
    $badSlots = @()
    foreach ($entry in $decoded.GetEnumerator()) {
        $slotNo = [int]([regex]::Match($entry.Key, 'userdata(\d+)').Groups[1].Value)
        if ($slotNo -ge $dl.Count) { continue }
        $meta = $dl[$slotNo]
        $u = $entry.Value.Json
        $pairs = @(
            @('pn', $meta.pn, $u.gstatus.pn),
            @('lo', $meta.lo, $u.gstatus.c)
        )
        foreach ($lv in @('lp', 'li', 'lc', 'ls', 'lb', 'la', 'lm')) {
            $pairs += , @($lv, $meta.$lv, $u.status.$lv)
        }
        $mismatch = @($pairs | Where-Object { $_[1] -ne $_[2] })
        $checkedCount++
        if ($mismatch.Count -gt 0) {
            $badSlots += ("槽位 $slotNo : " + (($mismatch | ForEach-Object { "$($_[0]) index=$($_[1]) 存档=$($_[2])" }) -join '; '))
        }
    }
    if ($badSlots.Count -eq 0) {
        Write-Pass "核对 $checkedCount 个槽位,index 与 userdata 完全一致"
    } else {
        Write-Fail ("index 与 userdata 不一致(可能是你自己手改过 index,也可能游戏改了字段含义):")
        $badSlots | ForEach-Object { Write-Info $_ }
    }
} else {
    Write-Warn '缺少 index 或槽位档,跳过同步核对。'
}

# ----------------------------------------------------------------- 汇总
Complete-Check
