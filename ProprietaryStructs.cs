using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
namespace AR_Fukuoka
{
    public struct Coordinates
    {
        public double latitude; 
        public double longitude; 
        public Coordinates(double latitude, double longitude) 
        { 
            this.latitude = latitude; 
            this.longitude = longitude; 
        } 
    }

    public struct NavSteps
    {
        public double latitude;
        public double longitude;
        public String htmlStep;
        public String polyLine;
        public NavSteps(double latitude, double longitude, String htmlStep, String polyLine)
        {
            this.latitude = latitude; 
            this.longitude = longitude; 
            this.htmlStep = htmlStep;
            this.polyLine = polyLine;
        }
    }

    public struct IntermediaryPoint
    {
        public GameObject inter;
        public double latitude;
        public double longitude;
        public IntermediaryPoint(GameObject inter, double latitude, double longitude)
        {
            this.inter = inter;
            this.latitude = latitude; 
            this.longitude = longitude; 
        }
    }
}
