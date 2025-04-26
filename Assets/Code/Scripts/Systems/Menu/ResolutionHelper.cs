using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace AF.TS.Utils
{
    public static class ResolutionHelper
    {
        public static List<Resolution> AvailableResolutions { get; private set; } = new();

        public static void PopulateResolutionDropdown(TMP_Dropdown dropdown, out int currentResolutionIndex)
        {
            dropdown.ClearOptions();
            AvailableResolutions.Clear();

            Resolution[] allResolutions = Screen.resolutions;
            HashSet<string> uniqueResolutions = new();

            List<string> options = new();
            currentResolutionIndex = 0;

            for (int i = 0; i < allResolutions.Length; i++)
            {
                string resolutionString = allResolutions[i].width + " x " + allResolutions[i].height;

                if (!uniqueResolutions.Contains(resolutionString))
                {
                    uniqueResolutions.Add(resolutionString);
                    AvailableResolutions.Add(allResolutions[i]);
                    options.Add(resolutionString);

                    if (allResolutions[i].width == Screen.currentResolution.width &&
                        allResolutions[i].height == Screen.currentResolution.height)
                    {
                        currentResolutionIndex = options.Count - 1;
                    }
                }
            }

            dropdown.AddOptions(options);
        }

        public static Resolution GetResolutionByIndex(int index)
        {
            if (index < 0 || index >= AvailableResolutions.Count)
                return Screen.currentResolution;

            return AvailableResolutions[index];
        }
    }
}