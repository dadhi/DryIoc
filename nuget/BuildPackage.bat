md ..\bin\nuget
call nuget pack DryIoc.nuspec -NonInteractive -OutputDirectory ..\bin\nuget
call nuget pack DryIoc.Code.nuspec -NonInteractive -OutputDirectory ..\bin\nuget