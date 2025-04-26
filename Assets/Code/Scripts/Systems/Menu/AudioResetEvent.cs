using System;


namespace AF.TS.Audio
{
    public static class AudioResetEvent
    {
        public static event Action OnAudioReset;

        public static void Raise()
        {
            OnAudioReset?.Invoke();
        }
    }
}