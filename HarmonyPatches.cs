using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Realtime;

namespace PropertiesManager
{
    public class HarmonyPatches
    {
        private static Harmony instance;

        public static bool IsPatched { get; private set; }
        public const string InstanceId = "com.uhclash.gorillatag.propsmanager";

        public static void ApplyHarmonyPatches()
        {
            if (!IsPatched)
            {
                if (instance == null)
                {
                    instance = new Harmony(InstanceId);
                }

                instance.PatchAll(typeof(HarmonyPatches).Assembly);
                IsPatched = true;
            }
        }

        public static void RemoveHarmonyPatches()
        {
            if (instance != null && IsPatched)
            {
                instance.UnpatchSelf();
                IsPatched = false;
            }
        }

        [HarmonyPatch(typeof(Player), "CustomProperties", MethodType.Setter)]
        internal class PropSetterPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref Hashtable value)
            {
                if (!Plugin.NukeEnabled)
                    return true;
                
                foreach (var prop in value)
                {
                    if ((string)prop.Key != "didTutorial")
                        return false;
                }
                
                return true;
            }
        }

        [HarmonyPatch(typeof(Player), "SetCustomProperties", MethodType.Normal)]
        internal class SetPropPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref Hashtable propertiesToSet)
            {
                if (!Plugin.NukeEnabled)
                    return true;
                
                foreach (var prop in propertiesToSet)
                {
                    if ((string)prop.Key != "didTutorial")
                        return false;
                }
                
                return true;
            }
        }
    }
}