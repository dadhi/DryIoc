# Contributing

## Areas

Your contribution to the DryIoc project and extensions is greatly appreciated.

It may in a form of:

- Comments to existing issues.
- New issues.
- Pull Requests with improvements and issue fixes.
- Documentation error corrections and new content.
- [StackOverflow](http://stackoverflow.com/questions/tagged/dryioc) questions and answers. Please tag questions with __dryioc__ tag for easy finding.

## How to build and verify your changes

### Build from command line

__Before sending me PR__ please build solution with _build.bat_ located in root folder.

It will build all projects, will run unit tests with coverage, and will create NuGet packages.

Ensure that there are no project build errors or failing tests. Also check package creation errors for obvious issues, e.g. missing files.

### Visual Studio solution

Open _DryIoc.sln_ located in root folder to develop in Visual Studio 2013. 

Solution combines projects from all supported platform starting from .NET 3.5 (may be changed in future).

__Hint:__ To simplify development you may unload projects under solution root and platform folders except the one default platform you are working with. Usually in development mode I am unloading all except projects in _Net45_ and _Extensions_ folder. But before commit run _build.bat_ which builds for all platforms.

__Hint:__ Solution is configured to be built and run Unit Tests continuously with [NCrunch](http://www.ncrunch.net/). That's how I am usually develop - very rarely building project by hand.

### FAQ

todo:




