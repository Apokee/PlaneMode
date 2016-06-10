# Contributing

## Copyright

All code contributions must be made under either the [MIT](LICENSE.md) license or the public domain. All non-code
contributions must be made under the [CC-BY 4.0](http://creativecommons.org/licenses/by/4.0/) license or the public
domain. No copyright assignment is necessary. If your contribution is not public domain add your name and contribution
years to the [LICENSE](LICENSE.md) file.

## Building

In order to build the code you should have Microsoft Visual Studio installed on Windows systems or Mono installed on
Linux or Mac systems.

- Copy `build.example.yml` to `build.yml` or `../PlaneMode.build.yml` relative to the project root.
  - Edit the variables `ksp_dir` and `ksp_bin` as appropriate.
- From a command line shell, execute `./build` in the project root.
  - On Windows, you *must* use PowerShell as your command line shell.
  - On Linux and Mac systems `/bin/sh` must be a POSIX-compliant shell.
- If the build was successful you should have a ZIP file in `.build/pkg/Debug`, packaged ready for distribution.
- If you would like to deploy to and execute KSP automatically after a build use `./build run`.
