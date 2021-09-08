cd II Avalonia\bin

del /q /s *
rmdir /q /s Debug
rmdir /q /s Release

cd ..

dotnet clean
dotnet build -c Release
dotnet publish -c Release -r win-x64
dotnet publish -c Release -r linux-x64
dotnet publish -c Release -r osx-x64

cd bin\Release\net5.0\win-x64\
del /q *
move publish "Infirmary Integrated"
tar -c -f Windows.zip "Infirmary Integrated"
move Windows.zip ..\..\..

cd ..\linux-x64
del /q *
move publish "Infirmary Integrated"
tar -c -f Linux.zip "Infirmary Integrated"
move Linux.zip ..\..\..

cd ..\osx-x64
del /q *
move publish "Infirmary Integrated"
tar -c -f OSX.zip "Infirmary Integrated"
move OSX.zip ..\..\..

cd ..\..\..\..\..