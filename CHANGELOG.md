# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2.0.0-preview.8] - 2019-4-8
Performance fixes to reduce CPU load.

## [2.0.0-preview.7] - 2019-3-25
Update CHANGELOG.md

## [2.0.0-preview.6] - 2019-3-23
Removed dependency on third-party moq.dll
Bug fixes to serialization when using legacy scripting runtime.
Bug fixes to remove migration dialog that should be forced.
Performance improvements to reduce polling frequency and unnecessary file hashes.

## [2.0.0] - 2019-1-22
Bump major semantic version to reflect move from Unity.CollabProxy.Editor.

## [1.3.0-preview.4] - 2018-12-17
Serialization updates

## [1.3.0-preview.3] - 2018-12-14
Further bugfixes related to updating the state of the Git repository asynchronously.

## [1.3.0-preview.2] - 2018-09-28
Bugfixes related to updating the state of the Git repository asynchronously.

## [1.3.0-preview.1] - 2018-09-20
Switched from snapshot system to Git repository, addressing stability issues.

## [1.2.11] - 2018-09-04
Made some performance improvements to reduce impact on ReloadAssemblies.

## [1.2.9] - 2018-08-13
Test issues for the Collab History Window are now fixed.

## [1.2.7] - 2018-08-07
Toolbar drop-down will no longer show up when package is uninstalled.

## [1.2.6] - 2018-06-15
Fixed an issue where Collab's History window wouldn't load properly.

## [1.2.5] - 2018-05-21
This is the first release of *Unity Package CollabProxy*.

### Added
- Collab history and toolbar windows
- Collab view and presenter classes
- Collab Editor tests for view and presenter
