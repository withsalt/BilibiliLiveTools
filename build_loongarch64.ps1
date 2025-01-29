# ---------------------
# 配置参数
# ---------------------

param (
    [string]$SolutionPath = "./src/BilibiliLiveTools.sln",  # .sln 文件的相对路径，请根据需要修改
    [string]$RemoteUser = "loong",
    [string]$RemoteHost = "192.168.188.25",
    [int]$RemotePort = 22,
    [string]$RemoteProjectPath = "/home/loong/Projects/CSharpProject/src",
    [switch]$UsePassword,  # 如果不使用密码认证，可以去掉这个开关
    [string]$ZipFileName = "CSharpProject.zip",
    [string]$BuildConfiguration = "Release"
)

# ---------------------
# 导入必要的模块
# ---------------------

# 导入 Posh-SSH 模块，如果尚未安装则安装它
if (!(Get-Module -ListAvailable -Name Posh-SSH)) {
    Write-Output "安装 Posh-SSH 模块..."
    Install-Module -Name Posh-SSH -Force -Scope CurrentUser
}
Import-Module Posh-SSH

# 获取脚本所在目录作为项目根目录
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Definition
Set-Location $scriptDirectory

# 验证 .sln 文件是否存在
if (-Not (Test-Path $SolutionPath)) {
    Write-Error "无法找到指定的解决方案文件：$SolutionPath"
    exit 1
}

# 获取项目目录路径（假设 .sln 文件位于项目根目录）
$localProjectPath = $scriptDirectory

# 认证方式
if ($UsePassword) {
    # 提示输入密码（安全处理）
    $password = Read-Host -Prompt "请输入远程主机的密码" -AsSecureString
    $credential = New-Object System.Management.Automation.PSCredential ($RemoteUser, $password)
} else {
    # 使用密钥认证
    # 请确保本地 SSH 密钥已配置，并且远程主机接受密钥认证
    $credential = Get-Credential -UserName $RemoteUser -Message "使用密钥认证，请确保配置了 SSH 密钥对。"
}

# 日志文件路径
$logFilePath = Join-Path -Path $scriptDirectory -ChildPath "build_log.txt"

# 清空或创建日志文件
New-Item -Path $logFilePath -ItemType File -Force | Out-Null

function Write-Log {
    param (
        [string]$Message,
        [string]$Level = "INFO",
        [switch]$NoConsole
    )
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "$timestamp [$Level] : $Message"
    if (-not $NoConsole) {
        Write-Output $logMessage
    }
    Add-Content -Path $logFilePath -Value $logMessage
}

# ---------------------
# 处理 tmp 文件夹
# ---------------------

$tmpDirectory = Join-Path -Path $scriptDirectory -ChildPath "tmp"

if (Test-Path $tmpDirectory) {
    Write-Log "检测到已存在的 tmp 文件夹，将进行删除..." "INFO"  -NoConsole
    try {
        Remove-Item -Path $tmpDirectory -Recurse -Force
        Write-Log "已删除现有的 tmp 文件夹：$tmpDirectory" "INFO"  -NoConsole
    } catch {
        Write-Log "无法删除 tmp 文件夹：$_" "ERROR"
        exit 1
    }
}

# 创建 tmp 文件夹
try {
    New-Item -Path $tmpDirectory -ItemType Directory -Force | Out-Null
    Write-Log "已创建 tmp 文件夹：$tmpDirectory" "INFO"  -NoConsole
} catch {
    Write-Log "无法创建 tmp 文件夹：$_" "ERROR"
    exit 1
}

# 压缩包路径调整为 tmp 文件夹内
$zipFilePath = Join-Path -Path $tmpDirectory -ChildPath $ZipFileName

# ---------------------
# 压缩本地项目
# ---------------------
Write-Log "压缩本地项目..." -NoConsole
try {
    if (Test-Path $zipFilePath) {
        Remove-Item $zipFilePath -Force
        Write-Log "已删除旧的压缩包：$zipFilePath"
    }
    Compress-Archive -Path "$localProjectPath\src\*" -DestinationPath $zipFilePath -Force
    Write-Log "项目已成功压缩到 $zipFilePath" -NoConsole
} catch {
    Write-Log "压缩项目失败：$_" "ERROR"
    exit 1
}

# ---------------------
# 建立 SSH 会话
# ---------------------
Write-Log "尝试建立到 $RemoteHost 的 SSH 连接..." -NoConsole
try {
    $session = New-SSHSession -ComputerName $RemoteHost -Port $RemotePort -Credential $credential -AcceptKey -ErrorAction Stop
    Write-Log "SSH 会话已成功建立。" -NoConsole
} catch {
    Write-Log "无法建立 SSH 会话：$_" "ERROR"
    exit 1
}

# ---------------------
# 检查远程环境
# ---------------------
Write-Log "检查远程主机的必要工具..." -NoConsole

$requiredCommands = @("unzip", "dotnet")

foreach ($cmd in $requiredCommands) {
    $checkCmd = "which $cmd"
    $result = Invoke-SSHCommand -SessionId $session.SessionId -Command $checkCmd
    if ($result.Output.Trim() -eq "") {
        Write-Log "远程主机缺少必要工具：$cmd" "ERROR"
        Remove-SSHSession -SessionId $session.SessionId
        exit 1
    } else {
        Write-Log "远程主机已安装 $cmd" -NoConsole
    }
}

