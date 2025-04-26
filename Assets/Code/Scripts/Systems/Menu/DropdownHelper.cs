using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace AF.TS.Utils
{
    public static class DropdownHelper
    {
        private static readonly List<AudioSpeakerMode> AllowedSpeakerModes = new()
        {
            AudioSpeakerMode.Mono,
            AudioSpeakerMode.Stereo,
            AudioSpeakerMode.Mode5point1,
            AudioSpeakerMode.Mode7point1
        };

        public static void PopulateAudioSpeakerModeDropdown(TMP_Dropdown dropdown)
        {
            if (dropdown == null)
            {
                Debug.LogWarning("[DropdownHelper] Dropdown reference is null.");
                return;
            }

            dropdown.ClearOptions();

            List<string> options = new();
            foreach (var mode in AllowedSpeakerModes)
            {
                options.Add(mode.ToString());
            }

            dropdown.AddOptions(options);
        }

        public static int GetSpeakerModeIndex(AudioSpeakerMode mode)
        {
            for (int i = 0; i < AllowedSpeakerModes.Count; i++)
            {
                if (AllowedSpeakerModes[i] == mode)
                    return i;
            }
            return 0; // Default
        }

        public static AudioSpeakerMode GetSpeakerModeByIndex(int index)
        {
            if (index < 0 || index >= AllowedSpeakerModes.Count)
                return AudioSpeakerMode.Stereo;

            return AllowedSpeakerModes[index];
        }
    }
}
