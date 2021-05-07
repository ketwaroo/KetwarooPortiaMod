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
using Pathea.ItemSystem;

namespace KetwarooPortiaModGiveItem
{
    public class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry mod;
        public static Settings modSettings;
        public static List<ItemObject> items;
        public static Pathea.UISystemNs.ItemChoiceCtr itemMenu;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            modSettings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            mod = modEntry;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnUpdate = OnUpdate;

            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }
        public static void GiveItem(int itemId, int number)
        {
            mod.Logger.Log(string.Format("give item {0} {1}", itemId, number));
            Pathea.Player.Self.bag.AddItem(ItemObject.CreateItem(itemId, number), true);
        }

        static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {

            if (
                null == itemMenu
                && Input.GetKeyDown(Main.modSettings.GiveItemKey.keyCode)
                && Pathea.Player.Self.actor != null
              )
            {

                if (items == null)
                {
                    mod.Logger.Log("making item list");
                    items = new List<ItemObject>();

                    // items.Add(ItemObject.CreateItem(ItemConstants.ItemID_Sofa, 1));
                    // items.Add(ItemObject.CreateItem(ItemConstants.ItemID_BronzeSword, 1));

                    ItemDataMgr.Instance.ForeachItemBase(delegate (ItemBaseConfData itemData)
                    {

                        if (
                        itemData.ItemType.Contains(ItemType.BoxItem)
                        || itemData.ItemType.Contains(ItemType.Equipment)
                        || itemData.ItemType.Contains(ItemType.FarmFloor)
                        || itemData.ItemType.Contains(ItemType.FarmItem)
                        || itemData.ItemType.Contains(ItemType.Food)
                        // those can cause crashes?
                        // || itemData.ItemType.Contains(ItemType.CreationBook) // assembly
                        // || itemData.ItemType.Contains(ItemType.ProductionBook) // craft
                        || itemData.ItemType.Contains(ItemType.Home)
                        || itemData.ItemType.Contains(ItemType.HomeMaterial)
                        || itemData.ItemType.Contains(ItemType.HomeSystemUnit)
                        || itemData.ItemType.Contains(ItemType.Material)
                        || itemData.ItemType.Contains(ItemType.Nutrient)
                        || itemData.ItemType.Contains(ItemType.NutrientContainer)
                        || itemData.ItemType.Contains(ItemType.Other)
                        || itemData.ItemType.Contains(ItemType.PutDownItem)
                        || itemData.ItemType.Contains(ItemType.Seed)
                        )
                        {
                            items.Add(ItemObject.CreateItem(itemData.ID, 999));
                        }
                    });

                    items.Sort(delegate (ItemObject a, ItemObject b)
                    {

                        int num3 = (2 * SortCompareType(a, b)) + SortCompareID(a, b);

                        if (num3 == 0)
                        {
                            return 0;
                        }
                        return num3 > 0 ? 1 : -1;

                    });
                }

                itemMenu = Pathea.UISystemNs.UIUtils.ShowItemChoice(items, null, true, GiveItem, false);

                // because this doesn't work by default?
                Pathea.UISystemNs.UIStateComm.Instance.SetCursor(true);
                itemMenu.OnCancelCallBack = delegate
                {
                    Pathea.UISystemNs.UIStateComm.Instance.SetCursor(false);
                };

            }

        }

        // Token: 0x0600391D RID: 14621 RVA: 0x001039C0 File Offset: 0x00101DC0
        static int SortCompareType(ItemObject cp1, ItemObject cp2)
        {
            int orderIndex = cp1.ItemBase.OrderIndex;
            int orderIndex2 = cp2.ItemBase.OrderIndex;
            return (orderIndex != orderIndex2) ? ((orderIndex <= orderIndex2) ? 1 : -1) : 0;
        }

        // Token: 0x0600391E RID: 14622 RVA: 0x00103A00 File Offset: 0x00101E00
        static int SortCompareID(ItemObject cp1, ItemObject cp2)
        {
            return (cp1.ItemDataId != cp2.ItemDataId) ? ((cp1.ItemDataId <= cp2.ItemDataId) ? 1 : -1) : 0;
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
        [Header("Keys")]
        [Draw("Give Item Key")] public UnityModManagerNet.KeyBinding GiveItemKey = new UnityModManagerNet.KeyBinding { keyCode = KeyCode.Z };
        [Draw("Max Selected Item Stack")] public UnityModManagerNet.KeyBinding MaxItemStackKey = new UnityModManagerNet.KeyBinding { keyCode = KeyCode.X };

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {
        }
    }


    // Pathea.UISystemNs.PackageUIBase
    [HarmonyPatch(typeof(Pathea.UISystemNs.PackageUIBase))]
    class Pathea_UISystemNs_PackageUIBase_Patches
    {
        static ItemObject currentSelection;

        [HarmonyPostfix]
        [HarmonyPatch("curSelectItem", MethodType.Getter)]
        static void PostfixUpdate(ref ItemObject __result)
        {
            currentSelection = __result;
        }


        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        static void PostfixUpdate(ref Pathea.UISystemNs.PackageUIBase __instance)
        {
            if (
                 null != currentSelection
                 && Input.GetKeyDown(Main.modSettings.MaxItemStackKey.keyCode)
                 && currentSelection.Number < 999
               )
            {
                currentSelection.ChangeNumber(999 - currentSelection.Number);
                __instance.FreshCurpageItem();
            }

        }

    }

}