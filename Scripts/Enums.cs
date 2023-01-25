using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    //Used by Hands/Grabbable and Tracking Scripts
    public enum Hand
    {
        Left = 0,
        Right = 1
    }

    public enum TrackingMode
    {
        Kinematic = 0,
        Velocity = 1,
        ActiveJoint = 2,
        Force = 4,
        FixedJoint = 5,
        PDForce = 6
    }

    public enum TwoHandedModes
    {
        SwitchHand = 0,
        Average = 1,
        JointRotation = 2
    }

    public enum GrabbableType
    {
        Interactables,
        Props
    }

    //Used by Hand Poser
    public enum HandState
    {
        open = 0,
        grip = 1,
        trigger = 2,
        point = 3
    }

    public enum FingerTrackingMode
    {
        Kinematic = 0,
        CollisionTest = 1,
        Joint = 2
    }

    //Used by Movement
    public enum MovementDirection
    {
        HeadOriented = 0,
        HandOriented = 1,
    }

    public enum TurnMode
    {
        SnapTurn = 0,
        SmoothTurn = 1
    }

    public enum Direction
    {
        North = 0,
        South = 1,
        West = 2,
        East = 3
    }

    public enum PlayerState
    {
        Crouching,
        Standing,
        Jumping,
        Falling
    }

    //Used by Sockets
    public enum AttractType
    {
        Rigidbodys,
        Grabbables
    }

    //Rig
    public enum RigType
    {
        Mock,
        XR
    }

    //Stabbing
    public enum Axis
    {
        X,
        Y,
        Z
    }
}
