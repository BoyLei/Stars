@echo off
chcp 65001 >nul
echo 安装 Star 项目 git hooks...
echo.

git config core.hooksPath .githooks

echo ✅ hooksPath 已配置为 .githooks
echo    每次 git pull 后，变更文件列表将自动保存到 %%USERPROFILE%%\.star_md_sync\diff_files.txt
echo.
echo 团队成员每人执行一次本脚本即可生效。
pause
