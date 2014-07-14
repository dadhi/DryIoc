# Sign package using Nivot.StrongNaming <https://www.nuget.org/packages/Nivot.StrongNaming/>

$scriptDir=[System.IO.Path]::GetDirectoryName($MyInvocation.MyCommand.Path)
pushd $scriptDir
Try 
{
    $assemblyPath=".\lib\net40\*.dll"
    $nuget="..\..\.nuget\NuGet.exe"
    $packagesDir=".."

    &$nuget Install Nivot.StrongNaming -OutputDirectory $packagesDir -ExcludeVersion -NonInteractive
    Import-Module (Join-Path $packagesDir "Nivot.StrongNaming\tools\StrongNaming.psd1")

    $snk=Import-StrongNameKeyPair ".\DryIoc.snk"
    Get-ChildItem $assemblyPath | % { Set-StrongName $_ -Keypair $snk -Force -NoBackup }

	echo "Finished Successfully!"
}
Catch
{
	echo "Failed."
}
Finally
{
    popd
}

Write-Host "Press any key to exit ..."
$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")