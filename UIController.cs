using System;
using UnityEngine;
using UnityEngine.UIElements;
namespace AR_Fukuoka
{
    public class UIController : MonoBehaviour
    {
        public static VisualElement root;
        public static VisualElement DistanceBackground;
        public static Label CurrentStepText;
        public static Label DistanceText;
        private void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            CurrentStepText = root.Q<Label>("CurrentStepLabel");
            DistanceText = root.Q<Label>("DistanceLabel");
            DistanceBackground = root.Q<VisualElement>("BottomTextContainer");
            DistanceBackground.style.visibility = Visibility.Hidden;
        }
        public static void ShowTrackingInfo(bool initialized, string status, string destination, string currentStep, string distance)
            {

                if (!initialized) // If the navigation has not been initialized, let the user know that it is pending
                {
                    CurrentStepText.text = "Loading navigation to <b>" + destination + "</b>\n";
                    // OutputText.text = "Loading...\nLow Tracking Accuracy\n";
                }
                else if (status == "Destination reached!") // If navigation is complete, simply let the user know
                {
                    CurrentStepText.text = status;
                }
                else // The normal display of information to the user
                {
                    DistanceBackground.style.visibility = Visibility.Visible;
                    CurrentStepText.text = string.Format(
                        "{0}\n" +
                        "{1}\n"
                        ,
                        currentStep,          //{0}
                        status                         //{1}
                    );
                    DistanceText.text = string.Format(
                        "{0} meters away\n"
                        ,
                        distance      //{0}
                    );
                }
            }   
    }
}