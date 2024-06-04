SET AVALONIA_PROJECT=DiffusionToolkit.AvaloniaApp\DiffusionToolkit.AvaloniaApp.csproj
SET OPTIONS=-c Release -p:PublishSingleFile=true --self-contained false
dotnet publish %AVALONIA_PROJECT% %OPTIONS% -o build\win-x64 --runtime win-x64
dotnet publish %AVALONIA_PROJECT% %OPTIONS% -o build\osx-x64 --runtime osx-x64
dotnet publish %AVALONIA_PROJECT% %OPTIONS% -o build\linux-x64 --runtime linux-x64
