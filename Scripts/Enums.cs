using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR
{
    //Used by Hands/Grabable and Tracking Scripts
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
        PassiveJoint = 3,
    }

    public enum TwoHandedMode
    {
        SwitchHand = 0,
        Average = 1,
        //AttachHand = 2
    }

    //Used by Hand Poser
    public enum HandState
    {
        open = 0,
        grab = 1,
        pinch = 2,
        point = 3
    }

    public enum FingerTrackingMode
    {
        Kinematic = 0,
        CollisionTest = 1,
        ActiveJoint = 2
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
        //SmoothTurn = 1 //Not working
    }

    public enum Direction
    {
        North = 0,
        South = 1,
        West = 2,
        East = 3
    }

    //Used by Sockets
    public enum AttractType
    {
        AllCollisionObjects,
        Rigidbodys,
        Grabables
    }
}
