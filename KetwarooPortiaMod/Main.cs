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

namespace KetwarooPortiaMod
{
    public class Main
    {
        public static float timeTick = 0.0F;
        public static bool enabled;
        public static UnityModManager.ModEntry mod;
        public static Settings modSettings;
        public static Vector3 resetVector = new Vector3(1.0F, 1.0F, 1.0F);
        public static Vector3 headScaleVector;
        public static Vector3 actorScaleVector;
        public static Vector3 playerScaleVector;

        static bool Load(UnityModManager.ModEntry modEntry)
        {

            modSettings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            // save some cpu cycles
            headScaleVector = new Vector3(modSettings.ActorHeadScale, modSettings.ActorHeadScale, modSettings.ActorHeadScale);
            actorScaleVector = new Vector3(modSettings.ActorScaleMultiplier, modSettings.ActorScaleMultiplier, modSettings.ActorScaleMultiplier);
            playerScaleVector = new Vector3(modSettings.PlayerScaleMultiplier, modSettings.PlayerScaleMultiplier, modSettings.PlayerScaleMultiplier);

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
        [Header("Global")]
        [Draw("Head Size")] public float ActorHeadScale = 1.0F;
        [Draw("Actor Scale(best left alone)")] public float ActorScaleMultiplier = 1.0F;

        [Header("Player Modification")]

        [Draw("Player Scale Multiplier")] public float PlayerScaleMultiplier = 1.0F;

        [Draw("Player Stats Multiplier Hp/Sp/Atk/Def/etc..")] public float PlayerStatsMultiplier = 1.0F;
        [Draw("Player Defence Boost (additive)")] public float PlayerDefenceBoost = 100.0F;
        [Draw("Player Critical Chance Multiplier")] public float PlayerCriticalChanceMultiplier = 1.0F;
        [Draw("Player Critical Amount Multiplier")] public float PlayerCriticalAmountMultiplier = 1.0F;
        [Draw("Run Speed Multiplier")] public float RunMultiplier = 1.0F;


        [Draw("Vampire Attack (Health on Hit)")] public float HealthLeech = 10.0F;
        [Draw("Vampire Attack (Stamina on Hit)")] public float StaminaLeech = 10.0F;

        [Draw("Regen Health")] public float HealthPerSecond = 1.0F;
        [Draw("Regen Stamina")] public float StaminaPerSecond = 1.0F;


        [Header("Ruin Diving")]

        [Draw("Dig Intensity Multiplier (How much resources per hit)")] public float DigIntensity = 1.0F;
        [Draw("Dig Radius Multiplier (Size of hole)")] public float DigRadius = 1.0F;
        [Draw("Longer Jetpack")] public bool ExtendJetpack = false;
        [Draw("Treasure Detection Range")] public float DigTreasureDetectRange = 50F;
        [Draw("Treasure Lock-On Time(seconds)")] public float DigTreasureLockOnTime = 0.5F;
        [Draw("Treasure Lock-On Count")] public int DigTreasureLockOnCount = 1024;
        [Draw("Show Object Outline")] public bool DigShowTargetOutline = false;
        [Draw("Show Object Name")] public bool DigShowTargetName = false;
        [Draw("Long Distance Loot")] public bool DigLongDistanceLoot = false;

        [Header("Missions")]

        [Draw("Research Speed Multiplier")] public float ResearchSpeed = 1.0F;
        [Draw("Player Mission Reward Multiplier")] public float PlayerMissionRewardMultiplier = 1.0F;

        [Header("Crafting/Resources")]

        [Draw("Crafting Cost Multiplier (floors at 1 material cost.)")] public float CostCrafting = 1.0F;
        [Draw("Crafting Time Multiplier")] public float TimeCraftingMultiplier = 1.0F;


        [Header("Planter/Farming")]
        [Draw("Planter Harvest Amount Multiplier")] public float PlanterHarvestAmountMultiplier = 1.0F;
        [Draw("Planter Growth Speed Multiplier")] public float PlanterGrowthSpeedMultiplier = 1.0F;
        [Draw("Happy Plant Bonus Multiplier ")] public float PlanterHappySeedMultiplier = 1.0F;

        [Draw("Days To Regrow Tree")] public int TreeRegrowDays = Pathea.ConfigNs.OtherConfig.Self.TreeFreshDate;

        [Header("Misc/Useless/NotWorking")]

        [Draw("Blood Effect Percent")]
        public float BloodEffectPercent = Pathea.ConfigNs.OtherConfig.Self.BloodEffect_Percent;

        [Draw("Jump Time Scale")]
        public float JumpTimeScale = Pathea.ConfigNs.OtherConfig.Self.JumpTimeScale;

        [Draw("Player Mission Max Count")]
        public int PlayerMissionMaxCount = Pathea.ConfigNs.OtherConfig.Self.PlayerMissionMaxCount;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {
            Main.headScaleVector = new Vector3(ActorHeadScale, ActorHeadScale, ActorHeadScale);
            Main.actorScaleVector = new Vector3(ActorScaleMultiplier, ActorScaleMultiplier, ActorScaleMultiplier);
            Main.playerScaleVector = new Vector3(ActorScaleMultiplier, ActorScaleMultiplier, ActorScaleMultiplier);
        }
    }


