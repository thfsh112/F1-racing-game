# F1 2026 Vehicle Physics

This folder contains the next physics layer:

- F12026CarController
- F12026Tire
- F12026Suspension
- F12026VerticalLoad

Tyres:
- longitudinal slip ratio
- slip angle
- combined-slip saturation
- load-sensitive grip
- rolling resistance
- wheel rotational inertia

Suspension:
- custom raycast contact
- spring force
- bump/rebound damping
- bump stops
- suspension travel
- contact normal

Vertical load:
- static weight distribution
- longitudinal load transfer
- lateral load transfer
- aerodynamic load distribution
- per-corner normal-load telemetry

The controller exposes 2026-style active aero X/Z states.

This is an original game-physics approximation, not an FIA homologation simulator.
The FIA latest published 2026 Section C technical regulations are Issue 20, published 2026-10-01. Exact game setup values remain configurable.

Unity setup:
1. Create a car Rigidbody.
2. Attach F12026CarController.
3. Create four child suspension anchors: FL, FR, RL, RR.
4. Attach F12026Suspension and F12026Tire to each corner.
5. Assign all eight components in F12026CarController.
6. Put the track collider on the suspension trackMask layer.

The default 768 kg value is a game baseline inspired by the 2026 minimum-mass framework, not a claim that every race setup weighs exactly 768 kg.

The scripts target Unity 6 and use Rigidbody.linearVelocity.
