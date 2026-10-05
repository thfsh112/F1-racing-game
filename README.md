# F1 Racing Game

## F12026 Unity Simulation

The repository now contains a Unity-oriented AAA racing simulation foundation under `Assets/F12026/`.

### Current Phase 1
- Rigidbody vehicle core
- Four WheelColliders
- Steering, throttle and brake
- 8-speed transmission
- ICE + MGU-K power model
- ERS battery SoC
- Regenerative braking
- Overdrive
- X/Z active aero
- Speed-squared aerodynamic downforce and drag
- Wheel visual synchronization
- Telemetry accessors

### Roadmap
1. Vehicle Core
2. Pacejka tires and suspension
3. Advanced aero and ground effect
4. AI racing line / overtaking / race rules
5. AAA rendering, VFX and audio
6. Full race weekend / career / multiplayer systems

## Important

The existing GitHub Pages / Three.js files are retained as the web prototype. Unity C# scripts do **not** execute inside GitHub Pages. To run the new simulation, open/import this repository in a Unity project and build the vehicle prefab from the hierarchy documented in `Assets/F12026/Scripts/README.md`.

The Unity implementation is intentionally being built as a modular simulation instead of expanding the previous browser arcade prototype.
