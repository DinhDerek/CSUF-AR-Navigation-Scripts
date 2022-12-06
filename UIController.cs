using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace CSUF_AR_Navigation
{
    public class UIController : MonoBehaviour
    {
        public static VisualElement root;
        public static VisualElement DistanceBackground;
        public static VisualElement CompletedButtonContainer;
        public static TextField DestinationInput;
        public static Label CurrentStepText;
        public static Label DistanceText;
        private void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            CurrentStepText = root.Q<Label>("CurrentStepLabel");
            DistanceText = root.Q<Label>("DistanceLabel");
            DistanceBackground = root.Q<VisualElement>("BottomTextContainer");
            CompletedButtonContainer = root.Q<VisualElement>("CompletedTextContainer");
            DestinationInput = root.Q<TextField>("DestinationInput");
            Button DestinationButton = root.Q<Button>("DestinationButton");
            Button DismissButton = root.Q<Button>("DismissButton");
            
            DistanceBackground.style.visibility = Visibility.Hidden;
            CompletedButtonContainer.style.visibility = Visibility.Hidden;

            DestinationButton.clicked += sendDestination;
            DismissButton.clicked += resetNavigation;
        }
        public static void showTrackingInfo(bool initialized, string status, string destination, string currentStep, string distance)
        {
            if (!initialized) // If the navigation has not been initialized, let the user know that it is pending
            {
                CurrentStepText.text = "Loading navigation to <b>" + destination + "</b>";
                // OutputText.text = "Loading...\nLow Tracking Accuracy\n";
            }
            else // The normal display of information to the user
            {
                DistanceBackground.style.visibility = Visibility.Visible;
                CurrentStepText.text = currentStep + "\n" + status;
                DistanceText.text = distance + " meters away";
            }
        }   

        private static void resetNavigation()
        {
            CompletedButtonContainer.style.visibility = Visibility.Hidden;
            DistanceBackground.style.visibility = Visibility.Hidden;
            DestinationInput.style.display = DisplayStyle.Flex;
            CurrentStepText.text = "Enter your target destination";
        }

        public static void displayNavComplete(string msg)
        {
            CurrentStepText.text = msg;
            CompletedButtonContainer.style.visibility = Visibility.Visible;
            DistanceBackground.style.visibility = Visibility.Hidden;
        }

        private static void sendDestination()
        {
            DestinationInput.style.display = DisplayStyle.None;

            SampleScript.setDestination(DestinationInput.text);

            CurrentStepText.text = "Loading...";
        }

        public static void retrySendDestination()
        {
            DestinationInput.style.display = DisplayStyle.Flex;
            CurrentStepText.text = "There was an error finding your destination, please try again";
        }
    }
}