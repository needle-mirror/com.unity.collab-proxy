# Resources 
This directory contains the non-code assets for the Collaborate UI.

## Overview

```
Icons/
Layouts/
Styles/
```
The directory for images will be added once it is needed.

## Editing
USS and UXML files are inspired by the respective CSS and XML files. USS is a non-struct subset of CSS and for the 
manager there are three primary USS files. `styles.uss`, `dark_styles.uss`, and `light_styles.uss` where the 
non-colour/image live in `styles` and the rest lay in the respective dark/light file.

Documentation about the two file types is provided within the Unity documentation for UiElements:
https://docs.unity3d.com/2019.1/Documentation/Manual/UIElements.html

In general each component and page will have its own layout file. When adding new components with their uxml factories
the UiElements schema will need to updated within the editor then copied into the project before they can be referenced
in other UXML files. Click `Assets/Update UiElements Schema` in the Editor and then copy the schema files from the
`UIElementsSchema` directory in the Unity project into `../../../UIElementsSchema/`.
