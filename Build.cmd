@mkdir .\bin
dotnet restore ./src/ElectricFieldVis.sln
dotnet msbuild ./src/ElectricFieldVis.sln -t:Rebuild -p:Configuration=Release
xcopy .\src\bin\Release\net8.0-windows\*.* .\bin /S