# ---------------------
# 创建远程目录
# ---------------------
Write-Log "在远程主机上创建项目目录：$RemoteProjectPath" -NoConsole
$mkdirCommand = "mkdir -p $RemoteProjectPath"
$mkdirResult = Invoke-SSHCommand -SessionId $session.SessionId -Command $mkdirCommand
if ($mkdirResult.ExitStatus -ne 0) {
    Write-Log "创建远程目录失败：$($mkdirResult.Error)" "ERROR"
    Remove-SSHSession -SessionId $session.SessionId
    exit 1
} else {
    Write-Log "远程目录已创建或已存在。" -NoConsole
}

# 清理远程目录中的旧文件
Write-Log "清理远程项目目录中的旧文件..." -NoConsole
$cleanupCommand = "rm -rf $RemoteProjectPath/*"
$cleanupResult = Invoke-SSHCommand -SessionId $session.SessionId -Command $cleanupCommand
if ($cleanupResult.ExitStatus -ne 0) {
    Write-Log "清理远程目录失败：$($cleanupResult.Error)" "ERROR"
    Remove-SSHSession -SessionId $session.SessionId
    exit 1
} else {
    Write-Log "远程目录已清理。" -NoConsole
}

# ---------------------
# 上传压缩包
# ---------------------
Write-Log "上传项目到远程主机..."
try {
    Set-SCPItem -ComputerName $RemoteHost -Port $RemotePort -Credential $credential -AcceptKey -Path $zipFilePath -Destination $RemoteProjectPath
    Write-Log "压缩包已上传。"
} catch {
    Write-Log "上传压缩包失败：$_" "ERROR"
    Remove-SSHSession -SessionId $session.SessionId
    exit 1
}

# ---------------------
# 解压缩远程文件
# ---------------------
Write-Log "在远程主机上解压缩项目文件..." -NoConsole
$unzipCommand = "cd $RemoteProjectPath && unzip -o $ZipFileName && rm $ZipFileName"
$unzipResult = Invoke-SSHCommand -SessionId $session.SessionId -Command $unzipCommand
if ($unzipResult.ExitStatus -ne 0) {
    Write-Log "解压缩失败：$($unzipResult.Error)" "ERROR"
    Remove-SSHSession -SessionId $session.SessionId
    exit 1
} else {
    Write-Log "项目文件已成功解压缩。" -NoConsole
}

# ---------------------
# 执行远程编译
# ---------------------
Write-Log "开始编译项目..."
$buildCommand = "cd $RemoteProjectPath/../ && dotnet build $SolutionPath --configuration $BuildConfiguration"
$buildResult = Invoke-SSHCommand -SessionId $session.SessionId -Command $buildCommand

Write-Log "编译输出："
Write-Log $buildResult.Output

# 检查编译是否成功
if ($buildResult.ExitStatus -eq 0) {
    Write-Log "编译成功。"
} else {
    Write-Log "编译失败。错误信息：$($buildResult.Error)" "ERROR"
    Remove-SSHSession -SessionId $session.SessionId
    exit 1
}

# 可选：执行单元测试或部署
# $runTests = Read-Host -Prompt "是否在远程主机上运行单元测试？(y/N)"
# if ($runTests -eq 'y' -or $runTests -eq 'Y') {
    # Write-Log "在远程主机上运行单元测试..."
    # $testCommand = "cd $RemoteProjectPath && dotnet test $SolutionPath --configuration $BuildConfiguration"
    # $testResult = Invoke-SSHCommand -SessionId $session.SessionId -Command $testCommand
    # Write-Log "测试输出："
    # Write-Log $testResult.Output

    # if ($testResult.ExitStatus -eq 0) {
        # Write-Log "所有单元测试通过。"
    # } else {
        # Write-Log "部分单元测试失败。错误信息：$($testResult.Error)" "ERROR"
        # Remove-SSHSession -SessionId $session.SessionId
        # exit 1
    # }
# }

# ---------------------
# 可选：部署或运行应用程序
# ---------------------
# $deploy = Read-Host -Prompt "是否在远程主机上部署或运行应用程序？(y/N)"
# if ($deploy -eq 'y' -or $deploy -eq 'Y') {
    # Write-Log "在远程主机上运行应用程序..."
    # # 根据需要自定义运行命令，例如：
    # $runCommand = "cd $RemoteProjectPath && dotnet run --configuration $BuildConfiguration"
    # $runResult = Invoke-SSHCommand -SessionId $session.SessionId -Command $runCommand
    # Write-Log "应用程序输出："
    # Write-Log $runResult.Output
# }

# ---------------------
# 关闭 SSH 会话
# ---------------------
Remove-SSHSession -SessionId $session.SessionId
Write-Log "会话已关闭。"

# ---------------------
# 清理本地压缩包
# ---------------------
try {
    Remove-Item $zipFilePath -Force
    Write-Log "本地压缩包已删除。" -NoConsole
	Remove-Item -Path $tmpDirectory -Recurse -Force
	Write-Log "已删除现有的 tmp 文件夹：$tmpDirectory" "INFO"  -NoConsole
} catch {
    Write-Log "无法删除本地压缩包：$_" "WARN"
}

Write-Log "日志请查看：$logFilePath"
