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
    public class NavigationManager
    {
        static String apiKey = "AIzaSyCJv1juPlayfNG086LAUQ4rtwEDqsT7DZA";
        static String baseNavigationUrl = "https://maps.googleapis.com/maps/api/directions/json";
        // static String baseRoadsUrl = "https://roads.googleapis.com/v1/snapToRoads";
        public static Queue<(double latitude, double longitude, String htmlStep)> getDirections(GeospatialPose pose, String destination)
        {
            // Generate the URL to make the API call
            String origin = pose.Latitude.ToString() + "," + pose.Longitude.ToString();
            String formattedDest = destination.Replace(" ", "+");
            String apiUrl = baseNavigationUrl + "?origin=" + origin + "&destination=" + formattedDest + "&mode=walking&key=" + apiKey;
            Console.WriteLine("COMPLETE URL: " + apiUrl);

            // Parse the JSON response from the URL to generate a queue of navigation steps
            Queue<(double latitude, double longitude, String htmlStep)> steps = new Queue<(double latitude, double longitude, String htmlStep)>();
            using (var client = new WebClient())
            {
                string response = client.DownloadString(apiUrl);
                if (!string.IsNullOrEmpty(response))
                {
                    JObject json = JObject.Parse(response);
                    foreach (JToken jtoken in json.SelectToken("routes[0].legs[0].steps"))
                    {   
                        double lat = 0.0;
                        double lng = 0.0;
                        String htmlStep = jtoken.SelectToken("html_instructions").ToString().Split("<div")[0];

                        double.TryParse(jtoken.SelectToken("end_location.lat").ToString(), out lat);
                        double.TryParse(jtoken.SelectToken("end_location.lng").ToString(), out lng);

                        steps.Enqueue((lat, lng, htmlStep));
                    }
                }
            }
            return steps;
        }
    }
}