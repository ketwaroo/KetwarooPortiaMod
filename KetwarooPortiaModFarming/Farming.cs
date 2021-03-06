using UnityEngine;
using UnityModManagerNet;
using HarmonyLib;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections;
using System.Linq;


namespace KetwarooPortiaModFarming
{
    public class Main
    {
        public static float timeTick = 0.0F;
        public static bool enabled;
        public static UnityModManager.ModEntry mod;
        public static Settings modSettings;

        static bool Load(UnityModManager.ModEntry modEntry)
        {

            modSettings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            mod = modEntry;
            UpdateStuff();
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            // modEntry.OnUpdate = OnUpdate;
            // modEntry.OnUnload = OnUnload;

            var harmony = new Harmony(modEntry.Info.Id);

            //UnityEngine.Debug.logger.logEnabled =false;
            Debug.unityLogger.logEnabled = true;

            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }

        static void UpdateStuff()
        {
        }
        static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            modSettings.Draw(modEntry);
            UpdateStuff();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            // fix values first.
            UpdateStuff();
            modSettings.Save(modEntry);

        }

        static void OnUnload(UnityModManager.ModEntry modEntry)
        {
            // fix values first.
            UpdateStuff();
            modSettings.Save(modEntry);

        }

        public static void inGameNotify(string message)
        {
            Pathea.TipsNs.TipsMgr.Instance.SendSimpleTip(message);
        }

        public static void dmp(string key, object obj)
        {
            dump(string.Format("  {0} : {1}", key, obj));
        }

