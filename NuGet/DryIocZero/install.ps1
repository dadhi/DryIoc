param($installPath, $toolsPath, $package, $project)

$project.Object.References | 
    Where-Object { $_.Name -eq 'DryIoc.dll' -or $_.Name -eq 'ExpressionToCodeLib' } | 
    ForEach-Object { $_.Remove() }
