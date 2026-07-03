echo off
set projectPath="E:/WorkSpace/Stars_Project/StarsProject_Client/trunk/Stars/"
set unity="C:/Program Files/Unity/Hub/Editor/2019.4.39f1c1/Editor/Unity.exe"
%unity%  -batchmode -quit -nographics -executeMethod MapEditor.MapEditor.ExportAllJson  -logFile "D:Editor.log" -projectPath %projectPath%
pause