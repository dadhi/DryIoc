param($installPath, $toolsPath, $package, $project)

$project.Object.References | 
    Where-Object { $_.Name -eq 'DryIoc.' -or $_.Name -eq 'ExpressionToCodeLib' } | 
    ForEach-Object { $_.Remove() }


$Regs = $project.ProjectItems.Item("DryIocZero").ProjectItems.Item("Registrations.tt");
$Regs.Properties.Item("BuildAction").Value = [int]0;
$Regs.Properties.Item("CustomTool").Value = [string]'';
