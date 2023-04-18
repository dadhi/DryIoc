# Contributing


- [Contributing](#contributing)
  - [Overview](#overview)
  - [How to report an issue step-by-step](#how-to-report-an-issue-step-by-step)
  - [How to build and develop the DryIoc](#how-to-build-and-develop-the-dryioc)
    - [Build from the command line](#build-from-the-command-line)
    - [Develop in Visual Studio 2017+](#develop-in-visual-studio-2017)
    - [Develop in Visual Studio Code](#develop-in-visual-studio-code)
  - [Contributing to the documentation](#contributing-to-the-documentation)
    - [TL;DR;](#tldr)
    - [Documentation in DryIoc explained](#documentation-in-dryioc-explained)


## Overview

Your help is highly appreciated. Thank you!

You may help via:

- Commenting on the existing issues in the [DryIoc GitHub repository](https://github.com/dadhi/DryIoc).
- Opening new issues for bugs, enhancements, and feature proposals.
- Creating Pull Requests with the improvements, failed tests for the found bugs, and the bug-fixes.
- [Correcting the documentation errors](#contributing-to-the-documentation) and submitting the new documentation topics.
- Asking and answering the questions on [StackOverflow](http://stackoverflow.com/questions/tagged/dryioc), tagging your questions with `dryioc` tag for easy finding.
- Discussing the problems and ideas in the [Gitter](https://gitter.im/dadhi/DryIoc) or [Slack](https://dryioc.slack.com) rooms.
- Poking me on [Twitter](http://twitter.com/intent/user?screen_name=DryIoc).


## How to report an issue step-by-step

If you found the problem with DryIoc:

 - Please check that you are using the [latest DryIoc version](https://github.com/dadhi/DryIoc/blob/master/docs/DryIoc.Docs/Home.md#latest-version).
 - [Create new issue](https://github.com/dadhi/DryIoc/issues/new) with problem description.
 - To get faster feedback, faster fixes, and generally to make me happy :-) 
     - [Fork DryIoc](https://github.com/dadhi/DryIoc/fork)
     - Add failing tests reproducing your case to `test\DryIoc.IssuesTests` project. Check other files in the project for general guideline, but nothing is strict here.
     - **Ignore** the failing tests with `[Ignore("fixme")]` attribute, so that CI build should pass.
     - Commit your tests and create a Pull Request for me to review.


## How to build and develop the DryIoc

### Build from the command line 

Before sending the Pull Request please build the solution with the `build.bat` located in the root folder.

It will build all projects for in the Release configuration, will run unit tests and [generate the documentation](#tldr).
Make sure that there are no project build errors or failing tests.

### Develop in Visual Studio 2017+

Open `DryIoc.sln` solution and re-build it. If something is failing you may try to close VS, run `build.bat` in the root folder, open VS and try to build again.

__Note:__ DryIoc targets multiple platforms (via msbuild project multi-targeting) which makes it slower to build. 

__Productivity hint:__ I am using [NCrunch](http://www.ncrunch.net/) extension for the MS Visual Studio to build and run the tests continuously and to get the immediate feedback, quickly find regressions, and generally experiment with the code.

### Develop in Visual Studio Code

DryIoc provides the friendly VSCode developing experience. 

Basically you need just the **C# extension** installed. 

Then you may run the `build.bat` from the shell to ensure packages are restored, code is built, tests are passing and packages are created.

You may go to the test projects in the _test_ sub-folder and Run or Debug the tests via the editor lens provided by the extension.

## Contributing to the documentation

### TL;DR;

- Edit the selected document **.cs** (not the .md) file in the _docs\DryIoc.Docs_ sub-folder.
- Run the `build_the_docs.bat` file from the root folder in the shell.
- Check the updated .md file. You may preview the markdown files with the respective markdown extension in VSCode or in Visual Studio.
- Commit the changes both for .cs and .md and follow the usual PR GitHub workflow.

### Documentation in DryIoc explained

DryIoc uses the **compile-able runnable documentation** written in `.cs` C# files in the markdown format - [example](https://github.com/dadhi/DryIoc/blob/master/docs/DryIoc.Docs/CreatingAndDisposingContainer.cs).
That's a lot to say :-) so let me explain...

Markdown text is placed inside the `/*md ... md*/` comments and 
the examples are just the unit test classes outside of the markdown comment blocks.

The documentation files are included into the normal NUnit test project _docs\DryIoc.Docs_ in the DryIoc solution. 
This way the docs are compiled as a normal code and __the example tests are up-to-date__ and can be run locally or on the CI side.

The documentation `.cs` files are converted to the markdown `.md` file via [CsToMd Visual Studio extension](https://marketplace.visualstudio.com/items?itemName=dadhi.cstomd123) 
and via the [dotnet-cstomd](https://www.nuget.org/packages/dotnet-cstomd) dotnet CLI local tool which is **already installed when you build the project**.
There is a [CsToMd repository](https://github.com/dadhi/CsToMd) with more information.

When installing the extension for the Visual Studio you will see that `DryIoc.Docs` project has a `.md` files located under corresponding the `.cs` files.

Try edit the `.cs` file then save it and see that the `.md` file is automatically changed by the extension.

The result markdown file may be automatically previewed inside the Visual Studio with the help of 
[Markdown Editor extension](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.MarkdownEditor).

In VSCode you need to run the `build_the_docs.bat` to see the changes or just build the `DryIoc.Docs` project in the Debug mode (the default).
