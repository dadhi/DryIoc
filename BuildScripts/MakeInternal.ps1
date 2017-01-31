$inputFiles = @(
	".\DryIoc\Container.cs",
	".\DryIoc\ImTools.cs",
	".\DryIoc\FastExpressionCompiler.cs",
	".\DryIoc\Ported-net40.cs",
	".\DryIoc\Ported-net45.cs",
	".\Net45\DryIoc\AsyncExecutionFlowScopeContext.cs"
)
$outputFolder = ".\DryIoc.Internal"

New-Item -ItemType Directory -Force -Path $outputFolder | Out-Null
ForEach ($file in $inputFiles)
{
	$content = Get-Content -path $file
	$content = $content -creplace "public(?=\s+(((abstract|sealed|static)\s+)?(partial\s+)?class|delegate|enum|interface|struct))", "internal"
	
	$outputPath = Join-Path $outputFolder (Split-Path $file -Leaf)
	Out-File $outputPath UTF8 -InputObject $content
}