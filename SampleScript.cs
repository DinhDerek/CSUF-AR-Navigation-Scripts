using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
namespace AR_Fukuoka
{
    public class SampleScript : MonoBehaviour
    {
        // Tracking information using GeospatialAPI
        public AREarthManager EarthManager;
        //GeospatialAPI and ARCore initialization and results
        public VpsInitializer Initializer;
        // Allowable accuracy of azimuth
        public double HeadingThreshold = 25;
        // Allowable accuracy of horizontal position
        public double HorizontalThreshold = 20;
        public String Destination; //String input value of the destination to navigate to
        double Altitude; // Height to place the object
        public double Heading; // Object orientation (North = 0 degrees)
        public GameObject IntermediaryPrefab; // Data of intermediary display object
        public GameObject MarkerPrefab; // Data of marker display object
        GameObject displayCurrentStep; // Actual marker object to be displayed
        public ARAnchorManager AnchorManager; // Used to create anchors
        Queue<NavSteps> Steps; // The list of steps of the navigation
        Queue<IntermediaryPoint> Intermediaries; // The list of intermediary arrows between points
        NavSteps CurrentStep; // The current step in the navigation
        IntermediaryPoint CurrentIntermediary; // The closest arrow to the user's position
        bool Initialized; // Whether or not the navigation has begun

        void Start()
        {
            Steps = new Queue<NavSteps>();
            Intermediaries = new Queue<IntermediaryPoint>();
            Initialized = false;
        }
        // Update is called once per frame
        void Update()
        {
            string status = "";
            // If initialization fails or you do not want to track, do nothing and return
            if (!Initializer.IsReady || EarthManager.EarthTrackingState != TrackingState.Tracking)
            {
                return;
            }
            // Get tracking results
            GeospatialPose pose = EarthManager.CameraGeospatialPose;

            // Tracking accuracy is worse than the threshold (larger value)
            if (pose.HeadingAccuracy > HeadingThreshold ||
                 pose.HorizontalAccuracy > HorizontalThreshold)
            {
                status = "Low Tracking accuracy";
            }
            else 
            {
                status = "High Tracking Accuracy";
                if (!Initialized) // If the directions have not been initialized, initialize
                {
                    initializeRouting(pose);
                }

                if (Steps.Count > 0) // If there are more steps in the navigation, continue to update
                {
                    updateCurrentStep(pose);
                }
                else // Else, the navigation is complete
                {
                    status = "Destination reached!";
                }
            }
            // Display tracking information regardless of navigation progress
            string computedDistance = NavigationCalculator.getDistance(pose.Latitude, pose.Longitude, CurrentStep.latitude, CurrentStep.longitude).ToString("F1");
            UIController.ShowTrackingInfo(Initialized, status, Destination, CurrentStep.htmlStep, computedDistance);
        }

        void getIntermediaries(GeospatialPose pose)
        {
            Queue<Coordinates> interCoords = NavigationManager.DecodePolyline(CurrentStep.polyLine);

            // Loop through the queue of intermediary points and generate GameObjects from them
            while (interCoords.Count > 0)
            {
                Coordinates intCoord = interCoords.Dequeue();

                double heading = 0.0;

                if (interCoords.Count > 1)
                {
                    heading = NavigationCalculator.getHeading(intCoord.latitude, intCoord.longitude, interCoords.Peek().latitude, interCoords.Peek().longitude);
                }
                else 
                {
                    heading = NavigationCalculator.getHeading(intCoord.latitude, intCoord.longitude, CurrentStep.latitude, CurrentStep.longitude);
                }
                Quaternion quaternion = Quaternion.AngleAxis(180f - (float)heading, Vector3.up);
                ARGeospatialAnchor currentIntermediary = AnchorManager.AddAnchor(intCoord.latitude, intCoord.longitude, Altitude, quaternion);
                if (currentIntermediary != null)
                {
                    GameObject displayIntermediary = Instantiate(IntermediaryPrefab, currentIntermediary.transform);
                    Intermediaries.Enqueue(new IntermediaryPoint(displayIntermediary, intCoord.latitude, intCoord.longitude));
                }
            }
        }

