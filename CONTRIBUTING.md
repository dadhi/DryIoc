# Contributing

## Areas

Your contribution to the DryIoc project and extensions is highly appreciated.

Changes can come in a form of:

- Comments to existing issues
- New issues
- Pull Requests with improvements, failed tests for the found bugs, and bug fixes
- Documentation error corrections and new content
- [StackOverflow](http://stackoverflow.com/questions/tagged/dryioc) questions and answers. Please tag questions with __dryioc__ tag for easy finding.

### How to report issue step-by-step

If you found the problem with DryIoc:

 - Please check that you are using the latest DryIoc version `PM> Install-Package DryIoc -Pre`
 - Then [create new issue](https://bitbucket.org/dadhi/dryioc/issues/new) with problem description.
 - Optionally: to get the fast feedback, fast fix, and generally to make me happy (: 
     - [Fork DryIoc](https://bitbucket.org/dadhi/dryioc/fork)
     - Add failing tests reproducing your case into [DryIoc.IssuesTests](https://bitbucket.org/dadhi/dryioc/src/8510666893daaea1d07b49ba0dfcbf3f95dcccd4/Net45/DryIoc.IssuesTests/?at=dev) project. Check other files in project for general guidelines, but nothing is strict here
     - Push your tests and create Pull Request
     
    Thank you!


## How to build and verify your changes

### Build from the command line 

Before sending a Pull Request, please build solution with _build.bat_ located in the root folder.

It will build all projects, run unit tests with coverage, and create NuGet packages.

Make sure that there are no project build errors or failing tests. Also, check package creation errors for obvious issues, e.g. missing files.

### Visual Studio solutionm

Open _DryIoc.sln_ located in the root folder to develop in Visual Studio 2013 and above. 

Solution combines projects for all supported platforms starting from .NET 3.5 (may be changed in future).

__HINT:__ To simplify development, you may unload projects under solution root and platform folders except the platform you are working with.  
Usually, I unload everything except projects in _Net45_ and _Extensions_ folders. But before commit, I do run _build.bat_ which builds all platforms.

__HINT:__ Solution is configured to be built and to run Unit Tests continuously with [NCrunch](http://www.ncrunch.net/). NCrunch allows developing with minimum manual project builds.






