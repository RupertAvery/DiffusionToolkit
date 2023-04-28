del .\build /q
dotnet publish Diffusion.Toolkit\Diffusion.Toolkit.csproj -c Release -r win-x64 -o .\build --no-self-contained /p:PublishSingleFile=true /p:PublishReadyToRun=false /p:DebugType=None /p:DebugSymbols=false
dotnet publish Diffusion.Updater\Diffusion.Updater.csproj -c Release -r win-x64 -o .\build --no-self-contained /p:PublishSingleFile=true /p:PublishReadyToRun=false /p:DebugType=None /p:DebugSymbols=false
pause

