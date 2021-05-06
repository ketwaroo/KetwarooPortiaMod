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


namespace KetwarooPortiaMod
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
            modEntry.OnUpdate = OnUpdate;
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
            if (Input.GetKeyDown(Main.modSettings.DebugKey.keyCode))
            {
                UpdateStuff();
                dmp("Pathea.Player.Self.actor.IsGround", Pathea.Player.Self.actor.IsGround());

                //Terrain[] ts = UnityEngine.GameObject.FindObjectsOfType<Terrain>();

                //foreach (Terrain t in ts)
                //{

                //    dump("Terrain: " + t.name);
                //    TerrainCollider r = t.GetComponent<TerrainCollider>();

                //    if (r != null)
                //    {

                //        Main.dump(r.enabled);
                //        Main.dump(r.contactOffset);

                //        r.enabled = true;
                //        r.contactOffset = 1.0f;
                //        if (null != r.material)
                //        {
                //            Main.dump(r.material.ToString());
                //            Main.dump(r.material.dynamicFriction);
                //            r.material.bounciness = 0.5F;
                //        }

                //        if (null != r.attachedRigidbody)
                //        {
                //            Main.dump(r.attachedRigidbody.ToString());
                //            Main.dump(r.attachedRigidbody.collisionDetectionMode.ToString());
                //        }
                //    }




                //}
                ////                Terrain mainTerrain =null;

                //Terrain waterTerrain = null;
                ////                global::UnityEngine.GameObject gameObject = global::UnityEngine.GameObject.Find("Root/Terrain/Land");
                ////                if (gameObject != null)
                ////                {
                ////                    mainTerrain = gameObject.GetComponent<global::UnityEngine.Terrain>();
                ////                }
                //global::UnityEngine.GameObject gameObject2 = global::UnityEngine.GameObject.Find("Root/Terrain/SeaPlane_Splite");
                //if (gameObject2 != null)
                //{
                //    dump("Water terrain: " + gameObject2.name);
                //    waterTerrain = gameObject2.GetComponent<Terrain>();
                //    MeshCollider col = waterTerrain.gameObject.AddComponent<MeshCollider>();
                //    MeshCollider col2 = gameObject2.AddComponent<MeshCollider>();
                //    col.enabled = true;
                //    col2.enabled = true;
                //}

                ////                if (mainTerrain != null && waterTerrain != null && terrainRigidBody!=null) {

                ////                    Main.dump("have terrains");
                ////                    waterTerrain.coll
                ////Rigidbody waterRigidBody = waterTerrain.gameObject.AddComponent<Rigidbody>(); // Add the rigidbody.

                ////                    waterRigidBody.mass = 5; // Set the GO's mass to 5 via the Rigidbody.
                ////                }



            }
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


        public static void inGameNotify(string message) {

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

        public static void dumpComponents(GameObject o)
        {
            if (o == null) { return; }
            Component[] components = o.GetComponents(typeof(Component));
            for (int i = 0; i < components.Length; i++)
            {
                dump(string.Format("component {1} {2}", components[i].name, components[i].ToString()));
            }
        }

        public static void dumpBoneNames(Transform parent, int level = 0)
        {
            if (parent == null)
            {
                return;
            }
            dump(string.Format("{0}: {1}", level, parent.name));
            IEnumerator enumerator = parent.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    object obj = enumerator.Current;
                    Transform transform = (Transform)obj;

                    dump(string.Format("{0}: {1}", level, transform.name));

                    dumpBoneNames(transform, level + 1);
                }
            }
            catch (Exception e)
            {
                mod.Logger.LogException(e);
            }

        }

    }



    public class Settings : UnityModManager.ModSettings, IDrawable
    {

        [Header("Player Modification")]

        [Draw("Player Stats Multiplier Hp/Sp/Atk/Def/etc..", Precision = 2, Min = 0.1F)] public float PlayerStatsMultiplier = 1.0F;
        [Draw("Player Defence Boost", Min = 0)] public float PlayerDefenceBoost = 100.0F;
        [Draw("Player Critical Chance Multiplier")] public float PlayerCriticalChanceMultiplier = 1.0F;
        [Draw("Player Critical Amount Multiplier")] public float PlayerCriticalAmountMultiplier = 1.0F;

        [Draw("Vampire Attack (Health on Hit)")] public float HealthLeech = 10.0F;
        [Draw("Vampire Attack (Stamina on Hit)")] public float StaminaLeech = 10.0F;

        [Draw("Regen Health")] public float HealthPerSecond = 1.0F;
        [Draw("Regen Stamina")] public float StaminaPerSecond = 1.0F;

        [Header("Equipment")]
        [Draw("Drill/Chainsaw Drift Speed (AutoMove to toggle).")] public float DrillDriftSpeed = 1.5F;

        [Header("Missions")]

        [Draw("Research Speed Multiplier")] public float ResearchSpeed = 1.0F;
        [Draw("Player Mission Reward Multiplier")] public float PlayerMissionRewardMultiplier = 1.0F;

        [Header("Crafting/Resources")]

        [Draw("Crafting Cost Multiplier (floors at 1 material cost.)")] public float CostCrafting = 1.0F;
        [Draw("Crafting Time Multiplier")] public float TimeCraftingMultiplier = 1.0F;

        [Draw("Factory Performance/Transport Level Cap")] public int FactoryPerformanceLevelCap = 999;
        [Draw("Factory Performance/Transport Gain Per Level")] public float FactoryPerformanceLevelGain = 0.05F;

        [Draw("Connect Factory Resources For Crafting")] public bool FactoryConnectCraftingResources = true;

        [Header("Planter/Farming")]
        [Draw("Farm Animal Max Growth Multiplier (default ~1.2)", Min = 1,Precision =2)] public float FarmAnimalMaxGrowth = 1.2F;
        [Draw("Planter Harvest Amount Multiplier")] public float PlanterHarvestAmountMultiplier = 1.0F;
        [Draw("Planter Growth Speed Multiplier")] public float PlanterGrowthSpeedMultiplier = 1.0F;
        [Draw("Happy Plant Bonus Multiplier (fertilizer required multiplier)")] public float PlanterHappySeedMultiplier = 1.0F;
        [Draw("Days To Regrow Tree")] public int TreeRegrowDays = Pathea.ConfigNs.OtherConfig.Self.TreeFreshDate;

        [Header("Relationships")]
        [Draw("Date mood bonus per kill in Ghost Cave event.")] public int GhostGameMoodBonusPerKill = 0;


        [Header("Misc/Useless/Not Working")]
        [Draw("Ignore Water")] public bool IgnoreWater = false;
        [Draw("Blood Effect Percent")] public float BloodEffectPercent = Pathea.ConfigNs.OtherConfig.Self.BloodEffect_Percent;
        [Draw("Player Mission Max Count")] public int PlayerMissionMaxCount = Pathea.ConfigNs.OtherConfig.Self.PlayerMissionMaxCount;

        [Draw("Debug Key, to trigger random things.")] public UnityModManagerNet.KeyBinding DebugKey = new UnityModManagerNet.KeyBinding { keyCode = KeyCode.V };

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {
            FarmAnimalMaxGrowth = Mathf.Clamp(FarmAnimalMaxGrowth,1.0F, 99F);
        }
    }

    [HarmonyPatch(typeof(Pathea.Player))]
    static class Pathea_Player_Patches
    {

        // when moving to safe position
        [HarmonyPrefix]
        [HarmonyPatch("BeginCorrect")]
        static bool PrefixBeginCorrect(Player __instance, Actor ___playingActor, UnityEngine.Vector3 pos)
        {

            // prevent position correction if in water. or in weird places.
            if (
                Main.modSettings.IgnoreWater
                // as long as alive
                && ___playingActor.hp > 0
                // and not in a bottomless pit
                //&& ___playingActor.IsGround()
                )
            {
                return false;
            }

            return true;
        }




        //  whenever player updates
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        static void PostfixUpdate(Player __instance, Actor ___playingActor)
        {
            if (___playingActor == null)
                return;


            if (Time.fixedTime - Main.timeTick >= 1.0F
                // and not dead.
                && ___playingActor.hp > 0)
            {
                Main.timeTick = Time.fixedTime;

                // stuff to do every second
                __instance.ChangeHp(Main.modSettings.HealthPerSecond);
                __instance.ChangeStamina(Main.modSettings.StaminaPerSecond);
            }

            if (
                Main.modSettings.DrillDriftSpeed > 0.0F
                && __instance.IsAutoMove()
                && (
                  __instance.IsActionRunning(ACType.Chainsaw)
                   || __instance.IsActionRunning(ACType.Drilling)
                  )
                )
            {
                Vector3 direction = Main.modSettings.DrillDriftSpeed * Pathea.CameraSystemNs.CameraManager.Instance.SourceCamera.transform.forward;
                ___playingActor.motor.MoveBySpeed(direction);
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
            if (!Main.enabled)
                return;

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
            if (!Main.enabled)
                return;

            if (!__instance.IsActorType(ActorTag.Player)) { return; }
            __result *= Main.modSettings.PlayerStatsMultiplier;
        }

        [HarmonyPostfix]
        [HarmonyPatch("ConstAttrCpMax", MethodType.Getter)]
        static void PostfixConstAttrCpMax(ref float __result, Pathea.ActorNs.Actor __instance)
        {
            if (!Main.enabled)
                return;

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
            __result *= Main.modSettings.PlayerCriticalAmountMultiplier;
        }

        [HarmonyPostfix]
        [HarmonyPatch("ConstAttrRangeCriticalAmount", MethodType.Getter)]
        static void PostfixConstAttrRangeCriticalAmount(ref float __result, Pathea.ActorNs.Actor __instance)
        {
            if (!__instance.IsActorType(ActorTag.Player)) { return; }
            __result *= Main.modSettings.PlayerCriticalAmountMultiplier;
        }

        [HarmonyPostfix]
        [HarmonyPatch("ConstAttrVpMax", MethodType.Getter)]
        static void PostfixConstAttrVpMax(ref float __result, Pathea.ActorNs.Actor __instance)
        {
            if (!__instance.IsActorType(ActorTag.Player)) { return; }
            __result *= Main.modSettings.PlayerStatsMultiplier;
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDeath")]
        static void PostfixOnDeath(ref Pathea.ActorNs.Actor __instance)
        {

            if (
                0 != Main.modSettings.GhostGameMoodBonusPerKill
                && __instance.FactionId == GhostCaveCenter.ghostFactionId
                && GhostCaveCenter.Instance.Game != null
                && Pathea.EG.EGMgr.Self.IsEngagementEvent()
                )
            {
                Main.mod.Logger.Log("Killed ghost on playdate");
                Pathea.EG.EGMgr.Self.AddMood(Main.modSettings.GhostGameMoodBonusPerKill);
                //GhostCaveCenter.Instance.Game.ChangeMood(Main.modSettings.GhostGameMoodBonusPerKill);
            }

        }
        //IgnoreCollision

        [HarmonyPrefix]
        [HarmonyPatch("IgnoreCollision")]
        [HarmonyPatch(new Type[] { typeof(UnityEngine.Collider), typeof(bool) })]
        static void PrefixIgnoreCollision(ref Pathea.ActorNs.Actor __instance, ref UnityEngine.Collider coll, ref bool ignore)
        {
            if (__instance.IsActorType(ActorTag.Player) && null != coll)
            {
                Main.dump(coll.name);
                Main.dump(ignore);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        static void PostfixUpdate(ref Pathea.ActorNs.Actor __instance)
        {
            
            //if (__instance.IsActorType(ActorTag.Player)/* && Input.GetKeyDown(Main.modSettings.DebugKey.keyCode)*/)
            //{


            //    // Main.dump("actor collider");
            //    Ray ray = default;
            //    RaycastHit[] hits = new RaycastHit[2];

            //    ray.origin = __instance.gamePos;
            //    ray.direction = Vector3.down;


            //    int num = global::UnityEngine.Physics.RaycastNonAlloc(ray, hits, __instance.GetActorHeight(), 16);
            //    for (int i = 0; i < num; i++)
            //    {
            //        RaycastHit raycastHit = hits[i];
            //        //Main.dump(raycastHit.ToString());
            //        //Main.dmp("raycastHit.distance", raycastHit.distance);
            //        //Main.dmp("raycastHit.collider.gameObject.layer", raycastHit.collider.gameObject.layer);
            //        //Main.dmp("raycastHit.collider.gameObject.name" , raycastHit.collider.gameObject.name);
            //        //Main.dump(raycastHit.collider.ToString());

            //        if ("SeaPlane_Splite" == raycastHit.collider.gameObject.name)
            //        {

            //            Vector3 pos = __instance.gamePos;
            //            pos.y = ray.origin.y - raycastHit.distance;
            //            __instance.gamePos = pos;
            //        }

            //    }

            //    //RaycastHit hit2 = new RaycastHit();
            //    //__instance.GetCollider().Raycast(ray, out hit2, 10f);
            //    //if (null != hit2.collider)
            //    //{
            //    //    Main.dmp("hit2.distance ", hit2.distance);
            //    //    Main.dmp("hit2.collider.gameObject.name", hit2.collider.gameObject.name);
            //    //}
            //    //Main.dump("actor collider end");
            //}
            
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

    // to prevent circular references.
    static class FactoryMatInfo
    {
        public static Dictionary<int, Pathea.HomeNs.IItemContainer> FactoryItemContainers = new Dictionary<int, Pathea.HomeNs.IItemContainer>();

        public static void InitItemContainer(int factoryId)
        {

            if (FactoryItemContainers.ContainsKey(factoryId))
            {
                return;
            }

            Pathea.HomeNs.IItemContainer itemContainer = new FactorySharedContainer(factoryId);
            Pathea.HomeNs.HomeModule.Self.AddItemContainer(itemContainer);

            FactoryItemContainers.Add(factoryId, itemContainer);
        }

        public static int GetMatCount(Pathea.FarmFactoryNs.FarmFactory __instance, int ietmId)
        {
            if (__instance == null || __instance.MatList == null)
            {
                return 0;
            }

            foreach (IdCount idCount in __instance.MatList)
            {
                if (ietmId == idCount.id)
                {
                    return idCount.count;
                }
            }

            return 0;
        }

        public static void RemoveMat(Pathea.FarmFactoryNs.FarmFactory __instance, int id, int count)
        {
            __instance.MatList.Remove(id, count);
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
                FactoryMatInfo.InitItemContainer(id);
            }
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
        [HarmonyPatch("RemoveMat")]
        [HarmonyPatch(new Type[] { typeof(int), typeof(int) })]
        static bool PrefixRemoveMat(int id, ref int count, Pathea.FarmFactoryNs.FarmFactory __instance)
        {

            if (!Main.modSettings.FactoryConnectCraftingResources)
            {
                return true;
            }

            Pathea.ItemSystem.ItemPackage.RemoveItem(id, count, true, true);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RemoveMat")]
        [HarmonyPatch(new Type[] { typeof(List<IdCount>) })]
        static bool PrefixRemoveMatByList(List<IdCount> list, ref List<IdCount> __result, Pathea.FarmFactoryNs.FarmFactory __instance)
        {
            if (!Main.modSettings.FactoryConnectCraftingResources)
            {
                return true;
            }

            foreach (IdCount idCount in list)
            {
                int id = idCount.id;
                int count = idCount.count;
                int currentCount = Pathea.ItemSystem.ItemPackage.GetItemCount(id, true);

                if (currentCount > 0)
                {
                    Pathea.ItemSystem.ItemPackage.RemoveItem(id, count, true, true);
                    int num = Mathf.Min(currentCount, count);
                    idCount.count = Mathf.Max(0, idCount.count - num);
                }
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].count <= 0)
                {
                    list.RemoveAt(i);
                }
            }
            __result = list;

            return false;
        }
    }

    class FactorySharedContainer : Pathea.HomeNs.IItemContainer
    {

        public Pathea.FarmFactoryNs.FarmFactory factory;
        public int factoryId;
        public FactorySharedContainer(int factoryId)
        {
            this.factoryId = factoryId;
        }

        public int Delete(int itemId, int count)
        {
            getFactory();
            if (factory != null)
            {
                int matcount = FactoryMatInfo.GetMatCount(factory, itemId);

                if (matcount == 0)
                {
                    return 0;
                }

                int toremove = (matcount >= count) ? count : (count - matcount);

                FactoryMatInfo.RemoveMat(factory, itemId, count);

                return (count - toremove);

            }
            return 0;
        }

        public int GetCount(int itemId)
        {
            getFactory();

            if (factory != null)
            {
                int count = FactoryMatInfo.GetMatCount(factory, itemId);
                return count;
            }
            return 0;
        }

        public Pathea.FarmFactoryNs.FarmFactory getFactory()
        {

            if (factory == null)
            {
                factory = Pathea.FarmFactoryNs.FarmFactoryMgr.Self.GetFactory(factoryId);
            }
            return factory;
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
            //__result *= Main.modSettings.TimeCraftingMultiplier;
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
        [HarmonyPatch("SetDataInfo")]
        static void PostfixTotalPoint(ref Pathea.HomeNs.FarmAnimalCtr __instance, Pathea.AnimalFarmNs.AnimalinFarm ___anm)
        {
            float rescale = 1.0F;
            if (null!=___anm) {
                rescale = ___anm.ProductionMul;
            }
            __instance.transform.localScale = Vector3.one*rescale;
            __instance.gameObject.transform.localScale = Vector3.one * rescale;
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


}

