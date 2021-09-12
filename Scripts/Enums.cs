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
        Joint = 2
    }

    public enum GrabMode
    {
        Kinematic = 0,
        Velocity = 1,
        Joint = 2
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
}
