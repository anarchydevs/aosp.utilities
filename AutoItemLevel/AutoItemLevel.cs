using System;
using System.Diagnostics;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Common.GameData;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using AOSharp.Common.GameData.UI;
using System.IO;
using Newtonsoft.Json;
using AOSharp.Core.Inventory;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using SmokeLounge.AOtomation.Messaging.GameData;
using System.Linq;

namespace AutoItemLevel
{
    public class AutoItemLevel : AOPluginEntry
    {
        public Dictionary<string, CharacterSettings> CharSettings { get; set; }

        static double Delay;

        bool leftArmEquipped = false;
        bool rightArmEquipped = false;
        bool nextArmIsLeft = true;

        public static Config Config { get; private set; }

        public static Window _infoWindow;
        public static View _infoView;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        protected Settings _settings;

        public static string PluginDir;

        private bool IsActiveWindow => GetForegroundWindow() == Process.GetCurrentProcess().MainWindowHandle;

        public override void Run(string pluginDir)
        {
            Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\AOSP\\AutoItemLevel\\{DynelManager.LocalPlayer.Name}\\Config.json");

            PluginDir = pluginDir;

            _settings = new Settings("AutoItemLevel");

            RegisterSettingsWindow("Auto Item", "AutoItemLevelSettingWindow.xml");

            Game.OnUpdate += OnUpdate;

            _settings.AddVariable("Enable", false);
            _settings["Enable"] = false;

            _settings.AddVariable("Newcomer", false);

            Chat.WriteLine("AutoItemLevel Loaded!");
            Chat.WriteLine("/autoitem for settings.");
        }

        public override void Teardown()
        {
            SettingsController.CleanUp();
        }

        public Window[] _windows => new Window[] { };

        private void InfoView(object s, ButtonBase button)
        {
            _infoWindow = Window.CreateFromXml("Info", PluginDir + "\\UI\\AutoItemLevelInfoView.xml",
                windowSize: new Rect(0, 0, 440, 510),
                windowStyle: WindowStyle.Default,
                windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

            _infoWindow.Show(true);
        }

        protected void RegisterSettingsWindow(string settingsName, string xmlName)
        {
            SettingsController.RegisterSettingsWindow(settingsName, PluginDir + "\\UI\\" + xmlName, _settings);
        }

        private void OnUpdate(object s, float deltaTime)
        {

            //if (!_settings["Enable"].AsBool())
            //{
            //    return;
            //}

            if (Time.AONormalTime > Delay + 0.5)
            {
                if (_settings["Newcomer"].AsBool())
                {
                    int playerLevel = DynelManager.LocalPlayer.Level;

                    foreach (Item item in Inventory.Items)
                    {
                        if (item.Name.Contains("Newcomer"))
                        {
                            // Step 1: Move armor to inventory if its QualityLevel doesn't match the player's level
                            if (item.QualityLevel != playerLevel && item.Slot.Type != IdentityType.Inventory)
                            {
                                item.MoveToInventory();
                            }

                            // Step 2: Use item in inventory to level up
                            if (item.Slot.Type == IdentityType.Inventory && item.QualityLevel != playerLevel)
                            {
                                item.Use(); // Level the armor
                            }

                            // Step 3: Equip item
                            Identity leftArmIdentity = new Identity(IdentityType.ArmorPage, (int)EquipSlot.Cloth_LeftArm);
                            List<EquipSlot> equipSlots = item.EquipSlots;

                            // If left arm is empty and the item is a sleeve, equip it there first
                            if (!Inventory.Find(leftArmIdentity, out _) && item.Name.Contains("Sleeve"))
                            {
                                item.Equip(EquipSlot.Cloth_LeftArm);

                            }
                            else
                            {
                                foreach (EquipSlot equipSlot in item.EquipSlots)
                                {
                                    item.Equip(equipSlot);
                                    break;  // Equip the item only once
                                }
                            }
                        }
                    }
                }

                #region UI

                if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
                {
                   // SettingsController.settingsWindow.FindView("ChannelBox", out TextInputView channelInput);

                    if (SettingsController.settingsWindow.FindView("AutoItemLevelInfoView", out Button infoView))
                    {
                        infoView.Tag = SettingsController.settingsWindow;
                        infoView.Clicked = InfoView;
                    }
                }

                #endregion

                Delay = Time.AONormalTime;
            }
        }
    }
    public class Config
    {
        public Dictionary<string, CharacterSettings> CharSettings { get; set; }

        protected string _path;

        public static Config Load(string path)
        {
            Config config;

            try
            {
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));

                config._path = path;
            }
            catch
            {
                Chat.WriteLine($"No config file found.");
                Chat.WriteLine($"Using default settings");

                config = new Config
                {
                    CharSettings = new Dictionary<string, CharacterSettings>()
                    {
                        { DynelManager.LocalPlayer.Name, new CharacterSettings() }
                    }
                };

                config._path = path;

                config.Save();
            }

            return config;
        }

        public void Save()
        {
            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\AOSP\\AutoItemLevel\\{DynelManager.LocalPlayer.Name}"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\AOSP\\AutoItemLevel\\{DynelManager.LocalPlayer.Name}");

            File.WriteAllText(_path, JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented));
        }
    }

    public class CharacterSettings
    {

    }
}

