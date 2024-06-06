SET AVALONIA_PROJECT=DiffusionToolkit.AvaloniaApp\DiffusionToolkit.AvaloniaApp.csproj
SET OPTIONS=-c Release -p:PublishSingleFile=true --self-contained false
REM dotnet publish %AVALONIA_PROJECT% %OPTIONS% -o build\win-x64 --runtime win-x64
REM dotnet publish %AVALONIA_PROJECT% %OPTIONS% -o build\osx-x64 --runtime osx-x64
REM dotnet publish %AVALONIA_PROJECT% %OPTIONS% -o build\linux-x64 --runtime linux-x64
dotnet restore -r osx-x64
dotnet msbuild %AVALONIA_PROJECT% -r -t:BundleApp -p:RuntimeIdentifier=osx-x64 -p:UseAppHost=true
pause