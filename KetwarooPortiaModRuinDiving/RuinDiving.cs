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

namespace KetwarooPortiaModRuinDiving
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
            UpdateStuff();
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnUpdate = OnUpdate;
            // modEntry.OnUnload = OnUnload;

            var harmony = new Harmony(modEntry.Info.Id);

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

    }

    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        [Header("Ruin Diving")]

        [Draw("Dig Radius Multiplier (Size of hole)")] public float DigRadius = 1.0F;
        [Draw("Dig Intensity Multiplier (amount of resources generated.)")] public float DigIntensity = 1.0F;
        [Draw("Dig Items Per Hit Cap (Prevents excessive dirt/resources pickup.)")] public int DigItemPerHitCap = 16;
        [Draw("Longer Jetpack")] public bool ExtendJetpack = false;
        [Draw("Treasure Detection Range")] public float DigTreasureDetectRange = 50F;
        [Draw("Treasure Lock-On Time(seconds)")] public float DigTreasureLockOnTime = 0.5F;
        [Draw("Treasure Lock-On Count")] public int DigTreasureLockOnCount = 1024;
        [Draw("Show Object Outline")] public bool DigShowTargetOutline = false;
        [Draw("Show Object Name")] public bool DigShowTargetName = false;
        [Draw("Long Distance Loot")] public bool DigLongDistanceLoot = false;

        [Header("Dungeons")]
        [Draw("Game Time Used Per Dungeon")] public int GameTimeUsedPerDungeon = 15;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {

        }
    }

    // Pathea.VoxelAimNs.VoxelTarget
    [HarmonyPatch(typeof(Pathea.VoxelAimNs.VoxelTarget))]
    static class Pathea_VoxelAimNs_VoxelTarget_Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch("ApplyDigs")]
        static void PrefixApplyDigs(ref Vector3 origin, ref Vector3 dir, ref float intensity, ref float radius)
        {


            radius *= Main.modSettings.DigRadius;
            intensity *= Main.modSettings.DigIntensity;
        }

        [HarmonyPrefix]
        [HarmonyPatch("DropItem")]
        static void PrefixDropItem(ref int itemDropId, ref float dmg)
        {

            if (dmg > (Main.modSettings.DigItemPerHitCap * 600F))
            {
                dmg = (Main.modSettings.DigItemPerHitCap * 600F);
            }
        }
    }


    // Pathea.EquipmentNs.JetPack
    [HarmonyPatch(typeof(Pathea.EquipmentNs.JetPack))]
    static class Pathea_EquipmentNs_JetPack_Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch("Fire")]
        public static void PostfixFire(ref float ___durationValue)
        {
            if (Main.modSettings.ExtendJetpack)
            {
                ___durationValue = float.MaxValue;
            }
        }
    }


    // Pathea.TreasureRevealerNs.TreasureRevealerManager
    [HarmonyPatch(typeof(Pathea.TreasureRevealerNs.TreasureRevealerManager))]
    class Pathea_TreasureRevealerNs_TreasureRevealerManager_Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch("property", MethodType.Getter)]
        static void PostfixProperty(ref Pathea.TreasureRevealerNs.TreasureRevealerManager.TreasureRevealerProperty __result)
        {
            __result.detectRange = Main.modSettings.DigTreasureDetectRange;
            __result.showBorder = Main.modSettings.DigShowTargetOutline;
            __result.showName = Main.modSettings.DigShowTargetName;
            __result.maxAimCount = Main.modSettings.DigTreasureLockOnCount;
        }

    }

    // Pathea.UISystemNs.TreasureRevealerUICtr
    [HarmonyPatch(typeof(Pathea.UISystemNs.TreasureRevealerUICtr))]
    class Pathea_UISystemNs_TreasureRevealerUICtr_Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnAim")]
        static void PostfixOnAim(ref List<Pathea.TreasureRevealerNs.TreasureRevealerItem> ___allTreasure, ref float ___lockTime)
        {
            ___lockTime = Main.modSettings.DigTreasureLockOnTime;
        }

        static List<Pathea.TreasureRevealerNs.TreasureRevealerItem> revealedTreasure = new List<Pathea.TreasureRevealerNs.TreasureRevealerItem>();

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        static void PostfixUpdate(
            ref Pathea.UISystemNs.TreasureRevealerUICtr __instance,
            ref Pathea.TreasureRevealerNs.TreasureRevealerItem ___curLock
        )
        {
            try
            {
                // keeptrack of last fresh lock
                // maybe transpiler later.
                if (
                    null != ___curLock
                    && !revealedTreasure.Contains(___curLock)
                    && null != ___curLock.transf.GetComponent<Pathea.ItemDropNs.ItemDrop>()
                    )
                {
                    revealedTreasure.Add(___curLock);
                }

                if (Main.modSettings.DigLongDistanceLoot && Pathea.InputSolutionNs.PlayerActionModule.Self.IsAcionPressed(ActionType.ActionInteract))
                {
                    Pathea.TreasureRevealerNs.TreasureRevealerItem result = treasureInCrosshair();
                    if (
                        null != result
                        )
                    {
                        //Main.mod.Logger.Log(string.Format("lastFreshLock {0}", result.GetInstanceID()));
                        //result.transf.position = Player.Self.GamePos;
                        // move to player and let autopickup do it's thing.
                        Pathea.ItemDropNs.ItemDrop itm = result.transf.GetComponent<Pathea.ItemDropNs.ItemDrop>();
                        itm.transform.position = Player.Self.GamePos;
                        revealedTreasure.Remove(result);
                    }
                }

            }
            catch (Exception e)
            {
                Main.mod.Logger.LogException(e);
            }

        }

        // determines if within scanner crosshair. code copied from Pathea.TreasureRevealerNs.TreasureRevealerItem::FreshLock()
        static Pathea.TreasureRevealerNs.TreasureRevealerItem treasureInCrosshair()
        {
            try
            {
                // Main.mod.Logger.Log(string.Format("treasure revealed count {0}", revealedTreasure.Count));
                if (revealedTreasure == null || revealedTreasure.Count == 0)
                {
                    return null;
                }
                Pathea.TreasureRevealerNs.TreasureRevealerItem result = null;
                float num = 25f;
                // Main.mod.Logger.Log(string.Format("treasure count {0}", revealedTreasure.Count));
                for (int i = 0; i < revealedTreasure.Count; i++)
                {
                    if (!(revealedTreasure[i] == null))
                    {
                        Vector3 v = Pathea.UISystemNs.UIUtils.WorldToScreenWithZ(revealedTreasure[i].pos, Vector2.zero);
                        //Main.mod.Logger.Log(string.Format("vector v {0}",v.ToString()));
                        Vector2 screenCoords = Pathea.UISystemNs.UIStateComm.Instance.canvas.GetComponent<RectTransform>().sizeDelta / 2f;
                        //Main.mod.Logger.Log(string.Format("vector vector {0}", vector.ToString()));
                        bool inscreen = v.x <= screenCoords.x && v.x >= -screenCoords.x && v.y <= screenCoords.y && v.y >= -screenCoords.y;
                        // Main.mod.Logger.Log(string.Format("inscreen {0}", inscreen));
                        if (inscreen && v.z >= 0f)
                        {
                            float num2 = Vector2.Distance(Vector2.zero, new Vector2(v.x, v.y));
                            //Main.mod.Logger.Log(string.Format("distance {0}", num2));
                            if (num2 < num)
                            {
                                result = revealedTreasure[i];
                                break;
                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                Main.mod.Logger.Error("altFreshLock");
                Main.mod.Logger.LogException(e);
            }
            return null;
        }

    }

    //Pathea.DungeonModuleNs.TrialRandomDungeonManager
    [HarmonyPatch(typeof(Pathea.DungeonModuleNs.TrialRandomDungeonManager))]
    class Pathea_DungeonModuleNs_TrialRandomDungeonManager_Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch("GetNeedTime")]
        static void PostfixGetNeedTime(ref Pathea.DungeonModuleNs.TrialRandomDungeonManager __instance, ref int __result, string dungeonScene, int difficulty)
        {
            __result = Main.modSettings.GameTimeUsedPerDungeon;
        }
    }
}
