# Collaborate Client Package
This is the package to add Collaborate support to the Unity Editor. Unlike its predecessor CollabProxy,
this package interacts directly with the Git and Git-LFS binaries on the user's machine. As a result of this design
decision, the package is highly portable between Unity versions and maintains a high degree of backwards compatibility
for any new version of this package. Presently, 2019.3 and above are supported.

The project is exclusively targeting .NetStandard 2.0 and will not work with the legacy Mono runtime.

## Development
**For developers:** clone this repository out into the `packages/` directory in a project.
Another option is clone elsewhere and link with the `packages/manefest.json` file:
```
"com.unity.cloud.collaborate": "file:/some/path/to/package"
```

**For testers:** simply add the git url into the `packages/manefest.json` file:
```
"com.unity.source-control": "git://git@github.cds.internal.unity3d.com:unity/com.unity.cloud.collaborate.git"
```
If you need a specific revisision:
```
"com.unity.source-control": "git://git@github.cds.internal.unity3d.com:unity/com.unity.cloud.collaborate.git#<rev>"
```
If you need more information, read the [Documentation](https://docs.unity3d.com/Manual/upm-dependencies.html#Git) for package dependencies from git.

Code style is as dictated in [Unity Meta](https://github.cds.internal.unity3d.com/unity/unity-meta).

There are IDE Specific code style configs under the `Config/` directory in the above repo.

## Overview
Source code for the packages is contained within the `Source/` directory with a separate directory for each project
and the tests are in `Tests/`

Here are some files and folders of note:
```none
<root>
  ├── package.json
  ├── README.md
  ├── CHANGELOG.md
  ├── LICENSE.md
  ├── Third Party Notices.md
  ├── QAReport.md
  ├── Editor/
  │   ├── Unity.SourceControl.asmdef
  │   ├── Backend/
  │   │   └── Collaborate.cs
  │   ├── Common/
  │   └── UserInterface/
  │       ├── Api/
  │       ├── Components/
  │       ├── Resources/
  │       └── MainWindow.cs
  ├── Tests/
  │   ├── .tests.json
  │   └── Editor/
  │       └── Unity.SourceControl.Editor.Tests.asmdef
  └── Documentation~/
       ├── unity-source-control.md
       └── Images/
```

- `Editor/Backend` root directory of the collaborate backend source code.
- `Editor/Backend/Collaborate.cs` collection of logic to talk to git and the collaborate server.
- `Editor/Common` root directory of the git client source code.
- `Editor/UserInterface/` root directory of the Collaborate UI code.
- `Editor/UserInterface/Api/` directory for contracts used to communicate between the backend and UI.
- `Editor/UserInterface/Components/` directory for UiElements components used in the manager UI.
- `Editor/UserInterface/Resources/` directory with image, style (uss), and layout (uxml) files.
- `Editor/UserInterface/MainWindow.cs` Unity editor window for the Collaborate UI.
control system.
- `Tests/GitClient/` root directory of the git client tests.
- `Tests/Manager/` root directory of the source control manager tests.

Each directory contains a README file with additional details about what is contained within them, including code
examples.

## Package Information
For more info on packages and best practices, visit the [package-starter-kit](https://github.cds.internal.unity3d.com/unity/com.unity.package-starter-kit) repository and read the documentation.

## Known Issues
* No way to tell if the git work-tree has an in-progress rebase. This will break most commands.
