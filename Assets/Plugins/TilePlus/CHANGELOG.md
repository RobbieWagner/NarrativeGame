# Changelog

## [NOTES]
 - Please note that documentation can be found in the TilePlusExtras folder.
 - The TpLib/TpLibEditor version numbers are printed as a console message after a script (re)load if the log configuration setting "Informational" is Set.
   - This setting is off by default but can be activated from the Tile+Painter configuration panel OR the Configuration window accessed from the Tools/TilePlus menu.
 - If you want to use DOTween please read the user/programmer's guide for more information.
 - Version 2.0 is internally very different than 1.X. It is advised to use a copy of your existing project when upgrading.

### Important note: requires Unity 2022.3 LTS or newer. Tested with 2023.X

### [2.0.1] 2023-09-01

### Known issues
- in Play mode, opening Tile+Painter or toggling Palettes ON in the Paint mode options panel in the center column when it had been off prior to entering Play mode may show this error: "The referenced script (Unknown) on this Behaviour is missing"
  - This isn't a Tile+Painter error, rather, one of the palettes has a missing asset reference; in other words, the original tile asset used in a Palette was deleted and when scanned to see how many tiles exist in the Palette, this 'deep within Unity' error occurs.
  - The fix is to examine all palettes by opening each Palette prefab so that you can see the embedded Tilemap, then open the info folder in the Tilemap component, and see if any of the tiles appear as 'TileBase'. 
  - If you find one, activate "Allow Prefab Editing" in the Configuration editor. Then select the Tilemap with your mouse and use Tools/TilePlus/Utilities/Delete Null Tiles. Then uncheck 'Allow Prefab Editing'.
- Tile+Painter won't show tiles in an opened Palette prefab's Tilemap. If you use the Tile+Brush to inspect a tile in an opened Palette prefab the TilePlusBase section of the Inspector will display "Could not Format: Tilemap was null".
  - This is because the Tiles in the Palette prefab aren't initialized properly, they don't seem to get the StartUp invocation.
  - It's not an error, it's a side effect (but not a feature).

### Added
- Documentation has a new file: Articles.pdf. This contains design and historical information about this project.

### Fixed

- TilePlusPainter.TpPainterContentPanel: fixed edge case where changing global mode to Paint mode didn't clear the PainterWindow.TileTarget.
  - Showed incorrect info in the brush inspector pane.
- TpPainterShortcuts: Copy to history would nullref if Painter window wasn't ever opened in a session. Fix = automatically open the window.
- TpSysInfo: added conditional compilation to avoid warning in 2023.x on change in content.verticalScrollerVisibility property.
- TpPainterSceneView: 
  - fixed tests for paintmask to only run in Paint mode when Painting tool is active. Minor: just removes spurious console messages.
  - changed the warnings in TpPainterSceneView.AnyRestrictionsForThisTile to only print to the console when the 'Informational' messages setting is active.
  - changed vertical offsets for some SceneView messages in TpPainterSceneView.HandleRepaint so that "Can't paint here" and "Tile sprite is hidden" appear correctly when multiple text messages are presented in the SceneView.

### Changed
- TpEditorUtilities in DeleteNulls, added check for 'Allow Prefab Editing' so that nulls can be removed from Palettes; see the Known Issues section.


### [2.0.0] 2023-5-08

### Added
- Tile+Painter: A UIElements-based tile/GameObject painter and tilemap viewer; replacing TilePlus Utility. See the pdf docs for more info.
  - Shortcut key is ALT+1. 
  - Uses the palettes you create with the Unity painter as well as TileFabs and Bundles.
  - Support is ONLY for single-tile-at-a-time painting/erasing/moving etc.
    - Exception: painting TileFabs and Bundles will of course paint numerous tiles at once.
  - Supports creating and saving GridSelections for better workflow.
  - Support for virtual grid as a multiple of Tilemap's Grid: useful to paint chunks of tiles from TileFabs.
- SystemInfo: The menu or ALT+2 displays a window with TPT statistics and useful information.
- Transform Editor: The menu or ALT+3 displays an editor window where you can create custom tile-sprite transforms when using Tile+Painter.
- Template Tool: The menu or ALT+4 displays an editor window that's used to create templates when using ZoneManagers.
- TpLib: add support for delayed callbacks using Async delays for those > 10 msec.
- TileUtil:  added tile transform convenience methods GetRotation, GetPosition, GetScale. Handy for Dotween 'getters'
- TileFabLib support for loading chunks of tiles at Runtime including adding/removing chunks around the camera as it moves.
  - Demo projects added to illustrate how to use this feature.
  - JSON archiving of active chunks for game saves.
- New Demos
 
### Changed
- Required Unity version changed to 2022.3 LTS or newer
- TilePlusBase:
  - Performance improvements in StartUp. Don't register tile if it already is registered (usually that's done in TpLib anyway but has a lot of extra checks that can be avoided if done in TilePlusBase)
  - new class for return from methods tagged with TptShowCustomGUI attribute.
  - NoActionRequiredCustomGuiReturn property can be used as a return value from methods tagged with TptShowCustomGui.
- TpLib:
  - new simplified forms of TpLog, TpLogWarning and TpLogError provided for convenience. Note: DO NOT USE within TilePlusBase or subclasses
    - Please note that this is for future compatibility.
  - support for delayed callbacks.
- Prefabs: Tilemap prefabs are simpler: only GameObjects are archived and the Tilemaps within the Prefab are loaded from TileFabs. 
  - Greatly increases efficiency especially if loaded more than once due to Bundle caching (see below).
- Animated tiles upgraded to use new Tile animation features and tile flags for animation.
- TileFabLib:
  - added capability to use Tilemap.LoadTiles with a TileChangeData array.
  - non-TilePlus tiles are cached when a Bundle is loaded for the first time: subsequent loads use the cache.
  - added filtering for tiles when a Bundle is loaded.

### Deprecated


### Removed
- Plugin system removed. TpTiming replaced with TpLib DelayedCall. TpDOTween replaced with DOTweenAdapter.
- TilePlus Utility window removed. Replaced with Tile+Painter.
- TpLog(LogType,string) deleted. Use instead TpLog, TpLogWarning, TpLogError.

### Fixed
- Initial release of 2.0 fixed innumerable small issues.

### Open Issues
- Rarely, when starting Unity with an existing scene that contains TilePlus tiles, these tiles may not be initialized properly. This is difficult to observe since it doesn't seem to be repeatable but it appears as if the Tilemap does not properly call StartUp in these cases.
  - This is an editor-only situation and if it occurs just use the Tools/Refresh TpLib menu item.  
- The History Stack in Painter has an inconsistent behaviour:
  - If you open and close the Painter window during a Unity session the stack will be cleared.
  - If you have the Painter window open during a Unity session and end the session with the window open then when you restart Unity the stack will be unchanged.
    - Since the stack can have clone tiles, in this situation the clone tile refs would be invalid (they are scene objects) so those are removed.
  - An obvious fix would be to clear the History stack when the Painter window is opened or Enabled but that would clear the stack on every scripting reload.
  - It's sort of a conundrum.
