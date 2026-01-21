::dotnet publish -f net10.0-windows10.0.19041.0 -c Release -p:WindowsPackageType=Exe --self-contained true -r win-x64 /p:PublishSingleFile=True
::dotnet publish -f net10.0-windows10.0.19041.0 -c Release -p:WindowsPackageType=None
::dotnet publish -f net10.0-windows10.0.19041.0 -c Release -p:WindowsPackageType=None --self-contained true /p:PublishSingleFile=True
::dotnet publish  -f net10.0-windows10.0.19041.0 -c Release -p:WindowsPackageType=None -p:PublishSingleFile=true --self-contained true -p:WindowsAppSDKSelfContained=true   
dotnet publish  -f net10.0-windows10.0.19041.0 -c Release -p:WindowsPackageType=None --self-contained true -p:WindowsAppSDKSelfContained=true
