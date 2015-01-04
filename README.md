# Plane Mode [![Build status][build-badge]][build]

**Plane Mode** is a Kerbal Space Program mod that allows you to easily swap control input for roll/yaw while in
flight. This is especially useful for joystick users who will typically want their joystick to control yaw for rockets
but roll for aircraft.

## Usage

To install, extract the contents of the archive to your KSP directory. This should create an `PlaneMode` directory
under the `<KSP>/GameData` directory. Configure your KSP control settings such that your roll/yaw controls are mapped
to how you would like to use them while flying rockets.

Rocket mode is the default mode. You can engage plane mode by pressing the Plane Mode button in the application
launcher. The button's icon will chage from a rocket to an aircraft to indicate the current mode.

You can also press the `Scroll Lock` key to toggle modes or press and hold the `Home` key to change
modes temporarily. These keys can be reassigned by editing the settings configuration file.

## Configuration

If you wish to change the key bindings as specified above, open the `<KSP>/GameData/PlaneMode/settings.cfg` file in a
text editor and change the `primary` entry for each command to the
[key](http://docs.unity3d.com/ScriptReference/KeyCode.html) you wish to use.

You can also inverse pitch control when switching modes by setting `pitch_invert` to `true`.

## Acknowledgements

This is a continuation of the [Aeroplane Mode](http://forum.kerbalspaceprogram.com/threads/90034) mod created by
Phillip "Belisarius" Reiss.

[build]: https://ci.appveyor.com/project/Apokee/planemode
[build-badge]: https://ci.appveyor.com/api/projects/status/nlnofph4shq6t7ic/branch/develop
