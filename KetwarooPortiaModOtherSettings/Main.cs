using UnityEngine;
using UnityModManagerNet;
using HarmonyLib;
using System.Reflection;
using System;
using Pathea.IntendNs;
using Pathea.InputSolutionNs;
using Pathea;
using Pathea.GameFlagNs;
using Pathea.ACT;
using Pathea.ModuleNs;
using Pathea.ActorNs;
using System.Collections.Generic;


namespace KetwarooPortiaModOtherSettings
{
    public class Main
    {
        public static float timeTick=0.0F;
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

        [Draw("Dig Intensity")]
        public float DigIntensity = 1.0F;

        [Draw("Dig Radius")]
        public float DigRadius = 1.0F;

        [Draw("Research Speed Multiplier")]
        public float ResearchSpeed = 1.0F;

        [Draw("Vampire Attack (Health on Hit)")]
        public float  HealthLeech = 10.0F;

        [Draw("Vampire Attack (Stamina on Hit)")]
        public float  StaminaLeech = 10.0F; 

        [Draw("Regen Health")]
        public float  HealthPerSecond = 1.0F;

        [Draw("Regen Stamina")]
        public float  StaminaPerSecond = 1.0F;

        [Draw("Run Multiplier (roll?)")]
        public float  RunMultiplier = 1.0F;
        
        [Draw("Crafting Cost Multiplier (floors at 1 material cost.)")]
        public float  CostCrafting = 1.0F;

        [Draw("Longer Jetpack")]
        public bool  ExtendJetpack = false;

        //[Draw("Floor Health/Stamina")] public bool  FreezeHealthStamina = false;
    
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {
        }
    }

    [HarmonyPatch(typeof(Pathea.ConfigNs.OtherConfig))] // make sure Harmony inspects the class
    class Pathea_ConfigNs_OtherConfig_Patches
    {

        static List<MethodBase> methodsToPatch;

        static void Prefix(MethodBase __originalMethod)
        {
            // use __originalMethod to decide what to do
        }
    }

}

