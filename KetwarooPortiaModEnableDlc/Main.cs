using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityModManagerNet;
using HarmonyLib;
using System.Reflection;
using Pathea.IntendNs;
using Pathea.InputSolutionNs;
using Pathea;
using Pathea.GameFlagNs;
using Pathea.ACT;
using Pathea.ModuleNs;
using Pathea.ActorNs;
using System.Text.RegularExpressions;

namespace KetwarooPortiaModEnableDlc
{
    public class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry mod;
        public static Settings modSettings;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            modSettings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            mod = modEntry;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }


        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;

            return true;
        }


        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            modSettings.Draw(modEntry);
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            modSettings.Save(modEntry);
        }

    }



    public class Settings : UnityModManager.ModSettings, IDrawable
    {

        //Kickstarter1,
        //Kickstarter2,
        //Kickstarter3,
        //Kickstarter4,
        //Kickstarter5,
        //Bikini,
        //NpcCloth,
        //PlayerCloth,
        [Header("Warning, Can cause permanently(?) duplicated items and pets:")]
        [Draw("Enable Steel Lion")] public bool enableDLc0 = false;
        [Draw("Enable Welding Outfit + Steel lion")] public bool enableDLc1 = false;
        [Draw("Enable Welding Outfit + Steel Lion + Yoyo Pet")] public bool enableDLc2 = false;
        [Draw("Enable YoYo Pet only")] public bool enableDLc4 = false;

        [Header("Unique stuff")]
        [Draw("Enable Kickstarter Lousy T-Shirt")] bool enableDLc3 = false;
        [Draw("Enable Suimsuit")] public bool enableDLc5 = false;
        [Draw("Enable Npc Attire")] public bool enableDLc6 = false;
        [Draw("Enable Player Attire")] public bool enableDLc7 = false;

        private bool[] enabledDlc;

        public void refreshMap()
        {
            enabledDlc = new bool[] {

            enableDLc0,
            enableDLc1,
            enableDLc2,
            enableDLc3,
            enableDLc4,
            enableDLc5,
            enableDLc6,
            enableDLc7

            };

        }

        public bool isDlcEnabled(Pathea.DlcNs.Dlc dlc)
        {
            refreshMap();
            int dlcVal = (int)dlc;
            if (dlcVal < enabledDlc.Length) {
                return enabledDlc[dlcVal]; 
            }
            return false;
        }


        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {
            refreshMap();
        }
    }


    [HarmonyPatch(typeof(Pathea.DlcNs.DLCConfigModule))]
    static class Pathea_DlcNs_DLCConfigModule_Patches
    {

        [HarmonyPostfix]
        [HarmonyPatch("AlwaysDisplay")]
        static void PostfixAlwaysDisplay(ref bool __result)
        {
            if (!Main.enabled)
                return;

            __result = true;
        }
    }

    [HarmonyPatch(typeof(Pathea.DistributeChannelNs.ChannelMgr))]
    static class Pathea_DistributeChannelNs_ChannelMgr_Patches
    {

        [HarmonyPrefix]
        [HarmonyPatch("IsDlcInstalled")]
        static void PrefixIsDlcInstalled(Pathea.DlcNs.Dlc dlc, out Pathea.DlcNs.Dlc __state)
        {
            __state = dlc;

            if (!Main.enabled)
                return;
            
        }
        
        [HarmonyPostfix]
        [HarmonyPatch("IsDlcInstalled")]
        static void PostfixIsDlcInstalled(ref bool __result, Pathea.DlcNs.Dlc __state)
        {
            if (!Main.enabled)
                return;

            __result = Main.modSettings.isDlcEnabled(__state);

            Main.mod.Logger.Log(string.Format("{0},{1}",__state,__result));
        }
    }
}
