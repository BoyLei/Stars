del ..\..\..\..\StarsProject_Server\trunk\gameserver\res\space /S /Q /F 
rd ..\..\..\..\StarsProject_Server\trunk\gameserver\res\space /S /Q

md ..\..\..\..\StarsProject_Server\trunk\gameserver\res\space

xcopy *.json ..\..\..\..\StarsProject_Server\trunk\gameserver\res\space /Y /E

del ..\..\..\..\StarsProject_Client\trunk\Stars\Assets\Res\MapData /S /Q /F 
rd ..\..\..\..\StarsProject_Client\trunk\Stars\Assets\Res\MapData /S /Q

md ..\..\..\..\StarsProject_Client\trunk\Stars\Assets\Res\MapData

xcopy *.json ..\..\..\..\StarsProject_Client\trunk\Stars\Assets\Res\MapData /Y /E
pause

