@echo off
cls
if not exist tools\FAKE\tools\Fake.exe ( 
  .nuget\nuget.exe install FAKE -OutputDirectory tools -ExcludeVersion -Prerelease
)
tools\FAKE\tools\Fake.exe build.fsx %* Clean
pause
