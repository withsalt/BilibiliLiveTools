# build.ps1 (v3 - Uses a temporary directory inside 'release')

# 设置脚本在遇到错误时立即停止执行
$ErrorActionPreference = 'Stop'
# 使用更严格的模式，有助于捕获常见错误
Set-StrictMode -Version Latest

# --- 配置变量 ---
# 项目文件路径
$projectPath = "src/BilibiliAutoLiver/BilibiliAutoLiver.csproj"
# 目标框架
$targetFramework = "net9.0"
# 项目基础名称，用于生成文件夹和压缩包文件名
$baseName = "BilibiliAutoLiver"
# 最终产出物（ZIP文件）的存放目录
$releaseDir = "release"
# 编译和打包的临时工作目录
$tempDir = Join-Path -Path $releaseDir -ChildPath "tmp"

# --- 定义构建目标 ---
# 每个目标包含运行时标识符(RID)和用于文件名的后缀
$targets = @(
    @{ RID = "win-x64";   Suffix = "Windows_x64" },
    @{ RID = "linux-x64"; Suffix = "Linux_x64" },
    @{ RID = "linux-arm64"; Suffix = "Linux_ARM64" }
)

# --- 脚本执行 ---

# 检查项目文件是否存在
if (-not (Test-Path $projectPath)) {
    Write-Host "错误：在 '$projectPath' 未找到项目文件。请确保脚本在正确的项目根目录下运行。" -ForegroundColor Red
    exit 1
}

# --- 准备目录 ---
# 创建或清空 release 目录
if (Test-Path $releaseDir) {
    Write-Host "正在清空已存在的 release 目录..." -ForegroundColor Yellow
    Remove-Item -Path $releaseDir -Recurse -Force
}
New-Item -ItemType Directory -Path $releaseDir | Out-Null
Write-Host "已创建 release 目录: $releaseDir" -ForegroundColor Green

# 创建临时工作目录
New-Item -ItemType Directory -Path $tempDir | Out-Null
Write-Host "已创建临时工作目录: $tempDir" -ForegroundColor Green

# 循环处理每一个构建目标
foreach ($target in $targets) {
    $rid = $target.RID
    $suffix = $target.Suffix
    
    # 定义将被打包的源文件夹名称
    $sourceFolderName = "$($baseName)_$($suffix)"
    # 定义源文件夹在临时目录中的完整路径
    $sourceFolderPath = Join-Path -Path $tempDir -ChildPath $sourceFolderName
    # 定义最终的ZIP压缩包完整路径
    $zipFilePath = Join-Path -Path $releaseDir -ChildPath "$($sourceFolderName).zip"

    Write-Host "------------------------------------------------------------"
    Write-Host "🚀 开始为 $($rid) 构建..." -ForegroundColor Cyan

    # 1. 发布项目
    # 将产出物发布到 release/tmp/ 目录下的对应文件夹
    Write-Host "   - 正在发布到 '$($sourceFolderPath)' 目录 (Single File, ReadyToRun)..."
    
    dotnet publish $projectPath `
        -c Release `
        -r $rid `
        -f $targetFramework `
        -o $sourceFolderPath `
        /p:PublishSingleFile=true `
        /p:PublishReadyToRun=true

    Write-Host "   - 发布完成！" -ForegroundColor Green

    # 2. 打包为ZIP文件
    # 从临时工作目录中找到源文件夹并进行压缩
    Write-Host "   - 正在将 '$($sourceFolderPath)' 文件夹打包为 '$($zipFilePath)'..."
    
    # 使用 Compress-Archive 命令创建ZIP文件
    # -LiteralPath 指定要压缩的文件夹的精确路径
    # -DestinationPath 指定输出的ZIP文件路径
    Compress-Archive -LiteralPath $sourceFolderPath -DestinationPath $zipFilePath -Force

    Write-Host "   - 打包完成！" -ForegroundColor Green
    
    Write-Host "✅ $($rid) 构建成功！" -ForegroundColor Green
}

Remove-Item -Path $tempDir -Recurse -Force

Write-Host "------------------------------------------------------------"
Write-Host "🎉 所有构建任务已成功完成！" -ForegroundColor Magenta
Write-Host "产出物位于 '$($releaseDir)' 目录下。"