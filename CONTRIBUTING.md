# Contributing

## In general

Your contribution to the DryIoc project and extensions is highly appreciated, and will help to evolve the project.

You may help via:

- Commenting on existing issues
- Opening new issues for bugs, enhancements, and feature proposals
- Creating Pull Requests with improvements, failed tests for the found bugs, and bug fixes
- Correcting documentation errors and submitting new documentation topics
- Asking and answering questions on [StackOverflow](http://stackoverflow.com/questions/tagged/dryioc), tagging your questions with `dryioc` tag for easy finding
- Discussing the problems and ideas in [Gitter](https://gitter.im/dadhi/DryIoc) or [Slack](https://dryioc.slack.com) rooms
- Poking me on [Twitter](http://twitter.com/intent/user?screen_name=DryIoc)


## How to report an issue step-by-step

If you found the problem with DryIoc:

 - Please check that you are using the [latest DryIoc version](https://bitbucket.org/dadhi/dryioc/wiki/Home#markdown-header-latest-version).
 - [Create new issue](https://github.com/dadhi/DryIoc/issues/new) with problem description.
 - To get faster feedback, faster fixes, and generally to make me happy :-) 
     - [Fork DryIoc](https://github.com/dadhi/DryIoc/fork)
     - Add failing tests reproducing your case to `test\DryIoc.IssuesTests` project. Check other files in the project for general guideline, but nothing is strict here.
     - **Ignore** the failing tests with `[Ignore("fixme")]` attribute, so that CI build pass.
     - Commit your tests and create a Pull Request for me to review.
    
Thank you!


## How to build and develop the DryIoc

### Build from the command line 

Before sending a Pull Request, please build solution with `build.bat` located in the root folder.

It will build all projects for __all platforms__ in Release configuration, run unit tests, and create NuGet packages.
Make sure that there are no project build errors or failing tests.

### Develop in Visual Studio 2017+

Open `DryIoc.sln` solution and re-build it. If something is failing, you may try to close VS, open command line and run `dotnet clean`, then `build.bat` in solution folder, open VS and try to build again.

__Note:__ Projects in the solution multi-target multiple platforms, e.g. `DryIoc` targets 6+ platforms which makes it slower to build. To speedup the development DryIoc has `<DevMode>true</DevMode>` MSBuild proprerty set to `true` in `Directory.Build.props` file in the solution root folder. This setting minimizes the number of platforms to target. That's why you need to run `build.bat` to test your work on all platforms.

__Productivity hint:__ Usually I am using [NCrunch](http://www.ncrunch.net/) to build and run tests continuously, to get an immediate feedback, quickly find regressions, and generally experiment with code.

### Develop in Visual Studio Code

It is possible to develop and even run tests (via plugin) in VS Code. 
