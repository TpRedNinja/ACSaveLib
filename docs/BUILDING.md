# Building

Needs .NET 8 SDK. Project has no NuGet dependencies.
```
dotnet build AcSaveLib.sln
```
In the sandbox the NuGet proxy returns 403; work around with an empty feed:
```
mkdir -p /tmp/emptyfeed
dotnet restore --source /tmp/emptyfeed
dotnet build --no-restore
```
WPF host projects (Route Tracker) can't be built here; build them in Visual Studio. .save files and samples/ are gitignored.
