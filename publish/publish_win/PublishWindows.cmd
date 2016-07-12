rmdir /s /q MCast
mkdir MCAst
xcopy /s mlp MCAst\
xcopy /s ..\source\* MCAst
xcopy ssview.cmd MCAst
xcopy mcast.cmd MCAst
mkdir MCAst\screenshots
mkdir MCAst\video
