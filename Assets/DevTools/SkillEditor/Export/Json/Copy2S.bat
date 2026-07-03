DEL ..\..\..\..\..\..\..\..\StarsProject_Server\trunk\gameserver\res\battle /S /Q /F 
DEL ..\..\..\..\..\..\..\..\StarsProject_Design\trunk\配置文件\battle /S /Q /F 

xcopy *.Json  ..\..\..\..\..\..\..\..\StarsProject_Server\trunk\gameserver\res\battle /S/Y
xcopy *.Json  ..\..\..\..\..\..\..\..\StarsProject_Design\trunk\配置文件\battle /S/Y
pause