    [HarmonyPatch(typeof(Pathea.Player))]
    static class Pathea_Player_Patches
    {

        [HarmonyPostfix]
        [HarmonyPatch("DigRadius", MethodType.Getter)]
        static void PostfixDigRadius(ref float __result)
        {
            if (!Main.enabled)
                return;

            try
            {
                __result *= Main.modSettings.DigRadius;
            }
            catch (Exception e)
            {
                Main.mod.Logger.Error(e.ToString());
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch("DigIntensity", MethodType.Getter)]
        static void PostfixDigIntensity(ref float __result)
        {
            if (!Main.enabled)
                return;

            try
            {
                __result *= Main.modSettings.DigIntensity;
            }
            catch (Exception e)
            {
                Main.mod.Logger.Error(e.ToString());
            }
        }

        //[HarmonyPostfix]
        //[HarmonyPatch("TriggerAction")]
        //static void PostfixTriggerAction(ActionType type, ActionTriggerMode mode, ref bool __result, Pathea.Player __instance, Intend ___intend)
        //{
        //}

        static float[] originalPlayerMotionValues = new float[3];

        [HarmonyPostfix]
        [HarmonyPatch("InitNew")]
        static void PostfixInitNew(Pathea.Player __instance, Actor ___playingActor)
        {

            if (___playingActor == null)
                return;

            // meh
            if (originalPlayerMotionValues[0] == 0.0F) { originalPlayerMotionValues[0] = __instance.actor.motor.maxSpeed; }
            if (originalPlayerMotionValues[1] == 0.0F) { originalPlayerMotionValues[1] = __instance.actor.RunSpeed; }
            if (originalPlayerMotionValues[2] == 0.0F) { originalPlayerMotionValues[2] = __instance.actor.FastRunSpeed; }

            __instance.actor.motor.maxSpeed = originalPlayerMotionValues[0] * Main.modSettings.RunMultiplier;
            __instance.actor.RunSpeed = originalPlayerMotionValues[1] * Main.modSettings.RunMultiplier;
            __instance.actor.FastRunSpeed = originalPlayerMotionValues[2] * Main.modSettings.RunMultiplier;

        }


        //  whenever player updates
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        static void PostfixUpdate(Player __instance, Actor ___playingActor)
        {
            if (___playingActor == null)
                return;

            if (Time.fixedTime - Main.timeTick >= 1.0F)
            {
                Main.timeTick = Time.fixedTime;

                // stuff to do every second
                __instance.ChangeHp(Main.modSettings.HealthPerSecond);
                __instance.ChangeStamina(Main.modSettings.StaminaPerSecond);
            }
        }

    }


    // Pathea.NotebookSystem.BlueprintResearchManager.Self.CurSpeed

