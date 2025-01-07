dotnet build -p:PublicRelease=true
IF ERRORLEVEL 1 ( EXIT /B %ERRORLEVEL% )
dotnet pack -p:PublicRelease=true
