# Contributing

## Areas

Your contribution to the DryIoc project and extensions is highly appreciated.

It may come as

- Comments to existing issues
- New issues: bugs, enhancements, and feature proposals
- Pull Requests with improvements, failed tests for the found bugs, and bug fixes
- Documentation error corrections and new documentation content
- [StackOverflow](http://stackoverflow.com/questions/tagged/dryioc) questions and answers. Please tag questions with __dryioc__ tag for easy finding.


### How to report issue step-by-step

If you found the problem with DryIoc:

 - Please check that you are using the [latest DryIoc version](https://bitbucket.org/dadhi/dryioc/wiki/Home#markdown-header-latest-version).
 - [Create new issue](https://bitbucket.org/dadhi/dryioc/issues/new) with problem description.
 - To get faster feedback, faster fixes, and generally to make me happy :) 
     - [Fork DryIoc](https://bitbucket.org/dadhi/dryioc/fork)
     - Add failing tests reproducing your case into [Net45\DryIoc.IssuesTests](https://bitbucket.org/dadhi/dryioc/src/8510666893daaea1d07b49ba0dfcbf3f95dcccd4/Net45/DryIoc.IssuesTests/?at=dev) project. Check other files in project for general guidelines, but nothing is strict here
     - Commit your tests and create Pull Request for me to merge
     
    Thank you!


## How to build and verify your changes

### Build from the command line 

Before sending a Pull Request, please build solution with _build.bat_ located in the root folder.

It will build all projects, run unit tests with coverage, and create NuGet packages.

Make sure that there are no project build errors or failing tests. Also, check package creation errors for obvious issues, e.g. missing files.

__Note:__ For .NET Core DependencyInjection adapter please run _NetCore\build.bat_ to run tests and create package. 


### Develop in Visual Studio

There are 3 solutions:

- _DryIoc.start.sln_ contains "starting" set of projects targeting .NET 4.5 only. __It is may be all you need.__
- _DryIoc.extenstions.sln_ Adds to _DryIoc.start.sln_ all extentions targeting .NET 4.5.
- _DryIoc.sln_ includes every project except for .Net Core for every target platform.
- _NetCore/DryIoc.Microsoft.DependencyInjection.sln_ for [.NET Core and DryIoc.Microsoft.DependencyInjection](https://github.com/aspnet/dependencyinjection).

__Hint:__ Usually I am enabling [NCrunch](http://www.ncrunch.net/) to build and run Unit Tests continuously, saves me a lot of time.