    [HarmonyPatch(typeof(Pathea.NotebookSystem.BlueprintResearchManager))]
    static class Pathea_NotebookSystem_BlueprintResearchManager_Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch("CurSpeed", MethodType.Getter)]
        static void PostfixResearchSpeed(ref float __result)
        {
            if (!Main.enabled)
                return;

            try
            {
                __result *= Main.modSettings.ResearchSpeed;
            }
            catch (Exception e)
            {
                Main.mod.Logger.Error(e.ToString());
            }
        }

    }


    // Pathea.ActorNs.Actor
    [HarmonyPatch(typeof(Pathea.ActorNs.Actor))]
    static class Pathea_ActorNs_Actor_Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch("HpChangeEventTrigger")]
        static void PrefixHpChangeEventTrigger(Pathea.SkillNs.Caster caster, float hpChangeOrigin, float hpChanged, bool critical, Pathea.ActorNs.Actor __instance)
        {
            // if player did it.
            if (System.Object.ReferenceEquals(caster.Castable, Player.Self.actor))
            {
                Player.Self.ChangeHp(Main.modSettings.HealthLeech);
                Player.Self.ChangeStamina(Main.modSettings.StaminaLeech);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("ConstAttrAttack", MethodType.Getter)]
        static void PostfixConstAttrAttack(ref float __result, Pathea.ActorNs.Actor __instance)
        {
            if (!__instance.IsActorType(ActorTag.Player)) { return; }
            __result *= Main.modSettings.PlayerStatsMultiplier;
        }

        [HarmonyPostfix]
        [HarmonyPatch("ConstAttrCpMax", MethodType.Getter)]
        static void PostfixConstAttrCpMax(ref float __result, Pathea.ActorNs.Actor __instance)
        {
            if (!__instance.IsActorType(ActorTag.Player)) { return; }
            __result *= Main.modSettings.PlayerStatsMultiplier;
        }

        [HarmonyPostfix]
        [HarmonyPatch("ConstAttrCritical", MethodType.Getter)]
        static void PostfixConstAttrCritical(ref float __result, Pathea.ActorNs.Actor __instance)
        {
            if (!__instance.IsActorType(ActorTag.Player)) { return; }
            __result *= Main.modSettings.PlayerCriticalChanceMultiplier;
        }

        [HarmonyPostfix]
        [HarmonyPatch("ConstAttrDefence", MethodType.Getter)]
        static void PostfixConstAttrDefence(ref float __result, Pathea.ActorNs.Actor __instance)
        {
            if (!__instance.IsActorType(ActorTag.Player)) { return; }
            __result = Main.modSettings.PlayerDefenceBoost + __result * Main.modSettings.PlayerStatsMultiplier;
        }

        [HarmonyPostfix]
        [HarmonyPatch("ConstAttrHpMax", MethodType.Getter)]
        static void PostfixConstAttrHpMax(ref float __result, Pathea.ActorNs.Actor __instance)
        {
            if (!__instance.IsActorType(ActorTag.Player)) { return; }
            __result *= Main.modSettings.PlayerStatsMultiplier;
        }

        [HarmonyPostfix]
        [HarmonyPatch("ConstAttrMeleeCriticalAmount", MethodType.Getter)]
        static void PostfixConstAttrMeleeCriticalAmount(ref float __result, Pathea.ActorNs.Actor __instance)
        {
            if (!__instance.IsActorType(ActorTag.Player)) { return; }
            __result *= Main.modSettings.PlayerStatsMultiplier;
        }

        [HarmonyPostfix]
        [HarmonyPatch("ConstAttrRangeCriticalAmount", MethodType.Getter)]
        static void PostfixConstAttrRangeCriticalAmount(ref float __result, Pathea.ActorNs.Actor __instance)
        {
            if (!__instance.IsActorType(ActorTag.Player)) { return; }
            __result *= Main.modSettings.PlayerStatsMultiplier;
        }

        [HarmonyPostfix]
        [HarmonyPatch("ConstAttrVpMax", MethodType.Getter)]
        static void PostfixConstAttrVpMax(ref float __result, Pathea.ActorNs.Actor __instance)
        {
            if (!__instance.IsActorType(ActorTag.Player)) { return; }
            __result *= Main.modSettings.PlayerStatsMultiplier;
        }



        [HarmonyPrefix]
        [HarmonyPatch("ApplySubModel")]
        static void PrefixApplySubModel(int slot, string subModelInfo, ref Pathea.ActorNs.Actor __instance)
        {
            // just assume something's changed.
            // reset scaling so it gets picked up by update
            if (null != __instance.GetHead())
            {
                __instance.GetHead().localScale = Main.resetVector;
            }

            if (null != __instance.modelRoot)
            {
                __instance.modelRoot.transform.localScale = Main.resetVector;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        static void PostfixUpdate(ref Pathea.ActorNs.Actor __instance)
        {
            // todo OnUpdate might be too often.
            rescaleActor(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch("UpdateSubModel")]
        static void PostUpdateSubModel(ref Pathea.ActorNs.Actor __instance)
        {
            rescaleActor(__instance);
        }

        // rescale an actor if possible
        static void rescaleActor(Pathea.ActorNs.Actor __instance)
        {

            if (__instance == null)
            {
                return;
            }


            if (
            __instance.IsActorType(ActorTag.Npc) || __instance.IsActorType(ActorTag.Player)
            // todo, add exclusion for papa bear. maybe the mayor too.
           )
            {
                Transform head = __instance.GetHead();
                if (null != head)
                {
                    head.localScale = Main.headScaleVector;
                }
            }

            if (null != __instance.modelRoot)
            {
                if (__instance.IsActorType(ActorTag.Player))
                {
                    __instance.modelRoot.transform.localScale = Main.playerScaleVector;
                }
                else if (__instance.IsActorType(ActorTag.Npc))
                {
                    __instance.modelRoot.transform.localScale = Main.actorScaleVector;
                }
            }
        }
    }


    // Pathea.EquipmentNs.JetPack
    [HarmonyPatch(typeof(Pathea.EquipmentNs.JetPack))]
    static class Pathea_EquipmentNs_JetPack_Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        public static void PostfixEnableJetpack(ref float ___durationValue)
        {
            if (Main.modSettings.ExtendJetpack)
            {
                ___durationValue = float.MaxValue;
            }
        }
    }

    // Pathea.CompoundSystem.CompoundManager
    [HarmonyPatch(typeof(Pathea.CompoundSystem.CompoundItem))]
    static class Pathea_CompoundSystem_CompoundItem_Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch("CompoundTime", MethodType.Getter)]
        public static void PostfixCompoutTime(ref float __result)
        {
            __result *= Main.modSettings.TimeCraftingMultiplier;
        }
    }

    //AutomataMachineMenuCtr
    // for crafting stuff
    [HarmonyPatch(typeof(AutomataMachineMenuCtr))]
    static class AutomataMachineMenuCtr_Patches
    {
        static Dictionary<int, int[]> originalValues = new Dictionary<int, int[]>();

        [HarmonyPostfix]
        [HarmonyPatch("ChangePage")]
        public static void PostfixReduceRequireItemCost(List<Pathea.CompoundSystem.CompoundItem> ___itemList)
        {

            if (null == ___itemList)
                return;

            for (int i = 0; i < ___itemList.Count; i++)
            {
                try
                {
                    Pathea.CompoundSystem.CompoundItem item = ___itemList[i];


                    // keep track of original.
                    if (!originalValues.ContainsKey(item.Id))
                    {
                        int[] arr = new int[] {
                            (null==item.RequireItem1)?0:item.RequireItem1.Number,
                            (null==item.RequireItem2)?0:item.RequireItem2.Number,
                            (null==item.RequireItem3)?0:item.RequireItem3.Number

                        };
                        //Main.mod.Logger.Log(string.Format("Initial value for {0} : {1}", item.Id, arr.ToString()));
                        originalValues.Add(item.Id, arr);
                    }

                    int newnum1 = ChangeCraftingMaterialNumber(originalValues[item.Id][0]);
                    int newnum2 = ChangeCraftingMaterialNumber(originalValues[item.Id][1]);
                    int newnum3 = ChangeCraftingMaterialNumber(originalValues[item.Id][2]);

                    if (newnum1 > 0)
                    {
                        item.RequireItem1.DeleteNumber(item.RequireItem1.Number);
                        item.RequireItem1.ChangeNumber(newnum1);

                    }
                    if (newnum2 > 0)
                    {
                        item.RequireItem2.DeleteNumber(item.RequireItem2.Number);
                        item.RequireItem2.ChangeNumber(newnum2);

                    }
                    if (newnum3 > 0)
                    {
                        item.RequireItem3.DeleteNumber(item.RequireItem3.Number);
                        item.RequireItem3.ChangeNumber(newnum3);

                    }
                    // Main.mod.Logger.Log(string.Format("value for {0} : {1} {2} {3}", item.Id, newnum1, newnum2,newnum3));
                }
                catch (Exception e)
                {
                    Main.mod.Logger.Error(e.ToString());
                    Main.mod.Logger.Error(e.Source);
                    Main.mod.Logger.Error(e.StackTrace);

                }

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

    // Pathea.Missions.MissionRewards
    [HarmonyPatch(typeof(Pathea.Missions.MissionRewards))]
    static class Pathea_Missions_MissionRewards_Patches
    {

        [HarmonyPostfix]
        [HarmonyPatch("Money", MethodType.Getter)]
        static void PostfixMultipleMissionRewardMoney(ref int __result)
        {
            //PlayerMissionRewardMultiplier
            __result = Mathf.RoundToInt((float)__result * Main.modSettings.PlayerMissionRewardMultiplier);
        }

    }

    [HarmonyPatch(typeof(Pathea.ConfigNs.OtherConfig))]
    class Pathea_ConfigNs_OtherConfig_Patches
    {

        [HarmonyPostfix]
        [HarmonyPatch("TreeFreshDate", MethodType.Getter)]
        static void PostfixTreeFreshDate(ref int __result)
        {
            __result = Main.modSettings.TreeRegrowDays;
        }

        [HarmonyPostfix]
        [HarmonyPatch("BloodEffect_Percent", MethodType.Getter)]
        static void PostfixBloodEffect_Percent(ref float __result)
        {
            __result = Main.modSettings.BloodEffectPercent;
        }

        [HarmonyPostfix]
        [HarmonyPatch("JumpTimeScale", MethodType.Getter)]
        static void PostfixJumpTimeScale(ref float __result)
        {
            __result = Main.modSettings.JumpTimeScale;
        }

        [HarmonyPostfix]
        [HarmonyPatch("PlayerMissionMaxCount", MethodType.Getter)]
        static void PostfixPlayerMissionMaxCount(ref int __result)
        {
            __result = Main.modSettings.PlayerMissionMaxCount;
        }

    }


    //Pathea.ItemSystem.ItemSeedCmpt

    [HarmonyPatch(typeof(Pathea.ItemSystem.ItemSeedCmpt))]
    class Pathea_ItemSystem_ItemSeedCmpt_Patches
    {

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
            __result = Mathf.RoundToInt(__result * Main.modSettings.PlanterHappySeedMultiplier);
        }

        [HarmonyPostfix]
        [HarmonyPatch("GrowPerfectExtraDrop", MethodType.Getter)]
        static void PostfixGrowPerfectExtraDrop(ref int __result)
        {
            __result = Mathf.RoundToInt(__result * Main.modSettings.PlanterHappySeedMultiplier);
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
                Main.mod.Logger.Error(e.Message);
                Main.mod.Logger.Error(e.Source);
                Main.mod.Logger.Error(e.StackTrace);
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
                Main.mod.Logger.Error(e.Message);
                Main.mod.Logger.Error(e.Source);
                Main.mod.Logger.Error(e.StackTrace);
            }
            return null;
        }

    }
}

