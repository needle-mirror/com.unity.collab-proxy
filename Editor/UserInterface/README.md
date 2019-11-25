# Unity Source Control User Interface
This directory contains the logic and resource that make up the Collaborate UI.

## Overview
This is the structure of the directory:
```none
<root>
  ├── Api/
  ├── Components/
  ├── Exceptions/
  ├── Resources/
  │   ├── Icons/
  │   ├── Layouts/
  │   └── Styles/
  ├── BackendProvider.cs
  ├── MainWindow.cs
  └── WindowCache.cs
```
The `Api/` directory contains the contract between the UI and the backend code.

The `Components/` directory contains all UiElement classes for the components in the UI. Each class extends VisualElement,
specifies one element that may or may not depend on other components, implements a factory class such that it can be
instantiated in UXML, and includes a layout via a respective UXML file in the `Resources/Layouts/` directory. Please
view the README in the directory for more information on how to create a new component.

The `Exceptions/` directory includes a few general exceptions that are expected to be used by the UI and any source
control providers.

The `Resources/` directory contains the non-code assets for the package. Currently limited to just layouts (UXML) and
styles (USS), but this will extend to images in the near future as the UI is developed. Please view the README in the
directory for more information about how to add or modify these files.

`BackendProvider.cs` provides a singleton that allows the UI to send and receive data with the backend.

`MainWindow.cs` is the entry point for the user interface. It spawns a EditorWindow and sets up the UI.

`WindowCache.cs` provides a collection of fields that are preserved during domain reload and editor restart. Some 
examples are the the current commit message and the currently selected items for the simple UI/UX. Any data that would 
impact UX if lost during reload or exit, should be saved in here.
