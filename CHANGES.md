## v1.1.0
##### New
- Add `KSPAssembly` attribute to assembly.

##### Changes
- `PLANEMODE_USER_SETTINGS` is deprecated (although still supported), a Module Manager patch should now be used to
  modify settings.  An example patch is distributed in the `PlaneMode/Configuration` directory.
- Clarified log message that made it appeared as if code was being executed more times than it was.
- Simplified the way textures are loaded.

##### Fixes
- Fix Plane mode settings being persisted in certain situations.
- Fix the display of the log level for debug messages.

## v1.0.0
##### Fixes
- Initalize ControlMode to Rocket to avoid warning on vessel load

## v0.4.1
##### Fixes
- Kerbal Space Program v1.0 compatibility

## v0.4.0
##### New
- Added setting to disable Application Launcher (stock toolbar) button.
- Docking controls are now supported.

##### Fixes
- Interaction with trim controls should now be fixed.
- Interaction with SAS/Autopilot should now be fixed.

## v0.3.0
##### New
- Use stock Application Launcher.
- Control mode is persisted with command pods, probe cores, and docking ports. The mode used is determined by whichever
  part is selected with the *Control From Here* button.
- Control mode is automatically selected for new parts in the editor. Parts in the VAB are placed in Rocket mode and
  parts in the SPH are placed in Plane mode.

##### Changes
- Renamed from "Aeroplane Mode" to "Plane Mode".
- Settings configuration has been changed slightly and toggle and hold keys have both been defaulted to None rather
  than ScrollLock and Home.

##### Fixes
- Handle switching vessels better.
