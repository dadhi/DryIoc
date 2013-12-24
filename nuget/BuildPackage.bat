md ..\bin\nuget
call nuget pack DryIoc.nuspec -NonInteractive -OutputDirectory ..\bin\nuget
call nuget pack DryIoc.dll.nuspec -NonInteractive -OutputDirectory ..\bin\nuget