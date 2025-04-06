using static VioletTemp.Utilities.GunTemplate;
using static VioletTemp.Utilities.Variables;
using static VioletTemp.Utilities.ColorLib;
using static VioletTemp.Mods.Categories.Move;
using static VioletTemp.Mods.Categories.Playerr;
using static VioletTemp.Mods.Categories.Room;
using static VioletTemp.Mods.Categories.Settings;
using static VioletTemp.Menu.ButtonHandler;
using static VioletTemp.Menu.Optimizations;
using static VioletTemp.Menu.Optimizations.ResourceLoader;
using static VioletTemp.Menu.Main;
using UnityEngine;
using Fusion;
using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections.Generic;
using VioletTemp.Utilities;
using VioletTemp.Mods.Categories;
using Unity.Mathematics;

namespace VioletTemp.Mods
{
    public enum Category
    {
        // Starting Page
        Home,

        // Mod Categories
        Settings,
        Room,
        Player,
        Move,
        Visuals,
        Creds,
    }
    public class ModButtons
    {
        public static Button[] buttons =
        {
#region Starting Page
            new Button("Settings", Category.Home, false, false, ()=>ChangePage(Category.Settings)),
            new Button("Room", Category.Home, false, false, ()=>ChangePage(Category.Room)),
            new Button("Movement", Category.Home, false, false, ()=>ChangePage(Category.Move)),
            new Button("Player", Category.Home, false, false, ()=>ChangePage(Category.Player)),
            new Button("Visual", Category.Home, false, false, ()=>ChangePage(Category.Visuals)),
            new Button("Creds", Category.Home, false, false, ()=>ChangePage(Category.Creds)),
            #endregion

            #region Settings
            new Button("Disable All Mods", Category.Settings, false, false, ()=>DisableAllMods()),
            new Button("Switch Hands", Category.Settings, true, false, ()=>SwitchHands(true), ()=>SwitchHands(false)),
            new Button("Disconnect Button", Category.Settings, true, true, ()=>ToggleDisconnectButton(true), ()=>ToggleDisconnectButton(false)),
            new Button("Toggle Notifications", Category.Settings, true, true, ()=>ToggleNotifications(true), ()=>ToggleNotifications(false)),
            new Button("Clear Notifications", Category.Settings, false, false, ()=>ClearNotifications()),
            new Button("Change Layout", Category.Settings, false, false, ()=> ChangeMenuLayout()),
            new Button("Change Theme", Category.Settings, false, false, ()=> ChangeTheme()),
            new Button("Change Sound", Category.Settings, false, false, ()=> ChangeSound()),
            new Button("Refresh Menu", Category.Settings, false, false, ()=> RefreshMenu()),
            #endregion

            #region Room
            new Button("Quit Game", Category.Room, true, false, ()=>QuitGTAG()),
            new Button("Join Random", Category.Room, false, false, ()=>JoinRandomPublic()),
            new Button("Disconnect", Category.Room, false, false, ()=>Disconnect()),
            new Button("Primary Disconnect", Category.Room, true, false, ()=>PrimaryDisconnect()),
            new Button("Check If Master", Category.Room, false, false, ()=>IsMasterCheck()),
            #endregion

            #region Movement
            new Button("Toggleable Placeholder", Category.Move, true, false, ()=>Placeholder()),
            new Button("Enabled Placeholder", Category.Move, true, true, ()=>Placeholder()),
            new Button("Placeholder", Category.Move, false, false, ()=>Placeholder()),
            #endregion

            #region Player
            new Button("Toggleable Placeholder", Category.Player, true, false, ()=>Placeholder()),
            new Button("Enabled Placeholder", Category.Player, true, true, ()=>Placeholder()),
            new Button("Placeholder", Category.Player, false, false, ()=>Placeholder()),
            #endregion

            #region Visuals
            new Button("Toggleable Placeholder", Category.Visuals, true, false, ()=>Placeholder()),
            new Button("Enabled Placeholder", Category.Visuals, true, true, ()=>Placeholder()),
            new Button("Placeholder", Category.Visuals, false, false, ()=>Placeholder()),
            #endregion

            #region Credits
            new Button("Menu Credits:", Category.Creds, false, false, ()=>Placeholder()),
            new Button("Violet Template", Category.Creds, false, false, ()=>Placeholder()), // Template you are using
            new Button("NxO Template", Category.Creds, false, false, ()=>Placeholder()), // Template Violet Temp is based off
            new Button("Join The Discord!", Category.Creds, false, false, ()=>Discord()),
            #endregion
        };
    }
}
