## v1.4.4-alpha

## v1.4.3
##### Changed
- Compatability with KSP 1.2.

## v1.4.2
##### Fixed
- Fixed using AppLauncher button to change control mode.

## v1.4.1
##### Changed
- Use `UI_Cycle` attribute for more stock-like context menu item.

##### Fixed
- Fixed some annoying NullReferenceExceptions.

## v1.4.0
##### Changed
- Context menu items have been consolidated into a single item that shows current control mode and allows you to toggle
  betwen them.

## v1.3.1
##### Changed
- Compatability with KSP 1.1

## v1.3.0
##### Added
- Add actions to all applicables parts to change control modes, making them available to action groups. Available actions are:
  - `Control Mode: Toggle`
  - `Control Mode: Rocket`
  - `Control Mode: Plane`

##### Fixed
- Fix example configuration patch to show how to actually change toggle keys.

## v1.2.0
##### Added
- Add `defaultControlMode`, `defaultVabControlMode`, `defaultSphControlMode` configuration settings to control default
  control mode used for various situations.

## v1.1.0
##### Added
- Add `KSPAssembly` attribute to assembly.

##### Changed
- `PLANEMODE_USER_SETTINGS` is deprecated (although still supported), a Module Manager patch should now be used to
  modify settings.  An example patch is distributed in the `PlaneMode/Configuration` directory.
- Clarified log message that made it appeared as if code was being executed more times than it was.
- Simplified the way textures are loaded.

##### Fixed
- Fix Plane mode settings being persisted in certain situations.
- Fix the display of the log level for debug messages.

## v1.0.0
##### Fixed
- Initalize ControlMode to Rocket to avoid warning on vessel load

## v0.4.1
##### Fixed
- Kerbal Space Program v1.0 compatibility

## v0.4.0
##### Added
- Added setting to disable Application Launcher (stock toolbar) button.
- Docking controls are now supported.

##### Fixed
- Interaction with trim controls should now be fixed.
- Interaction with SAS/Autopilot should now be fixed.

## v0.3.0
##### Added
- Use stock Application Launcher.
- Control mode is persisted with command pods, probe cores, and docking ports. The mode used is determined by whichever
  part is selected with the *Control From Here* button.
- Control mode is automatically selected for new parts in the editor. Parts in the VAB are placed in Rocket mode and
  parts in the SPH are placed in Plane mode.

##### Changed
- Renamed from "Aeroplane Mode" to "Plane Mode".
- Settings configuration has been changed slightly and toggle and hold keys have both been defaulted to None rather
  than ScrollLock and Home.

##### Fixed
- Handle switching vessels better.
