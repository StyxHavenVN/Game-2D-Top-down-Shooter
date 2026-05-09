@echo off
title Launch Unity 6 with DirectX 11
echo Launching Unity Editor with DirectX 11 to prevent D3D12 crash...
start "" "C:\Program Files\Unity\Hub\Editor\6000.4.4f1\Editor\Unity.exe" -force-d3d11 -projectPath "%~dp0"
exit
