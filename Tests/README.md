# Unity Source Control Tests
This project contains the tests for the 3 current projects in `com.unity.source-control`.

## Overview
This is the structure of the project:
```none
<root>
  ├── .tests.json
  └── Editor/
      ├── Unity.SourceControl.Editor.Tests.asmdef
      ├── GitClient/
      └── Manager/
```

The `GitClient/` directory contains tests for the git client source code.

The `Manager/` directory contains tests for the source control manager source code.

## Tests
To run the tests, use the Unity Test Runner from within the Unity Editor. Unity Test Runner documentation is [here](https://docs.unity3d.com/Manual/testing-editortestsrunner.html).

## Adding a Test
While 100% coverage is hard to achieve, tests should be added with each new feature to ensure coverage either remains constant or increases.

With that out of the way, tests are in the typical C# format with a function with a `[Test]` decorator. Below is an example of a test taken from `GitClient/Tasks/GitVersionTests.cs`
```csharp
[Test]
public void Can_Handle_Output()
{
    var task = new TestGitVersionTask();
    const string rString = "git version 2.20.1.windows.1";
    var result = task.TestHandleOutput(0, rString, "");
    Assert.That(result.Major == 2);
    Assert.That(result.Minor == 20);
    Assert.That(result.Fix == 1);
}
```
For documentation on the testing library, look at the NUnit [documentation](https://github.com/nunit/docs/wiki/NUnit-Documentation) over at GitHub. Unity Test Runner is a superset of NUnit and the documentation for that is [here](https://docs.unity3d.com/Manual/testing-editortestsrunner.html).


To access private/internal classes, creating a subclass and marking the parent fields as protected/internal will allow them to be used in testing.
This is an example from `Editor/GitClient/Tasks/GitVersionTests.cs` where access to a private field and method is required:
```csharp
class TestGitVersionTask : GitVersionTask
{
    public IList<string> GetArgs()
    {
        return Arguments;
    }

    public GitVersion TestHandleOutput(int exitCode, string stdOut, string stdErr)
    {
        return HandleOutput(exitCode, stdOut, stdErr);
    }
}
```
Use of this testing wrapper is shown above in the example test.
