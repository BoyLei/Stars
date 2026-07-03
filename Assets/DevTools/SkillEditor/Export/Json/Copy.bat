DEL ..\..\..\..\..\..\..\..\StarsProject_Server\trunk\gameserver\res\battle /S/F/Q
DEL ..\..\..\..\..\..\..\..\StarsProject_Design\trunk\配置文件\battle /S/F/Q

for /f "delims=" %%i in ('dir ..\..\..\..\..\..\..\..\StarsProject_Client\trunk\Stars\Assets\Res\Config\Skill /a-d /s /b') do (
if not %%~xi equ .meta (
if exist "%%i" ( del /s/a "%%i" >nul ) ) )

xcopy *.Json  ..\..\..\..\..\..\..\..\StarsProject_Server\trunk\gameserver\res\battle /S/Y/Q
xcopy *.Json  ..\..\..\..\..\..\..\..\StarsProject_Design\trunk\配置文件\battle /S/Y/Q
xcopy Skill\*.Json ..\..\..\..\..\..\..\..\StarsProject_Client\trunk\Stars\Assets\Res\Config\Skill /S/Y/Q
xcopy Buff\*.Json ..\..\..\..\..\..\..\..\StarsProject_Client\trunk\Stars\Assets\Res\Config\Skill /S/Y/Q
xcopy Bullet\*.Json ..\..\..\..\..\..\..\..\StarsProject_Client\trunk\Stars\Assets\Res\Config\Skill /S/Y/Q
xcopy Passive\*.Json ..\..\..\..\..\..\..\..\StarsProject_Client\trunk\Stars\Assets\Res\Config\Skill /S/Y/Q
xcopy FxDetail\*.Json ..\..\..\..\..\..\..\..\StarsProject_Client\trunk\Stars\Assets\Res\Config\Skill /S/Y/Q

pause