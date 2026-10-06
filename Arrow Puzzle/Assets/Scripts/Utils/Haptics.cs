using UnityEngine;


    public static class Haptics
    {
        public static void Wrong() { if (Application.isMobilePlatform) Handheld.Vibrate(); }
        public static void Win() { if (Application.isMobilePlatform) Handheld.Vibrate(); }
    }

