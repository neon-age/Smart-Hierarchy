## [0.5.5] - Unreleased
- Compact Settings / Filtering Menu
- Hiding Scene Visibility columns

## [0.5.0] - Unreleased
Branch: #view-items-refactoring
### What to expect?
## Major Changes
- Initial Public Customization API
- Switch to Harmony for IMGUI patching
- Complete Items Refactoring, GUI stacking / composition support
#### Minor
- Headers / separators
- Middle-click expanding

## [0.3.1] - 2021-04-10
### Added
-  Activation Toggle [Swiping](https://github.com/neon-age/Smart-Hierarchy/wiki/Activation-Toggle).

    ![Activation Swiping](https://i.imgur.com/wzSsNOb.gif)
### Changed
- Collection component has moved into separate Runtime assembly.
- Main assembly is now Editor-only. (it was unchecked for Stadia as a temp fix for "Can't add editor script")

## [0.3.0] - 2021-03-09
### Added
- Collection "Keep Transform Hierarchy" option. 
  This will make collection act as a regular game-object, allowing you to add components on it.
- Customizable Paste behaviour:
    - Place new game objects next to selection instead of the end of hierarchy.
    - Auto Paste as Child when selection is expanded.
- Gizmo icon support.
- Types Priority "Ignore" toggle.
- Null components are now displayed with "Default Asset" icon.

### Fixed
- Fixed constant repainting that caused high CPU usage in background.
- Types Priority not being initialized properly.
- Sub-Scene icon being hidden by other components.

## [0.2.9] - 2020-12-15
### Added
- [Types Priority](https://github.com/neon-age/Smart-Hierarchy/wiki/Icons) feature, you can now define which types are preferred to be displayed.

    ![Types Priority](https://i.imgur.com/RDGNhGH.png)


### Changed
- Icons are now rendered separately with custom shader for selected items. Default hierarchy icons are hidden.
- Rename "Folders" to "Collections".