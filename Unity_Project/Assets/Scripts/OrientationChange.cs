using UnityEngine;

using System;

using UnitySlippyMap.Map;
using UnitySlippyMap.Markers;
using UnitySlippyMap.Layers;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.Converters.WellKnownText;
using System.IO;
using System.Collections;
using System.Collections.Generic;


public class OrientationChange : MonoBehaviour
{
    private bool isPerspectiveView = false;
    private float perspectiveAngle = 30.0f;
    private float destinationAngle = 0.0f;
    private float currentAngle = 0.0f;
    private float animationDuration = 0.5f;
    private float animationStartTime = 0.0f;


    public void ChangePerspective()
    {
        if (isPerspectiveView)
        {
            destinationAngle = -perspectiveAngle;
        }
        else
        {
            destinationAngle = perspectiveAngle;
        }

        animationStartTime = Time.time;

        isPerspectiveView = !isPerspectiveView;
    }
}
