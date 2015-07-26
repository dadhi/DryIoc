# Contributing

## Areas

Your contribution to the DryIoc project and extensions is greatly appreciated.

It may come in a form of:

- Comments to existing issues.
- New issues.
- Pull Requests with improvements and issue fixes.
- Documentation error corrections and new content.
- [StackOverflow](http://stackoverflow.com/questions/tagged/dryioc) questions and answers. Please tag questions with __dryioc__ tag for easy finding.

## How to build and verify your changes

### Build from command line

Before sending me Pull Request please build solution with _build.bat_ located in root folder.

It will build all projects, will run unit tests with coverage, and will create NuGet packages.

Ensure that there are no project build errors or failing tests. Also check package creation errors for obvious issues, e.g. missing files.

### Visual Studio solution

Open _DryIoc.sln_ located in root folder to develop in Visual Studio 2013. 

Solution combines projects for all supported platforms starting from .NET 3.5 (may be changed in future).

__HINT:__ To simplify development you may unload projects under solution root and platform folders except the one platform you are working with.  
Usually I am unloading everything except projects in _Net45_ and _Extensions_ folders. But before commit I do run _build.bat_ which builds all platforms.

__HINT:__ Solution is configured to be built and run Unit Tests continuously with [NCrunch](http://www.ncrunch.net/). That's how I am usually develop - very rarely building project manually.

### FAQ

todo:




