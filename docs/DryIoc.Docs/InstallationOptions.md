# Installation Options

[TOC]

## Source and Binary NuGet packages

DryIoc and its extensions distributed via [NuGet](https://www.nuget.org/packages?q=dryioc) either as a __Source__ or a Binary package.

__Note:__ Source code is the default way of distribution (e.g. __DryIoc__ package) for a historical reasons. If you want a "normal" assembly reference, please use the packages with `.dll` suffix (e.g. __DryIoc.dll__ package).

### Source package 

__DryIoc package__ contains source files: _Container.cs_ and maybe couple of others depending on target framework. All the files will be located in the target project in _DryIoc_ folder.

`PM> Install-Package DryIoc`

### Binary package 

__DryIoc.dll package__ has a `.dll` suffix and contains assembly that normally referenced by target project.

`PM> Install-Package DryIoc.dll`


## Assembly Signing

By default DryIoc binaries provided without strong-signing. For those who requires strong-signing DryIoc package includes two files:

- _tools\SignPackageAssemblies.ps1_
- _tools\DryIoc.snk_

Run _SignPackageAssemblies.ps1_ from __NuGet Package Manager -> Package Manager Console__ to sign the installed DryIoc assemblies with _DryIoc.snk_ key.
