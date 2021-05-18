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
            //UnityEngine.Debug.logger.logEnabled =false;
            Debug.unityLogger.logEnabled = modSettings.UnityDebugLogEnabled;
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
            if (Input.GetKeyDown(Main.modSettings.DebugKey.keyCode))
            {
                UpdateStuff();
                dmp("Pathea.Player.Self.actor.IsGround", Pathea.Player.Self.actor.IsGround());
                dmp(" Pathea.Player.Self.actor.gamePos", Pathea.Player.Self.actor.gamePos.ToString());

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
                //                global::UnityEngine.GameObject gameObject = global::UnityEngine.GameObject.Find("Root/Terrain/Land");
                //                if (gameObject != null)
                //                {
                //                    mainTerrain = gameObject.GetComponent<global::UnityEngine.Terrain>();
                //                }
                //global::UnityEngine.GameObject gameObject2 = global::UnityEngine.GameObject.Find("Root/Terrain/SeaPlane_Splite");
                //if (gameObject2 != null)
                //{
                //    dump("Water terrain: " + gameObject2.name);
                //    waterTerrain = gameObject2.GetComponent<Terrain>();

                //    TerrainCollider col = waterTerrain.gameObject.GetComponent<TerrainCollider>();
                //    if (null != col)
                //    {
                //        dmp("TerrainCollider", col.ToString());
                //        col.enabled = true;
                //        Mesh mesh = col.GetComponentInChildren<MeshFilter>()?.mesh;
                //        Renderer rend = col.GetComponentInChildren<Renderer>();
                //        if (null != mesh)
                //        {
                //            dmp("TerrainCollider Mesh", mesh.ToString());
                //        }

                //        if (null != rend)
                //        {
                //            dmp("TerrainCollider Renderer", rend.ToString());
                //            rend.material.color = Color.green;
                //        }
                //    }
                //    //MeshCollider col2 = gameObject2.AddComponent<MeshCollider>();



                //    //col2.enabled = true;
                //}

                //                if (mainTerrain != null && waterTerrain != null && terrainRigidBody!=null) {

                //                    Main.dump("have terrains");
                //                    waterTerrain.coll
                //Rigidbody waterRigidBody = waterTerrain.gameObject.AddComponent<Rigidbody>(); // Add the rigidbody.

                //                    waterRigidBody.mass = 5; // Set the GO's mass to 5 via the Rigidbody.
                //                }



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
        [Draw("Player Defence Boost (Adds to player base defense. Easily makes you near impossible to kill.)")] public float PlayerDefenceBoost = 100.0F;
        [Draw("Player Critical Chance Multiplier")] public float PlayerCriticalChanceMultiplier = 1.0F;
        [Draw("Player Critical Amount Multiplier")] public float PlayerCriticalAmountMultiplier = 1.0F;

        [Draw("Vampire Attack (Health on Hit)")] public float HealthLeech = 10.0F;
        [Draw("Vampire Attack (Stamina on Hit)")] public float StaminaLeech = 10.0F;

        [Draw("Regen Health")] public float HealthPerSecond = 1.0F;
        [Draw("Regen Stamina")] public float StaminaPerSecond = 1.0F;

        [Header("Missions")]
        [Draw("Research Speed Multiplier (Higher means faster.)")] public float ResearchSpeed = 1.0F;
        [Draw("Player Mission Reward Multiplier(Higher means more rewards)")] public float PlayerMissionRewardMultiplier = 1.0F;

        [Header("Resources")]
        [Draw("Days To Regrow Tree (default=5)")] public int TreeRegrowDays = Pathea.ConfigNs.OtherConfig.Self.TreeFreshDate;

        [Header("Relationships")]
        [Draw("Date mood bonus per kill in Ghost Cave event. (Other skill bonuses may apply if non-zero.")] public int GhostGameMoodBonusPerKill = 0;
        [Draw("Unlimited Talk Favour Gain")] public bool UnlimitedTalkFavourGain = false;
        [Draw("Talk Favour Gain Bonus")] public int TalkFavourGainBonus = 0;

        [Header("Misc/Useless/Not Working")]
        [Draw("Ignore Water (Attempt to walk on water.)")] public bool IgnoreWater = false;
        [Draw("Blood Effect Percent")] public float BloodEffectPercent = Pathea.ConfigNs.OtherConfig.Self.BloodEffect_Percent;
        [Draw("Player Mission Max Count")] public int PlayerMissionMaxCount = Pathea.ConfigNs.OtherConfig.Self.PlayerMissionMaxCount;

        [Draw("Debug Key, to trigger random things.")] public UnityModManagerNet.KeyBinding DebugKey = new UnityModManagerNet.KeyBinding { keyCode = KeyCode.V };
        [Draw("Unity Debug Log Enabled (output_log.txt).")] public bool UnityDebugLogEnabled = false;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {

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

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        static void PostfixUpdate(ref Pathea.ActorNs.Actor __instance)
        {

            if (Input.GetKeyDown(Main.modSettings.DebugKey.keyCode))
            {



            }

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

    [HarmonyPatch(typeof(Pathea.FavorSystemNs.FavorBehavior_Dialog))]
    class Pathea_FavorSystemNs_FavorBehavior_Dialog_Patches
    {

        [HarmonyPostfix]
        [HarmonyPatch("Pathea.FavorSystemNs.IFavorBehavior.CanExecute")]
        static void PostfixCanExecute(ref bool __result)
        {
            if (Main.modSettings.UnlimitedTalkFavourGain)
                __result = true;
        }

    }

    [HarmonyPatch(typeof(Pathea.FeatureNs.FeatureModule))]
    class Pathea_FeatureNs_FeatureModule_Patches
    {

        [HarmonyPostfix]
        [HarmonyPatch("ModifyFloat")]
        static void PostfixModifyFloat(ref float __result, Pathea.FeatureNs.FeatureType type)
        {
            switch (type) {
                case Pathea.FeatureNs.FeatureType.TalkFavor:
                    __result += Main.modSettings.TalkFavourGainBonus;

                    break;
            }
             
        }

    }
}

