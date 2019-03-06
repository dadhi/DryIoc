# Installation Options

[TOC]

## Source and Binary NuGet packages

DryIoc and its extensions distributed via [NuGet](https://www.nuget.org/packages?q=dryioc) either as a __source__ or a __binary__ package.

__Note:__ Source code is the default way of distribution (e.g. __DryIoc__ package) for a historical reasons. If you want a normal assembly reference, please use the packages with `.dll` suffix (e.g. __DryIoc.dll__ package).

### Source package 

__DryIoc package__ contains source files: _Container.cs_ and maybe couple of others depending on target framework. All the files will be located in the target project in _DryIoc_ folder.

`PM> Install-Package DryIoc`

### Binary package 

__DryIoc.dll package__ has a `.dll` suffix and contains assembly that normally referenced by target project.

`PM> Install-Package DryIoc.dll`


## Assembly Signing

The `.dll` packages are signed starting from the DryIoc V4 version and the extension version's released at the same time.

The `DryIoc.snk` file itself is included into the package and published to [GitHub repo](https://github.com/dadhi/DryIoc/blob/master/DryIoc.snk).
