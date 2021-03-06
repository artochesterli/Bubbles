﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleMotionData : MonoBehaviour
{
    public float MotionTime;
    public float RecoverTime;
    public float TeleportTime;
    public float TeleportWaitTime;
    public float BlinkTime;

    public float MoveDis;

    public float NormalScale;
    public float ExhaustScale;

    public float BlockedDis;
    public float ConflictBlockedDis;

    public float TeleportSlotBlockedRotationAngle;
    public float TeleportSlotBlockedRotationTime;
    public int TeleportSlotShakeCycle;

    public Color DefaultEnergyColor;
    public Color AffectedEnergyColor;
    
}
