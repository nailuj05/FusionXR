# FusionXR
A all in one VR Toolkit.

This has the goal of setting a open standard for VR. Many Problems already have solutions, so why figure them out yourself?
My goal is it to compile a large set of tools so developers can focus on making games instead of worrying about interactions, systems, etc.
I hope to find many contributers to this project and anyone that has some code from a old project, ideas for improvement or even brand new additions to this set is invited to share them with me. I will try to integrate everything I can get, so even if you only have some old code that might help, just throw it my way and I will try the best I can do to make it availabe. :)

---

## Setup
The project is currently in *full development*, if you are reading this you are invited as a alpha tester. Currently I am not testing on other versions of Unity or the XR Plugins. 
For testing I'd recommend using the same setup as I do:
- Unity Version: **2021.1.16f1** (newer 2021 versions should work though)
- URP Render Pipeline: **11.0.0**

### Plugins:
- Open XR: **1.3.1**
- XR Interaction Toolkit: **1.0.0-pre.8**
- Animation Rigging: **1.1.1**
(- Text Mesh Pro: 3.0.6)

### Layers and Tags:
| Layer   | Layer Name   |
|---------|--------------|
| Layer 3 | Player       |
| Layer 6 | Enemy        |
| Layer 7 | Hands        |
| Layer 8 | Interactables|
| Layer 9 | Props        |
| Layer 10| Stabable     |

Tag X: **Attached**

### Physics Settings:
- Default Solver Iterations: **10**
- Default Solver Velocity Iterations: **10**
- Default Max Angular Speed: **100**

### Layermask:
Your Layermask should look like this:

![image](https://user-images.githubusercontent.com/57530068/155902620-acc5be96-c0e0-4410-b7c3-d11308c71597.png)

---

## The Hybrid System
Most of the XR Rigs in the Package are setup to be hybrid, that means they can be tested using Mouse and Keyboard aswell as in VR. On each Hybrid Rig (On the Physics Rig aswell) there should be a HybridRig component, on there you can switch between *Mock* and *XR*.
If there is something that is not working correctly try updating the Rig manually using the Button after switching.
In the *Mock* Mode you can:
- Walk with `W` `A` `S` `D`
- Look around with `Mouse`
- Pinch with `Mouse 1`
- Grab with `Mouse 2`
- Stretch the Arm with `Scrollwheel`
- Move the Arm with `LeftAlt`
- Switch Hands with `LeftCtrl`
- *Jump/Crouch with `Space` (only with the PhysicsRigV2)*

## Known Issues
- VR Controllers don't always align with the Hands (tested on Index)
