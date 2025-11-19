using System.Text;
using BepInEx;
using BepInEx.Configuration;
using Photon.Pun;
using ExitGames.Client.Photon;
using ComputerInterface.Behaviours;
using ComputerInterface.Interfaces;


namespace PropertiesManager
{
    [BepInPlugin("com.uhclash.gorillatag.PropertiesManager", "PropertiesManager", "1.0.0")]
    [BepInDependency("tonimacaroni.computerinterface", "1.8.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool NukeEnabled = false;
        private static ConfigEntry<bool> _nukeEnabledConfig;

        void Awake()
        {
            _nukeEnabledConfig = Config.Bind("Settings", "NukeEnabled", false, "Is the property nuke enabled?");
            NukeEnabled = _nukeEnabledConfig.Value;
        }

        void OnEnable()
        {
            HarmonyPatches.ApplyHarmonyPatches();
            new PropertiesCommandManager().Initialize();
        }

        void OnDisable()
        {
            HarmonyPatches.RemoveHarmonyPatches();
        }

        public static void SaveNukeState()
        {
            _nukeEnabledConfig.Value = NukeEnabled;
        }
    }

    public class PropertiesCommandManager : ICommandRegistrar
    {
        public void Initialize()
        {
            RegisterCommands();
        }

        public void RegisterCommands()
        {
            var commandHandler = CommandHandler.Singleton;
            
            commandHandler.AddCommand(new Command("myprops", null, args =>
            {
                var localPlayer = PhotonNetwork.LocalPlayer;
                var stringBuilder = new StringBuilder();
    
                stringBuilder.AppendLine($"Your Properties ({localPlayer.NickName}):\n");

                if (localPlayer.CustomProperties.Count == 0)
                {
                    stringBuilder.AppendLine("You have no custom properties!");
                }
                else
                {
                    int count = 0;
                    foreach (var prop in localPlayer.CustomProperties)
                    {
                        stringBuilder.Append($"{prop.Key}");
                        count++;

                        if (count < localPlayer.CustomProperties.Count)
                        {
                            stringBuilder.Append(", ");
                        }

                        if (count % 4 == 0 && count < localPlayer.CustomProperties.Count)
                        {
                            stringBuilder.AppendLine();
                        }
                    }

                    if (count % 4 != 0)
                    {
                        stringBuilder.AppendLine();
                    }
                }

                return stringBuilder.ToString();
            }));
            
            commandHandler.AddCommand(new Command("clearprops", null, args =>
            {
                var localPlayer = PhotonNetwork.LocalPlayer;
                if (localPlayer.CustomProperties.Count == 0)
                {
                    return "You have no properties to clear!";
                }
                
                var propsToRemove = new Hashtable();
                
                foreach (var key in localPlayer.CustomProperties.Keys)
                {
                    if ((string)key != "didTutorial")
                    {
                        propsToRemove[key] = null;
                    }
                }
                
                localPlayer.SetCustomProperties(propsToRemove);
                return "Properties cleared!";
            }));

            commandHandler.AddCommand(new Command("togglepropnuke", null, args =>
            {
                Plugin.NukeEnabled = !Plugin.NukeEnabled;
                Plugin.SaveNukeState();

                string status = Plugin.NukeEnabled ? "ENABLED" : "DISABLED";
                string description = Plugin.NukeEnabled
                    ? "All custom properties are now being blocked!"
                    : "All custom properties are now allowed!";

                return $"Property Nuke: {status}\n\n{description}\n(Saved for next restart)";
            }));
        }
    }

}

