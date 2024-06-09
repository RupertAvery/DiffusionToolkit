AVALONIA_PROJECT=DiffusionToolkit.AvaloniaApp/DiffusionToolkit.AvaloniaApp.csproj
dotnet publish $AVALONIA_PROJECT -c Release -p:PublishSingleFile=true --self-contained false -o build/linux-x64 --runtime linux-x64
chmod +x build/linux-x64/DiffusionToolkit.AvaloniaApp
