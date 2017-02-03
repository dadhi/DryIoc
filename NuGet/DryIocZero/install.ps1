param($installPath, $toolsPath, $package, $project)

$project.Object.References | 
    Where-Object { $_.Name -eq 'DryIoc.' -or $_.Name -eq 'ExpressionToCodeLib' } | 
    ForEach-Object { $_.Remove() }


$Regs = $project.ProjectItems.Item("DryIocZero").ProjectItems.Item("Registrations.tt");
$Regs.Properties.Item("BuildAction").Value = [int]0; // None=0, Compile=1, Content=2
$Regs.Properties.Item("CustomTool").Value = [string]''; // No tool, as registrations are only for inclusion into Ciontainer.Generated.tt
