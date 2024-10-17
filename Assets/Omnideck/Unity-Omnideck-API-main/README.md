# Omnideck API
This is the package for the Unity Omnitrack API by Omnifinity. It relies on the Unity XR Interaction Toolkit and the OpenXR implementation in Unity.

## Tested with
Tested with:

2024-07-04:
- Unity 2022.3.35f1
-   OpenXR Plugin 1.11.0
-   XR Interaction Toolkit 2.5.4
- SteamVR 2.6.2
- Omnitrack 2.6.9

Previous tests:
- Unity 2022.3.8f1

## Requirements
- Unity XR Interaction Toolkit and the included Starter Assets sample

## Installation
1) Either

Use the Unity Package Manager and add this github repo using the code URL.
(https://github.com/Omnifinity/Unity-Omnideck-API.git)

2) Or instead

    Download the code as a ZIP-file and unarchive it to your computer. Use the Unity Package Manager and use the "Add package from disk" and select the package.json file. 

3) Omnideck Sample Scene

    Import one of the Omnideck sample scenes via the Package Manager. E.g. import our "Omnideck XRI toolkit" sample by navigating to the "Omnideck" package, and further on to the "Samples" section. Press "Import". You should now see a sample scene appearing inside the "Project > Assets > Samples > Omnideck folder".

4) Missing prefabs from the "XR Interaction Toolkit"

    If you try to open the sample scene you will find that there are a lot of missing Prefabs in the Hierarchy listing. We have not yet imported the required  "Starter Assets" in the "XR Interaction Toolkit" from Unity.

5) Import the "Starter Assets" from the "XR Interaction Toolkit"

    Navigate to the <i>Window > Package Manager</i>. Search for the <i>XR Interaction Toolkit</i> package, navigate to the "Samples" tab and import the "Starter Assets" sample. The warnings should now disappear. If you had installed the "Starter Assets" sample before you imported our package the warnings would never have appeared.

6) Fix any warnings and add the XR Plugin-in management for the HTC Vive Controller profile

   Navigate to the <i>Project Settings > XR Plug-in Managment</i>. Enable the *OpenXR* tickbox and fix any issues that Unity recommends. This could take a while if scripts need recompilation so be patient.

   If you are using a HTC Vive Controller navigate to the <i>Project Settings > XR Plug-in Management > OpenXR</i> and add the <i>HTC Vive Controller Profile</i> in the <i>Interaction Profiles</i> section.


## Samples
The package contains three samples on how to integrate the Omnideck API with your VR application.
- Example 1: Integration with the Unity XR Interaction toolkit interaction system (v2.5.2+) by adding a OmnideckContinousMoveProvider that extends the ContinousMoveProviderBase from XRI. Remember to import the XRI Starter Asset samples. Uses a Unity Character Controller that can collide with the surrounding objects. Supports hand controllers and basic interaction.
- Example 2: Integration using a Unity Character Controller that can collide with the surrounding objects. It has an XR Rig attached to it.
- Example 3: Integration using a Vector3 script, without any object collision. It has an XR Rig attached to it.


## Sample 1 - Omnideck integration with the Unity XR Interaction Toolkit

This sample combines the XRI toolkit with our API so that youcan use both controllers and the Omnideck at the same time.  
   
In the Hierarcy, the first game object related to the Omnideck is the "Omnideck reference" Game Object. Read the "integrated instructions" on the contained game objects.

**World Forward Direction**

The *Rotation* part of the *Transform* of this Game Object will define the forward vector of the users' VR game object/camera when walking on the Omnideck.
The angle in the *Y*-component of the *Rotation* should match with the Y-component of the <i>XR Interaction Setup > XR Origin (XR Rig)</i>.
In this example the "Forward" direction (blue axis) of the XR Rig is in the negative global Z-axis (as defined by the Unity sample that our sample is based on).
If you rotate the XR Rig forward direction you must also rotate the World Forward Direction game object accordingly.   
    

**Locomotion system**
   
In the Hierarchy, navigate to and expand the <i>XR Interaction Setup > XR Origin > Locomotion system </i>.

The "Locomotion system" (which is provided as a base by Unity themselves) contains various ways that the VR user can navigate the very simple scene.

You can enable / disable the various movement schemes by activating / deactivating the Game Object in the Inspector.
If you do not disable them, in the default setup in this sample you can see that you are able to use the VR hand controller to *Turn*, *Teleport* and *Climb* while you are able to walk around on the Omnideck (using the OmnideckMove implementation).


**OmnideckMove Game Object - Omnideck Continous Move Provider script**
    
This Game Object contains the actual code for moving the "XR Origin (XR Rig)" around in the world. 
This Game Object and the OmnideckContinousMoveProvider.cs script builds upon the move provider script (ContinousMoveProviderBase.cs) and the locomotion system (LocomotionSystem.cs) provided by Unity in the XR Interaction toolkit. Ensure that you have enabled the *Enable Strage* option, otherwise you will not be able to move around in all 360 degrees. 
    

## Sample 1 and Sample 2

These samples are very basic and show you the most basic way of implementing the Omnideck API without the XRI toolkit.


More details will follow...




## Changelog
Please see https://github.com/Omnifinity/Unity-Omnideck-API/blob/main/CHANGELOG.md
