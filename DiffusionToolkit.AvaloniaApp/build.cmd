dotnet publish DiffusionToolkit.AvaloniaApp.csproj -c Release -o build\win --runtime win-x64 -p:PublishSingleFile=true --self-contained false
dotnet publish DiffusionToolkit.AvaloniaApp.csproj -c Release -o build\osx --runtime osx-x64 -p:PublishSingleFile=true --self-contained false
dotnet publish DiffusionToolkit.AvaloniaApp.csproj -c Release -o build\linux --runtime linux-x64 -p:PublishSingleFile=true --self-contained false
