# F12026 Unity 6

This folder contains the Unity 6.2 racing simulation foundation.

## Unity version
- Unity 6.2
- Editor version: 6000.2.0f1

## Open the project
1. Install Unity 6.2 in Unity Hub.
2. Add the cloned/downloaded repository as a project.
3. Open the project.
4. In the Unity Editor choose **F12026 > Build Vehicle Test Scene**.
5. Open `Assets/F12026/Scenes/F12026_VehicleTest.unity`.
6. Press Play.

## Current playable systems
- Rigidbody + four WheelColliders
- Steering, throttle and brake
- 8-speed transmission
- ICE + MGU-K foundation
- ERS / battery SoC
- regenerative braking
- overdrive
- X/Z active aero
- speed-based downforce and drag
- wheel visual synchronization
- telemetry foundation

## Controls
- W / Up: throttle
- A,D / Left,Right: steering
- Space: brake
- X: overdrive
- Z: low-drag aero

The generated test scene uses placeholder geometry. It is the physics testbed for the later F1 car model, track, VFX and UI work.
