# Plane Mode [![Build status][build-badge]][build]

**Plane Mode** is a Kerbal Space Program mod that allows you to easily swap control input for roll/yaw while in
flight. This is especially useful for joystick users who will typically want their joystick to control yaw for rockets
but roll for aircraft.

## Installation
### CKAN
Plane Mode's CKAN identifier is `PlaneMode`. It may be installed from the command line with:

```
> ckan install PlaneMode
```

It can also be installed from the GUI.

### Manual
1. Download the distribution package from [GitHub][github-releases].
2. Extract the contents of the archive to your KSP directory. This should create an `PlaneMode` directory under
the `<KSP>/GameData` directory.
3. Follow the installation instructions for all dependencies.

#### Dependencies
- [Module Manager][module-manager]

## Usage

Configure your KSP control settings such that your roll/yaw controls are mapped to how you would like to use them
while flying rockets.

The control mode is stored with command pods, probe cores, and docking ports. The default control mode is determined by
whether the part was created in the VAB (Rocket) or SPH (Plane). Existing parts in flight will default to Rocket mode.
The part used is determined by which is selected by the *Control From Here* button. You can toggle the control mode of
a part by right clicking on it in the editor or in flight and press *Toggle Control Mode*. Pressing the Application
Launcher button will also toggle the control mode of the current controlling part. Actions are also available for use
in action groups to toggle the control mode or switch to a specific mode.

## Configuration

Plane Mode can be configured by creating Module Manager patches against the default settings stored in
`<KSP>/GameData/PlaneMode/Configuration/PlaneMode.cfg`. How to use Module Manager is outside the scope of this README,
please see the Module Manager documentation for more information.

## Acknowledgements

This is a continuation of the [Aeroplane Mode](http://forum.kerbalspaceprogram.com/threads/90034) mod created by
Phillip "Belisarius" Reiss.

[build]: https://ci.appveyor.com/project/Apokee/planemode/branch/develop
[build-badge]: https://ci.appveyor.com/api/projects/status/nlnofph4shq6t7ic/branch/develop?svg=true
[github-releases]: https://github.com/Apokee/PlaneMode/releases
[module-manager]: http://forum.kerbalspaceprogram.com/threads/55219
