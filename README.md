# Plane Mode [![Build status][build-badge]][build]

**Plane Mode** is a Kerbal Space Program mod that allows you to easily swap control input for roll/yaw while in
flight. This is especially useful for joystick users who will typically want their joystick to control yaw for rockets
but roll for aircraft.

## Usage

To install, extract the contents of the archive to your KSP directory. This should create an `PlaneMode` directory
under the `<KSP>/GameData` directory. Plane Mode requires [Module Manager][module-manager] to be installed. Configure
your KSP control settings such that your roll/yaw controls are mapped to how you would like to use them while flying
rockets.

The control mode is stored with command pods, probe cores, and docking ports. The default control mode is determined by
whether the part was created in the VAB (Rocket) or SPH (Plane). Existing parts in flight will default to Rocket mode.
The part used is determined by which is selected by the *Control From Here* button. You can toggle the control mode of
a part by right clicking on it in the editor or in flight and press *Toggle Control Mode*. Pressing the Application
Launcher button will also toggle the control mode of the current controlling part.

## Configuration

Plane Mode can be configured by creating a user settings file. Open
`<KSP>/GameData/PlaneMode/Settings/DefaultSettings.cfg` in a text editor for details and instructions.

## Acknowledgements

This is a continuation of the [Aeroplane Mode](http://forum.kerbalspaceprogram.com/threads/90034) mod created by
Phillip "Belisarius" Reiss.

[build]: https://ci.appveyor.com/project/Apokee/planemode
[build-badge]: https://ci.appveyor.com/api/projects/status/nlnofph4shq6t7ic/branch/develop
[module-manager]: http://forum.kerbalspaceprogram.com/threads/55219