        void initializeRouting(GeospatialPose pose)
        {
            Steps = NavigationManager.getDirections(pose, Destination); // Use the navigation manager to make an API call to retrieve the navigation steps
            CurrentStep = Steps.Dequeue(); // Pop the current navigation step from the queue
            
            // The quaternion determines where the marker should be pointed.  If there are many steps, point the marker to the next marker.
            Quaternion quaternion;
            if (Steps.Count > 0)
            {
                double heading = NavigationCalculator.getHeading(CurrentStep.latitude, CurrentStep.longitude, Steps.Peek().latitude, Steps.Peek().longitude);
                quaternion = Quaternion.AngleAxis(180f - (float)heading, Vector3.up);
            }
            else
            {
                quaternion = Quaternion.AngleAxis(180f - (float)Heading, Vector3.up);
            }

            // Once the correct information is gathered, the anchor can then be instantiated
            ARGeospatialAnchor currentAnchor = AnchorManager.AddAnchor(CurrentStep.latitude, CurrentStep.longitude, Altitude, quaternion);
            // Using the anchor information, the GameObject can now be placed into the space
            if (currentAnchor != null)
            {
                displayCurrentStep = Instantiate(MarkerPrefab, currentAnchor.transform);
            }

            // After the marker is placed, make an API call to generate the points along the current step and instantiate them as well
            getIntermediaries(pose);
            CurrentIntermediary = Intermediaries.Dequeue();

            Initialized = true;
        }

        void updateCurrentStep(GeospatialPose pose) 
        {
            // This destroys intermediary arrows as the user walks through them so the space isn't cluttered with unnecessary arrows
            double distanceToIntermediary = NavigationCalculator.getDistance(pose.Latitude, pose.Longitude, CurrentIntermediary.latitude, CurrentIntermediary.longitude);
            if (Intermediaries.Count > 0) {
                if (distanceToIntermediary < 3)
                {
                    Destroy(CurrentIntermediary.inter);
                    CurrentIntermediary = Intermediaries.Dequeue();
                }
            }
            
            // This checks if the user has reached the marker, and if the current step should be updated
            double distanceToStep = NavigationCalculator.getDistance(pose.Latitude, pose.Longitude, CurrentStep.latitude, CurrentStep.longitude);
            if (distanceToStep < 5) // If within 5 meters of the marker, navigation can be updated
            {
                CurrentStep = Steps.Dequeue(); // Dequeue the next step
                Destroy(displayCurrentStep); // Destroy the display of the current step
                // Destroy all arrows of the current step to declutter
                while (Intermediaries.Count > 0)
                {
                    Destroy(CurrentIntermediary.inter);
                    CurrentIntermediary = Intermediaries.Dequeue();
                }
                
                // Height of the phone - 1.5m to be approximately the height of the ground
                Altitude = pose.Altitude - 3f;
                
                if (Steps.Count > 0) // If there are additional steps in the navigation, proceed to update them as normal
                {
                    double heading = NavigationCalculator.getHeading(CurrentStep.latitude, CurrentStep.longitude, Steps.Peek().latitude, Steps.Peek().longitude);
                    Quaternion quaternion = Quaternion.AngleAxis(180f - (float)heading, Vector3.up);

                    getIntermediaries(pose);
                    CurrentIntermediary = Intermediaries.Dequeue();
                
                    // Create anchors at specified position and orientation
                    ARGeospatialAnchor currentAnchor = AnchorManager.AddAnchor(CurrentStep.latitude, CurrentStep.longitude, Altitude, quaternion);
                    // Materialize the object if the anchor is correctly created
                    if (currentAnchor != null)
                    {
                        displayCurrentStep = Instantiate(MarkerPrefab, currentAnchor.transform);
                    }
                }
                else // Otherwise destroy any remaining game objects and do nothing else
                {
                    Destroy(CurrentIntermediary.inter);
                    while (Intermediaries.Count > 0)
                    {
                        CurrentIntermediary = Intermediaries.Dequeue();
                        Destroy(CurrentIntermediary.inter);
                    }
                }
            }
        }
    }
}


