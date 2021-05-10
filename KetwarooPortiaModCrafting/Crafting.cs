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
using System.Text.RegularExpressions;
using System.Collections;
using System.Linq;

namespace KetwarooPortiaModCrafting
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

            Singleton<Pathea.TipsNs.TipsMgr>.Instance.SendSimpleTip(message);
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

        [Header("Crafting/Resources")]

        [Draw("Crafting Cost Multiplier (floors at 1 material cost.)")] public float CostCrafting = 1.0F;
        [Draw("Crafting Time Multiplier (0.5 halves time)")] public float TimeCraftingMultiplier = 1.0F;

        [Draw("Factory Performance/Transport Upgrades Level Cap")] public int FactoryPerformanceLevelCap = 999;
        [Draw("Factory Performance/Transport Gain Per Level")] public float FactoryPerformanceLevelGain = 0.05F;

        [Draw("Connect Factory Resources For Crafting")] public bool FactoryConnectCraftingResources = true;


        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {
            CostCrafting = Mathf.Clamp(CostCrafting, 0.001F, 99F);
            TimeCraftingMultiplier = Mathf.Clamp(TimeCraftingMultiplier, 0.001F, 99F);
        }
    }


    //Pathea.FarmFactoryNs.FarmFactoryMgr.Self.GetFactory();
    [HarmonyPatch(typeof(Pathea.FarmFactoryNs.FarmFactoryMgr))]
    static class Pathea_FarmFactoryNs_FarmFactoryMgr_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("GetFactory")]
        static void PostfixGetFactory(Pathea.FarmFactoryNs.FarmFactory __result, int id)
        {
            if (__result != null)
            {
                FactoryMatHelper.InitItemContainer(id);
            }
        }
    }

    static class FactoryMatHelper
    {
        public static List<int> FactoryItemContainers = new List<int>();

        public static void InitItemContainer(int factoryId)
        {

            if (FactoryItemContainers.Contains(factoryId))
            {
  
                return;
            }
            Main.dmp("FactoryMatHelper::InitItemContainer adding factoryId", factoryId);
            Pathea.HomeNs.IItemContainer factoryMaterialsContainer = new FactorySharedContainerMaterials(factoryId);
            Pathea.HomeNs.HomeModule.Self.AddItemContainer(factoryMaterialsContainer);

            Pathea.HomeNs.IItemContainer factoryFinishedProductContainer = new FactorySharedContainerFinishedProduct(factoryId);
            Pathea.HomeNs.HomeModule.Self.AddItemContainer(factoryFinishedProductContainer);

            FactoryItemContainers.Add(factoryId);
        }
    }

    //CompoundTreeCtr
    [HarmonyPatch(typeof(CompoundTreeCtr))]
    static class CompoundTreeCtr_Patch
    {

        [HarmonyPrefix]
        [HarmonyPatch("GetItemCount")]
        static bool PrefixGetItemCount(ref int __result, int itemId)
        {

            if (!Main.modSettings.FactoryConnectCraftingResources)
            {
                return true;
            }
            int count = Pathea.ItemSystem.ItemPackage.GetItemCount(itemId, true);
            __result = count;

            return false;
        }
    }

    //FarmFactoryProductUICtr
    [HarmonyPatch(typeof(FarmFactoryProductUICtr))]
    static class FarmFactoryProductUICtr_Patch
    {

        [HarmonyPrefix]
        [HarmonyPatch("GetItemCount")]
        static bool PrefixGetItemCount(ref int __result, int itemId)
        {

            if (!Main.modSettings.FactoryConnectCraftingResources)
            {
                return true;
            }
            // already fetched by  Pathea.ItemSystem.ItemPackage.GetItemCount
            __result = 0;

            return false;
        }


    }

    //ItemListInfoUICtr
    [HarmonyPatch(typeof(ItemListInfoUICtr))]
    static class ItemListInfoUICtr_Patch
    {

        [HarmonyPostfix]
        [HarmonyPatch("ClearAll")]
        static void PostfixShowInfo(ref ItemListInfoUICtr __instance)
        {

            if (!Main.modSettings.FactoryConnectCraftingResources)
            {
                return;
            }

            // should default to Pathea.ItemSystem.ItemPackage.GetItemCount 
            __instance.GetItemCount = null;

        }

    }

    //Pathea.FarmFactoryNs.FarmFactory
    [HarmonyPatch(typeof(Pathea.FarmFactoryNs.FarmFactory))]
    static class Pathea_FarmFactoryNs_FarmFactory_Patch
    {

        [HarmonyPrefix]
        [HarmonyPatch("GetMatCount")]
        static bool PrefixGetMatCount(ref int __result, int ietmId, Pathea.FarmFactoryNs.FarmFactory __instance)
        {

            if (!Main.modSettings.FactoryConnectCraftingResources)
            {
                return true;
            }
            int count = Pathea.ItemSystem.ItemPackage.GetItemCount(ietmId, true);
            __result = count;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RemoveAllMat")]
        static bool PrefixRemoveAllMat(List<IdCount> list, int num, Pathea.FarmFactoryNs.FarmFactory __instance)
        {

            if (!Main.modSettings.FactoryConnectCraftingResources)
            {
                return true;
            }

            foreach (global::IdCount idCount in list)
            {
                Main.dmp("Pathea.FarmFactoryNs.FarmFactory::RemoveAllMat", idCount.ToString());
                Pathea.ItemSystem.ItemPackage.RemoveItem(idCount.id, idCount.count, true, true);
            }

            return false;
        }

        //[HarmonyPrefix]
        //[HarmonyPatch("RemoveMat")]
        //[HarmonyPatch(new Type[] { typeof(int), typeof(int) })]
        //static bool PrefixRemoveMat(int id, ref int count, Pathea.FarmFactoryNs.FarmFactory __instance)
        //{

        //    if (!Main.modSettings.FactoryConnectCraftingResources)
        //    {
        //        return true;
        //    }

        //    Pathea.ItemSystem.ItemPackage.RemoveItem(id, count, true, true);

        //    return false;
        //}

        //[HarmonyPrefix]
        //[HarmonyPatch("RemoveMat")]
        //[HarmonyPatch(new Type[] { typeof(List<IdCount>) })]
        //static bool PrefixRemoveMatByList(ref List<IdCount> list, ref List<IdCount> __result, Pathea.FarmFactoryNs.FarmFactory __instance)
        //{
        //    if (!Main.modSettings.FactoryConnectCraftingResources)
        //    {
        //        return true;
        //    }

        //    foreach (IdCount idCount in list)
        //    {
        //        int id = idCount.id;
        //        int count = idCount.count;
        //        int currentCount = Pathea.ItemSystem.ItemPackage.GetItemCount(id, true);

        //        if (currentCount > 0)
        //        {
        //            Pathea.ItemSystem.ItemPackage.RemoveItem(id, count, true, true);
        //            int num = Mathf.Min(currentCount, count);
        //            idCount.count = Mathf.Max(0, idCount.count - num);
        //        }
        //    }

        //    for (int i = list.Count - 1; i >= 0; i--)
        //    {
        //        if (list[i].count <= 0)
        //        {
        //            list.RemoveAt(i);
        //        }
        //    }
        //    __result = list;

        //    return false;
        //}
    }

    // Pathea.CreationFactory.CreationAutomaticRoomData;
    [HarmonyPatch(typeof(Pathea.CreationFactory.CreationAutomaticRoomData))]
    static class Pathea_CreationFactory_CreationAutomaticRoomData_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("RemoveMaterial")]
        static bool PrefixRemoveMaterial(List<global::IdCount> material)
        {
            if (!Main.modSettings.FactoryConnectCraftingResources)
            {
                return true;
            }
            //Main.dump("skipping Pathea.CreationFactory.CreationAutomaticRoomData::RemoveMaterial");
            //foreach (IdCount i in material)
            //{
            //    Main.dmp("i.ToString()", i.ToString());
            //    Main.dmp("i.count", i.count);
            //}
            // prevent double dipping resources.
            Pathea.ItemSystem.ItemPackage.RemoveItemList(material, true, true);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("CheckMaterialItemEnough")]
        static bool PrefixCheckMaterialItemEnough(List<global::IdCount> material, ref bool __result)
        {
            if (!Main.modSettings.FactoryConnectCraftingResources)
            {
                return true;
            }
            //Main.dump("skipping Pathea.CreationFactory.CreationAutomaticRoomData::CheckMaterialItemEnough");
            //foreach (IdCount i in material)
            //{
            //    Main.dmp("i.ToString()", i.ToString());
            //    Main.dmp("i.count", i.count);
            //}
            // prevent double dipping resources.
            bool result = Pathea.ItemSystem.ItemPackage.CheckEnough(material, true);
            __result = result;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetItemCount")]
        static bool PrefixGetItemCount(int id, ref int __result)
        {
            if (!Main.modSettings.FactoryConnectCraftingResources)
            {
                return true;
            }
            //Main.dump("skipping Pathea.CreationFactory.CreationAutomaticRoomData::GetItemCount");
            //Main.dmp("id", id);

            // prevent double dipping resources.
            int result = Pathea.ItemSystem.ItemPackage.GetItemCount(id, true);
            __result = result;

            return false;
        }
    }

    //Pathea.FarmFactoryNs.FarmFactory
    [HarmonyPatch(typeof(Pathea.FarmFactoryNs.FarmFactory))]
    static class Pathea_FarmFactoryNs_FarmFactory_Patches
    {

        [HarmonyPrefix]
        [HarmonyPatch("GetPerformanceData")]
        static void PrefixGetPerformanceData(ref List<Pathea.FarmFactoryNs.Factory_PerformanceLevelData> ___performanceDataList, ref Pathea.FarmFactoryNs.FarmFactory __instance)
        {

            if (null == ___performanceDataList)
            {
                List<Pathea.FarmFactoryNs.Factory_PerformanceLevelData> tmp = new List<Pathea.FarmFactoryNs.Factory_PerformanceLevelData>();


                int maxLevel = Math.Max(Main.modSettings.FactoryPerformanceLevelCap, __instance.PerformanceLevel);

                for (int i = 0; i < maxLevel; i++)
                {

                    Pathea.FarmFactoryNs.Factory_PerformanceLevelData p = new Pathea.FarmFactoryNs.Factory_PerformanceLevelData();

                    Traverse.Create(p).Property("Level").SetValue(i);
                    Traverse.Create(p).Property("Speed").SetValue(1.0F + (i * Main.modSettings.FactoryPerformanceLevelGain));
                    // always cost 1 research note
                    Traverse.Create(p).Property("CostStr").SetValue("2060003,1");
                    //string[] arr = new string[] { "",""};
                    //Traverse.Create(p).Property("PlugListStr").SetValue(arr);
                    //Traverse.Create(p).Field("Cost").SetValue(IdCount.GetIdCountList("2060003,1", ';', ','));
                    tmp.Add(p);
                }

                foreach (Pathea.FarmFactoryNs.Factory_PerformanceLevelData p in tmp)
                {
                    p.Init();
                    //Main.mod.Logger.Log(string.Format("Level {0} {1}", p.Level, p.Cost.ToString()));
                }

                ___performanceDataList = tmp;

            }

        }

        [HarmonyPrefix]
        [HarmonyPatch("GetTransportData")]
        static void PrefixGetTransportData(ref List<Pathea.FarmFactoryNs.Factory_TransmissionLevel> ___transportDataList, ref Pathea.FarmFactoryNs.FarmFactory __instance)
        {

            if (null == ___transportDataList)
            {
                List<Pathea.FarmFactoryNs.Factory_TransmissionLevel> tmp = new List<Pathea.FarmFactoryNs.Factory_TransmissionLevel>();

                int maxLevel = Math.Max(Main.modSettings.FactoryPerformanceLevelCap, __instance.TransportLV);

                for (int i = 0; i < maxLevel; i++)
                {

                    Pathea.FarmFactoryNs.Factory_TransmissionLevel t = new Pathea.FarmFactoryNs.Factory_TransmissionLevel();

                    Traverse.Create(t).Property("Level").SetValue(i);
                    Traverse.Create(t).Property("Speed").SetValue(1.0F + (i * Main.modSettings.FactoryPerformanceLevelGain));
                    // always cost 1 research note
                    Traverse.Create(t).Property("CostStr").SetValue("2060003,1");
                    tmp.Add(t);
                }

                foreach (Pathea.FarmFactoryNs.Factory_TransmissionLevel t in tmp)
                {
                    t.Init();
                    // Main.mod.Logger.Log(string.Format("Level {0} {1}", t.Level, t.Cost.ToString()));
                }

                ___transportDataList = tmp;

            }

        }
        [HarmonyPostfix]
        [HarmonyPatch("GetDashBoardSpeed")]
        static void PostfixGetDashBoardSpeed(ref float __result, ref Pathea.FarmFactoryNs.FarmFactory __instance)
        {
            // should already be applied at compount item level.
            // automatic crafting time  cna be improved with research levels.
            //__result *= Main.modSettings.TimeCraftingMultiplier;
        }

    }


    //Pathea.CompoundSystem.CompoundItem
    [HarmonyPatch(typeof(Pathea.CompoundSystem.CompoundItem))]
    static class Pathea_CompoundSystem_CompoundItem_Patches
    {


        [HarmonyPostfix]
        [HarmonyPatch("CompoundTime", MethodType.Getter)]
        public static void PostfixCompoutTime(ref float __result)
        {
            __result *= Main.modSettings.TimeCraftingMultiplier;
        }

        [HarmonyPostfix]
        [HarmonyPatch("RequireItem1", MethodType.Getter)]
        public static void PostfixRequireItem1(ref Pathea.ItemSystem.ItemObject __result, Pathea.CompoundSystem.CompoundItem __instance)
        {
            RecalcItemObject(ref __result, 0, ref __instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch("RequireItem2", MethodType.Getter)]
        public static void PostfixRequireItem2(ref Pathea.ItemSystem.ItemObject __result, Pathea.CompoundSystem.CompoundItem __instance)
        {
            RecalcItemObject(ref __result, 1, ref __instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch("RequireItem3", MethodType.Getter)]
        public static void PostfixRequireItem3(ref Pathea.ItemSystem.ItemObject __result, Pathea.CompoundSystem.CompoundItem __instance)
        {
            RecalcItemObject(ref __result, 2, ref __instance);
        }

        static Dictionary<int, int[]> originalValues = new Dictionary<int, int[]>();
        public static void RecalcItemObject(ref Pathea.ItemSystem.ItemObject __result, int pos, ref Pathea.CompoundSystem.CompoundItem __instance)
        {

            if (__result == null || __instance == null)
            {
                return;
            }

            // all this to avoid circular references.
            if (!originalValues.ContainsKey(__instance.Id))
            {
                int[] arr = new int[3];
                arr[0] = -1;
                arr[1] = -1;
                arr[2] = -1;
                originalValues.Add(__instance.Id, arr);
            }

            if (originalValues[__instance.Id][pos] == -1)
            {
                originalValues[__instance.Id][pos] = __result.Number;
            }

            int newnum = ChangeCraftingMaterialNumber(originalValues[__instance.Id][pos]);

            // redo 
            if (
                __result.Number != newnum
             )
            {
                __result.DeleteNumber(__result.Number);
                __result.ChangeNumber(newnum);
            }
        }


        // recalc item cost.
        public static int ChangeCraftingMaterialNumber(int matCount)
        {
            if (matCount > 0)
            {
                int newnum = Math.Max((int)Math.Round((matCount * Main.modSettings.CostCrafting), 0, MidpointRounding.AwayFromZero), 1);
                return newnum;
            }

            return matCount;
        }

    }


}
