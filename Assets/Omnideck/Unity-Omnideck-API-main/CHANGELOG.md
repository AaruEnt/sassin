# Changelog

All notable changes will be documented in this file.

## [2.0.3-alpha] - 2023-10-13

- Adjusting samples to be compatible with the Unity OpenXR package and Unity XR Interaction Toolkit interaction system.
- Added an OmnideckContinousMoveProvider class, which extends the ContinousMoveProviderBase class. Overrides the ReadInput() method to pipe the movement vector coming from the OmnitrackInterface (from Omnitrack) into the XR Interaction toolkit interaction system.
- Requires XR Interaction Toolkit v2.5.2.
- Tested with Unity 2022.3.4 and 2022.3.8.
- Tested with Omnitrack v2.6.6.

## [2.0.2-alpha] - 2023-07-08

- Changed OnApplicationQuit() to OnDestroy(). Fixes bug where network connection was not closed due to working with Prefabs and the Editor. 

## [2.0.1-alpha] - 2023-07-06

- Created Samples~ fixing problem with having Prefabs in immutable Runtime-folder.

## [2.0.0-alpha] - 2023-07-06

- Added package
