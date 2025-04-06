using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine.InputSystem;
using UnityEngine;
using Object = UnityEngine.Object;
using static VioletTemp.Utilities.ColorLib;
using static VioletTemp.Utilities.Variables;
using static VioletTemp.Menu.Main;
using static VioletTemp.Menu.ButtonHandler;
using static VioletTemp.Menu.Optimizations;
using VioletTemp.Utilities;
using VioletTemp.Menu;
using static VioletTemp.Mods.Categories.Move;
using static VioletTemp.Utilities.Patches.OtherPatches;
using static VioletTemp.Utilities.GunTemplate;
using System.Linq;
using Oculus.Platform;
using Photon.Pun;

namespace VioletTemp.Mods.Categories
{
    public class Settings
    {
        public static void SwitchHands(bool setActive)
        {
            rightHandedMenu = setActive;
        }

        public static void ClearNotifications()
        {
            NotificationLib.ClearAllNotifications();
        }

        public static void ToggleNotifications(bool setActive)
        {
            toggleNotifications = setActive;
        }

        public static void ToggleDisconnectButton(bool setActive)
        {
            toggledisconnectButton = setActive;
        }
        public static void Discord()
        {
            UnityEngine.Application.OpenURL("https://discord.gg/violetmenu");
        }

    }
}