        public static void dump(object obj)
        {
            if (null != obj)
            { mod.Logger.Log(string.Format("{0}", obj.ToString())); }
            else
            { mod.Logger.Log("is null"); }

        }
    }


    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        
        [Header("Animals")]
        [Draw("Farm Animal Max Growth Multiplier (default ~1.2)", Min = 1, Precision = 2)] public float FarmAnimalMaxGrowth = 1.2F;
        [Draw("Farm Animal Rescale Limiter (mitigates super gigantism)", Min = 0.001, Max = 1.0, Type = DrawType.Slider, Precision = 3)] public float FarmAnimalRescaleCoeff = 0.25F;

        [Header("Mounts")]
        [Draw("Mount Training Limit Multiplier", Min = 1, Precision = 2)] public float MountTrainingLimitMultiplier = 1.0F;

        [Header("Plants")]
        [Draw("Planter Harvest Amount Multiplier")] public float PlanterHarvestAmountMultiplier = 1.0F;
        [Draw("Planter Growth Speed Multiplier")] public float PlanterGrowthSpeedMultiplier = 1.0F;
        [Draw("Happy Plant Bonus Coefficient (reduce fertilizer required.)")] public float PlanterHappySeedCoeff = 1.0F;
        [Draw("All Plants All Season")] public bool AllPlantsAllSeason = false;
        [Draw("Remove Replant Limit (default is 20/hour)")] public bool RemoveReplantLimit = false;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {
            FarmAnimalMaxGrowth = Mathf.Clamp(FarmAnimalMaxGrowth, 1.0F, 99F);
        }
    }



    //Pathea.ItemSystem.SeedItemConfData
    [HarmonyPatch(typeof(Pathea.ItemSystem.SeedItemConfData))]
    class Pathea_ItemSystem_SeedItemConfData_Patches
    {

        [HarmonyPrefix]
        [HarmonyPatch("GenerateInfo")]
        static void PrefixGenerateInfo(ref Pathea.ItemSystem.SeedItemConfData __instance)
        {
            if (Main.modSettings.AllPlantsAllSeason)
            {
                __instance.growthRate = new int[] { 10, 10, 10, 10 };
            }
        }

    }

    //Pathea.ItemSystem.ItemSeedCmpt
    [HarmonyPatch(typeof(Pathea.ItemSystem.ItemSeedCmpt))]
    class Pathea_ItemSystem_ItemSeedCmpt_Patches
    {

        [HarmonyPrefix]
        [HarmonyPatch("GrowthRatePerSecond", MethodType.Getter)]
        static void PrefixGrowthRatePerSecond(ref Pathea.ItemSystem.SeedItemConfData ___item)
        {
            if (Main.modSettings.AllPlantsAllSeason)
            {
                ___item.growthRate = new int[] { 10, 10, 10, 10 };
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("GrowthRatePerSecond", MethodType.Getter)]
        static void PostfixGrowthRatePerSecond(ref float __result)
        {
            __result *= Main.modSettings.PlanterGrowthSpeedMultiplier;
        }

        [HarmonyPostfix]
        [HarmonyPatch("FruitGrowthRatePerSecond", MethodType.Getter)]
        static void PostfixFruitGrowthRatePerSecond(ref float __result)
        {
            __result *= Main.modSettings.PlanterGrowthSpeedMultiplier;
        }

        [HarmonyPostfix]
        [HarmonyPatch("GrowPerfectNutrient", MethodType.Getter)]
        static void PostfixGrowPerfectNutrient(ref int __result)
        {
            __result = Mathf.RoundToInt(__result * Main.modSettings.PlanterHappySeedCoeff);
        }

        [HarmonyPostfix]
        [HarmonyPatch("GrowPerfectExtraDrop", MethodType.Getter)]
        static void PostfixGrowPerfectExtraDrop(ref int __result)
        {
            __result = Mathf.RoundToInt(__result * Main.modSettings.PlanterHappySeedCoeff);
        }
    }

    //Pathea.HomeNs.Plant
    [HarmonyPatch(typeof(Pathea.HomeNs.Plant))]
    class Pathea_HomeNs_Plant_Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch("GetHarvestItems")]
        static void PostfixGetHarvestItems(ref List<Pathea.ItemSystem.ItemObject> __result)
        {
            if (null == __result)
            {
                return;
            }

            for (int i = 0; i < __result.Count; i++)
            {

                Pathea.ItemSystem.ItemObject item = __result[i];

                int newnum = Mathf.CeilToInt(item.Number * Main.modSettings.PlanterHarvestAmountMultiplier);
                item.DeleteNumber(item.Number);
                // at least one.
                item.ChangeNumber(Math.Max(1, newnum));

            }
        }
    }

    // Pathea.HomeNs.PlantingBoxUnitMgr
    [HarmonyPatch(typeof(Pathea.HomeNs.PlantingBoxUnitMgr))]
    class Pathea_HomeNs_PlantingBoxUnitMgr_Patches
    {
        // replant on the hour.
        [HarmonyPrefix]
        [HarmonyPatch("Self_HourChangedEvent")]
        static bool PrefixSelf_HourChangedEvent(ref Pathea.HomeNs.PlantingBoxUnitMgr __instance)
        {
            if (!Main.modSettings.RemoveReplantLimit) {
                return true;
            }

            while (__instance.TryStartPlant() != null)
            {
                __instance.EndPlant();
            }

            return false;
        }
        
        // replant on load/wake up
        [HarmonyPrefix]
        [HarmonyPatch("Self_EndLoadEventor")]
        static bool PrefixSelf_EndLoadEventor(ref Pathea.HomeNs.PlantingBoxUnitMgr __instance)
        {
            if (!Main.modSettings.RemoveReplantLimit) {
                return true;
            }

            while (__instance.TryStartPlant() != null)
            {
                __instance.EndPlant();
            }

            return false;
        }
    }
    
    //Pathea.HomeNs.AnimalFarmPlaceCtr
    [HarmonyPatch(typeof(Pathea.HomeNs.AnimalFarmPlaceCtr))]
    class Pathea_HomeNs_AnimalFarmPlaceCtr_Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch("CreateAnimal")]
        static void PostfixGCreateAnimal(ref Pathea.HomeNs.AnimalFarmPlaceCtr __instance, ref List<Pathea.HomeNs.FarmAnimalCtr> ___animCtrList, List<UnityEngine.Transform> ___animalTransList)
        {

        }
    }

    //Pathea.HomeNs.FarmAnimalCtr
    [HarmonyPatch(typeof(Pathea.HomeNs.FarmAnimalCtr))]
    class Pathea_HomeNs_FarmAnimalCtr_Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnStateChange")]
        static void PostfixOnStateChange(ref Pathea.HomeNs.FarmAnimalCtr __instance, Pathea.AnimalFarmNs.AnimalinFarm ___anm)
        {
            float rescale = 1.0F;
            if (null != ___anm)
            {
                // allow mini animals.
                rescale = 0.25f + ((___anm.ProductionMul - 0.5f) * Main.modSettings.FarmAnimalRescaleCoeff);
            }
            __instance.transform.localScale = Vector3.one * rescale;
            //__instance.gameObject.transform.localScale = Vector3.one * rescale;
        }
    }

    //Pathea.AnimalFarmNs.AnimalData
    [HarmonyPatch(typeof(Pathea.AnimalFarmNs.AnimalData))]
    class Pathea_AnimalFarmNs_AnimalData_Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch("TotalPoint", MethodType.Getter)]
        static void PostfixTotalPoint(ref Pathea.AnimalFarmNs.AnimalData __instance, ref int __result)
        {
            __result = (int)(Main.modSettings.FarmAnimalMaxGrowth * __instance.StandardPoint);
        }
    }

    // Pathea.AnimalFarmNs.AnimalinFarm
    [HarmonyPatch(typeof(Pathea.AnimalFarmNs.AnimalinFarm))]
    class Pathea_AnimalFarmNs_AnimalinFarm_Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch("ProductionMul", MethodType.Getter)]
        static void PostfixTotalPoint(ref Pathea.AnimalFarmNs.AnimalinFarm __instance, ref float __result, ref float ___age)
        {
            // make production reflect the percentage above the farm animal instead of maxing out at 1X
            __result = (___age / (float)__instance.Data.StandardPoint) * Main.modSettings.FarmAnimalMaxGrowth;
        }
    }


    // Pathea.RiderNs.RidableInfoData
    [HarmonyPatch(typeof(Pathea.RiderNs.RidableInfoData))]
    class Pathea_RiderNs_RidableInfoData_Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch("JumpingPowerLimit", MethodType.Getter)]
        static void PostfixJumpingPowerLimit(ref Pathea.RiderNs.RidableInfoData __instance, ref Pathea.RiderNs.FloatRange __result)
        {
            __result = __result = new Pathea.RiderNs.FloatRange(__result.aValue, __result.bValue / Main.modSettings.MountTrainingLimitMultiplier);
        }

        [HarmonyPostfix]
        [HarmonyPatch("SpeedGrowthLimit", MethodType.Getter)]
        static void PostfixSpeedGrowthLimit(ref Pathea.RiderNs.RidableInfoData __instance, ref Pathea.RiderNs.FloatRange __result)
        {
            __result = __result = new Pathea.RiderNs.FloatRange(__result.aValue, __result.bValue * Main.modSettings.MountTrainingLimitMultiplier);
        }

        [HarmonyPostfix]
        [HarmonyPatch("VpConsumeFastRunLimit", MethodType.Getter)]
        static void PostfixVpConsumeFastRunLimit(ref Pathea.RiderNs.RidableInfoData __instance, ref Pathea.RiderNs.FloatRange __result)
        {
            __result = __result = new Pathea.RiderNs.FloatRange(__result.aValue, __result.bValue / Main.modSettings.MountTrainingLimitMultiplier);
        }

        [HarmonyPostfix]
        [HarmonyPatch("VpRecoverLimit", MethodType.Getter)]
        static void PostfixVpRecoverLimit(ref Pathea.RiderNs.RidableInfoData __instance, ref Pathea.RiderNs.FloatRange __result)
        {
            __result = new Pathea.RiderNs.FloatRange(__result.aValue, __result.bValue * Main.modSettings.MountTrainingLimitMultiplier);
        }

    }

}
