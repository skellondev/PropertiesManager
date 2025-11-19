using BepInEx;
using BepInEx.Configuration;
using ComputerInterface.Behaviours;
using ComputerInterface.Interfaces;
using ExitGames.Client.Photon;
using Photon.Pun;
using System.Linq;

namespace PropertiesManager
{
    [BepInPlugin("com.uhclash.gorillatag.propsmanager", "PropertiesManager", "1.0.0")]
    [BepInDependency("tonimacaroni.computerinterface", "1.8.0")]
    public class Plugin : BaseUnityPlugin
    {
        static ConfigEntry<bool> NukeConfig;
        static readonly ConfigDefinition NukeSetting = new("Property Nuker", "Enabled");
        public static bool NukeEnabled
        {
            get => NukeConfig.Value;
            set => NukeConfig.Value = value;
        }
        void Awake() => NukeConfig = Config.Bind(NukeSetting, false, new ConfigDescription($"Should PropertiesManager disable properties all together?"));
        void Start()
        {
            var commandHandler = CommandHandler.Singleton;

            commandHandler.AddCommand(new Command("myprops", null, args =>
            {
                return $"Properties:\n\n{string.Join('\n', PhotonNetwork.LocalPlayer.CustomProperties.Select(P => $"{P.Key}: {P.Value}"))}";
            }));

            commandHandler.AddCommand(new Command("clearprops", null, args =>
            {
                var props = PhotonNetwork.LocalPlayer.CustomProperties;
                if (props.Count == 0) return "You have no properties to clear!";

                var propsToRemove = new Hashtable();
                foreach (var key in props.Keys)
                {
                    if (key is not string str || str != "didTutorial")
                    {
                        propsToRemove[key] = null;
                    }
                }

                PhotonNetwork.LocalPlayer.SetCustomProperties(propsToRemove);
                return $"Cleared {props.Count} total props!";
            }));

            commandHandler.AddCommand(new Command("propnuke", [typeof(bool)], args =>
            {
                if (args.Length == 0 || args[0] is not bool enabled) return "An unexpected error occured.";

                Plugin.NukeEnabled = enabled;
                return enabled ? "The property nuke has been activated!" : "The property nuke has been disabled!";
            }));
        }
        void OnEnable() => HarmonyPatches.ApplyHarmonyPatches();
        void OnDisable() => HarmonyPatches.RemoveHarmonyPatches();
    }
}