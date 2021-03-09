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
- Types Priority feature, you can now define which types are preferred to be displayed.

### Changed
- Icons are now rendered separately with custom shader for selected items. Default hierarchy icons are hidden.
- Rename "Folders" to "Collections".