AVALONIA_PROJECT=DiffusionToolkit.AvaloniaApp/DiffusionToolkit.AvaloniaApp.csproj
dotnet restore -r osx-x64
dotnet msbuild $AVALONIA_PROJECT -r -t:BundleApp -p:RuntimeIdentifier=osx-x64 -p:UseAppHost=true -p:OutputPath=./build/osx-x64/
mkdir -p ./build/osx-x64
cp -r DiffusionToolkit.AvaloniaApp/build/osx-x64/publish/Diffusion\ Toolkit.app build/osx-x64/Diffusion\ Toolkit.app
chmod +x build/osx-x64/Diffusion\ Toolkit.app/Contents/MacOS/launch.sh
chmod +x build/osx-x64/Diffusion\ Toolkit.app/Contents/MacOS/DiffusionToolkit.AvaloniaApp


