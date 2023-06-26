# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/), 
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

[Changelog also available online](https://www.occasoftware.com/changelogs/buto)

## [5.0.0] - 2023-06-05
### Changed
- Migrated to UPM format
- Improved performance with custom shadow sampling

### Added
- Added more quality setting options to provide better guidance on recommended settings


## [4.3.0] - 2023-05-23
### Added
- Added an option to prefer nearby samples. This improves the quality of nearby fog at the cost of more distant fog. This is the new Ray Length Mode option. The default setting is "Constant", but "Prefer Nearby" can give better results in some cases.
- Added an option to set the start distance for Distant Fog.

### Changed
- Buto now upsamples the buto render before temporal anti-aliasing.


## [4.2.0] - 2023-05-11
### Added:
- Added option to control Distant Fog Base Height
- Added option to control Distant Fog Attenuation Size

### Changed:
- Changed various tooltips for brevity
- OccaSoftware.Buto renamed to OccaSoftware.Buto.Runtime
- OccaSoftware.Buto.Demo now has an associated asmdef.
- Updated basic demo scene to showcase color overrides and directional lighting.
- The Profiles folder has been moved from AssetResources/~ to DemoResources/~ to propertly indicate that it is not required for the asset to function.
- Buto Fog Density mask context item moved from GameObject/Effects panel to GameObject/Rendering panel.
- Grouped all Buto-related scripts to the Buto/~ folder in the Add Component menu.

### Fixed:
- Fixed a source of 40B GC Alloc per-frame.

### Removed:
- Buto Fog Volume context menu item has been removed, as it presents a potentially confusing choice between Buto, Global, Box, and Sphere volumes where no choice needs to be made. Buto can simply be added to any type of volume using the Add Override menu.
- Prefab items have been removed, as they present a potentially confusing alternative to the context menu approach for adding Buto Lights and Fog Density Masks. If you get a prefab missing warning, simply unpack the prefab from source and create a new base prefab and variants.


## [4.1.0] - 2023-05-09

### Added:
- Added Settings for the renderer feature. You can now configure the target RenderPassEvent directly from the Pipeline Asset inspector.
- Added a Depth Interaction Mode option. Early Exit mode clips rays early when there is nearby geometry. Maximize Samples mode shortens each step so that every ray takes the same number of samples.
- Added a Depth Softening Distance option. This option enables you to soften the fog when it is close to scene geometry, which gives a more natural and realistic look.
- Added more demo scenes.

### Changed:
- Changed the array declarations in the renderer feature so that they are initialized only once and the same arrays are populated each frame.
- The Fog Density Mask component now conditionally compiles editor-related code using the #if UNITY_EDITOR directive.
- The Buto Light component now conditionally compiles editor-related code using the #if UNITY_EDITOR directive.



## [4.0.0] - 2023-05-05

### Changed:
- Changed all Shader Graphs to Custom Shaders.
- Changed Distant Fog to start immediately from the Ray Origin. This gives more visually coherent results.
- Changed various functions to improve performance.

### Added:
- Added a Distant Fog Density property.
- Added support for Light Cookies for Directional Light.
- Added more demo scenes.

### Fixed:
- Fixed shadows when in Orthographic projection.
- Fixed an issue causing Distant Fog to throw an error.

## [...]

- Earlier changelog notes can be found online on the [Buto Asset Page](https://www.occasoftware.com/assets/buto#changelog).