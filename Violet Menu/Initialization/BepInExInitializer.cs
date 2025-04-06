using BepInEx;
using HarmonyLib;
using static VioletTemp.Initialization.PluginInfo;

namespace VioletTemp.Initialization
{
    [BepInPlugin(menuGUID, menuName, menuVersion)]
    public class BepInExInitializer : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource LoggerInstance;

        void Awake()
        {
            LoggerInstance = Logger;
            new Harmony(menuGUID).PatchAll();
        }
    }
}
