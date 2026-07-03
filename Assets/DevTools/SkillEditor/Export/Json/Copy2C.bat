for /f "delims=" %%i in ('dir ..\..\..\..\..\..\..\..\StarsProject_Client\trunk\Stars\Assets\Res\Config\Skill /a-d /s /b') do (
if not %%~xi equ .meta (
if exist "%%i" (
    del /s /a /q "%%i" >nul
        )
    )
)

xcopy Skill\*.Json ..\..\..\..\..\..\..\..\StarsProject_Client\trunk\Stars\Assets\Res\Config\Skill /y
xcopy Buff\*.Json ..\..\..\..\..\..\..\..\StarsProject_Client\trunk\Stars\Assets\Res\Config\Skill /y
xcopy Bullet\*.Json ..\..\..\..\..\..\..\..\StarsProject_Client\trunk\Stars\Assets\Res\Config\Skill /y
xcopy Passive\*.Json ..\..\..\..\..\..\..\..\StarsProject_Client\trunk\Stars\Assets\Res\Config\Skill /y
xcopy FxDetail\*.Json ..\..\..\..\..\..\..\..\StarsProject_Client\trunk\Stars\Assets\Res\Config\Skill /y
pause