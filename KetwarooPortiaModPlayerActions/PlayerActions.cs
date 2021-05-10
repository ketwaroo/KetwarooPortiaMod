using UnityEngine;
using UnityModManagerNet;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System;

namespace KetwarooPortiaModPlayerActions
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

            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }

        static void UpdateStuff()
        {
            AnimHelper.UpdateFromSettings(modSettings);
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
        [Header("Other")]
        [Draw("Tickable Equipment Cooldown")] public float TickCooldown = 0.2F;
        [Draw("Global Player Anim speed")] public float GlobalPlayerAnimSpeed = 1.0F;
        [Draw("Global NPC Anim speed")] public float GlobalNPCAnimSpeed = 1.0F;

        [Header("Animation Speed for each action.")]
        [Draw("Attack")] public float AnimAttackSpeedMult = 1.0F;
        [Draw("Kick Tree")] public float AnimKickTreeSpeedMult = 1.0F;
        [Draw("Pickaxe")] public float AnimPickaxeSpeedMult = 1.0F;
        [Draw("Axe")] public float AnimAxeSpeedMult = 1.0F;
        [Draw("Sow")] public float AnimSowSpeedMult = 1.0F;
        [Draw("Put Down")] public float AnimPutDownSpeedMult = 1.0F;
        [Draw("Drilling")] public float AnimDrillingSpeedMult = 1.0F;
        [Draw("Chainsaw")] public float AnimChainsawSpeedMult = 1.0F;
        [Draw("Petting Animals")] public float AnimPetSpeedMult = 1.0F;

        [Header("Running")]
        [Draw("Run Speed Multiplier", Precision = 2, Min = 0.01F)] public float RunMultiplier = 1.0F;

        [Header("Jumping")]
        [Draw("Jump Initial Speed Multiplier")] public float JumpInitialSpeed = 1.0F;

        [Header("Misc")]
        [Draw("Drill/Chainsaw Forward Drift Speed (AutoMove to toggle).")] public float DrillDriftSpeed = 1.5F;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {
            TickCooldown = Mathf.Clamp(TickCooldown, 0.01F, 999F);
            GlobalPlayerAnimSpeed = Mathf.Clamp(GlobalPlayerAnimSpeed, 0.01F, 999F);
            GlobalNPCAnimSpeed = Mathf.Clamp(GlobalNPCAnimSpeed, 0.01F, 999F);
        }
    }

    // dumb structure
    static class AnimHelper
    {
        public static Dictionary<string, float> newPlayerAnimSpeeds = new Dictionary<string, float>();
        public static int currentPlayerInteractionTarget;
        // safe to assume it's always 1.0F?
        //public static Dictionary<string, float> originalPlayerAnimSpeeds = new Dictionary<string, float>();
        public static UnityEngine.Animator playerAnimator;
        public static Pathea.ACT.ACTMgr playerActionManager;
        public static float currentPCSpeed = Main.modSettings.GlobalNPCAnimSpeed;
        public static float currentNPCSpeed;

        public static void SetPlayerAnimSpeed(float speed)
        {
            // set speed in actor update.
            //animator.speed = speed;
            currentPCSpeed = speed;
        }
        public static void clearPlayerAnimSpeed()
        {

            if (null != playerAnimator)
            {
                // set speed in actor update.
                //animator.speed = Main.modSettings.GlobalPlayerAnimSpeed;
                currentPCSpeed = Main.modSettings.GlobalPlayerAnimSpeed;
            }
        }

        public static void UpdateFromSettings(Settings modSettings)
        {
            // atack anim names.
            newPlayerAnimSpeeds["Attack"] = modSettings.AnimAttackSpeedMult;
            newPlayerAnimSpeeds["Drilling"] = modSettings.AnimDrillingSpeedMult;
            newPlayerAnimSpeeds["Chainsaw"] = modSettings.AnimChainsawSpeedMult;
            newPlayerAnimSpeeds["Sow"] = modSettings.AnimSowSpeedMult;
            newPlayerAnimSpeeds["Pickaxe"] = modSettings.AnimPickaxeSpeedMult;
            newPlayerAnimSpeeds["Axe"] = modSettings.AnimAxeSpeedMult;
            newPlayerAnimSpeeds["KickTree"] = modSettings.AnimKickTreeSpeedMult;
            newPlayerAnimSpeeds["Pet"] = modSettings.AnimPetSpeedMult;
            newPlayerAnimSpeeds["PutDown"] = modSettings.AnimPutDownSpeedMult;

            // may cause weirdness if changed midanimation.
            currentPCSpeed = Main.modSettings.GlobalPlayerAnimSpeed;
            currentNPCSpeed = Main.modSettings.GlobalNPCAnimSpeed;
        }

        public static bool IsPlayer(Pathea.ActorNs.Actor actor)
        {

            if (null != actor && actor.IsActorType(Pathea.ActorNs.ActorTag.Player))
            {
                return true;
            }

            return false;
        }

        public static void OnPlayerInteractiveStart(global::Pathea.ActorNs.Actor actor, global::Pathea.ActorNs.InteractType type)
        {
            Main.dump("event start " + actor.name + " " + type.ToString());
        }

        public static void OnPlayerInteractiveSuccess(global::Pathea.ActorNs.Actor actor, global::Pathea.ActorNs.InteractType type)
        {
            Main.dump("event success " + actor.name + " " + type.ToString());
        }

        public static void setNPCSpeed(Pathea.ActorNs.Actor actor)
        {
            // cache npc animators maybe?
            Animator npcAnimator = actor.animCtrl.GetComponentInChildren<Animator>();
            if (currentPlayerInteractionTarget == actor.GetInstanceID())
            {
                // Main.dump(currentPlayerInteractionTarget);
                // Main.dump("interacting with "+actor.name);
                npcAnimator.speed = currentPCSpeed;
            }
            else
            {
                npcAnimator.speed = currentNPCSpeed;
            }
        }

        public static float GetSpeedForAnimName(string animName)
        {

            string mainType = "";

            switch (animName)
            {
                // whatever. this work.
                // anim names mostly from Skills.json

                // all attack skills.
                case "Attack0":
                case "Attack1":
                case "Attack2":
                case "Attack3":
                case "Attack4":
                case "Attack5":
                case "Attack6":
                case "Attack7":
                case "Attack9":
                case "AttackGlove_1":
                case "AttackGlove_2":
                case "AttackGlove_3":
                case "AttackHammer_1":
                case "AttackHammer_2":
                case "AttackHammer_3":
                case "AttackStick_1":
                case "AttackStick_2":
                case "AttackStick_3":
                case "AttackStick_4":
                case "Gun":
                case "HandsAttack_1":
                case "HandsAttack_2":
                case "HandsAttack_3":
                case "Shoot":
                case "SMG_GunStart_Fire":
                    mainType = "Attack";
                    break;

                case "Drilling":
                    mainType = "Drilling";
                    break;
                case "Chainsaw":
                case "Chainsaw_Cut":
                    mainType = "Chainsaw";
                    break;
                case "Cutstone":
                case "Cutstone_Cut":
                    mainType = "Pickaxe";
                    break;
                case "CutTree":
                case "Cuttree_Cut":
                    mainType = "Axe";
                    break;
                // picking up stuff
                case "Sow":
                    mainType = "Sow";
                    break;

                case "Kicktree":
                case "Hold_KickTree":
                    mainType = "KickTree";
                    break;

                case "Touch_Animal_Low":
                case "Touch_Animal_Normal":
                case "Touch_Animal_High":
                case "Horse_Touch":
                case "Petting":
                case "Embrace_Cat":
                case "Embrace_Dog":
                    mainType = "Pet";
                    break;

                case "Throw_1":
                case "Throw_2":
                case "PutDown":
                    mainType = "PutDown";
                    break;
                // ignore for now.
                case "AttackTouchAnimal":
                case "FireStarter_Fire":
                case "Hit":
                case "Hit1":
                case "Hit2":
                case "Hit3":
                case "Hit4":
                case "Hit5":
                case "Hit6":
                case "Hit7":
                case "Hit8":
                case "Hit9":
                case "jian2-1":
                case "jian2-2":
                case "jian2-3":
                case "KnockDown":
                case "Roar":
                case "XiChengQi":

                    break;

                default:

                    break;
            }

            if ("" != mainType && newPlayerAnimSpeeds.ContainsKey(mainType))
            {
                return newPlayerAnimSpeeds[mainType];
            }

            return Main.modSettings.GlobalPlayerAnimSpeed;
        }
    }

    [HarmonyPatch(typeof(Pathea.ActorNs.Actor))]
    static class Pathea_ActorNs_Actor_Patches
    {

        [HarmonyPostfix]
        [HarmonyPatch("SetBool")]
        static void PostfixSetBool(Pathea.ActorNs.Actor __instance, string str, bool val)
        {


            if (!AnimHelper.IsPlayer(__instance))
            {
                return;
            }

            // Main.dump("Pathea.ActorNs.Actor::SetBool "+__instance.name+" " + str);

            if (!val)
            {
                AnimHelper.clearPlayerAnimSpeed();
                // release player from lingering anims.
                Pathea.MessageSystem.MessageManager.Instance.Dispatch("InteractAnimEnd");
                return;
            }

            AnimHelper.SetPlayerAnimSpeed(AnimHelper.GetSpeedForAnimName(str));

        }

        [HarmonyPostfix]
        [HarmonyPatch("InitActionRunner")]
        static void PostfixInitActionRunner(Pathea.ActorNs.Actor __instance, Pathea.ActorNs.ActorTag ___actorTag, Pathea.ACT.ACTMgr ___mActMgr)
        {
            if (___actorTag == Pathea.ActorNs.ActorTag.Player && null != ___mActMgr)
            {
                //Main.dump("got player action manager");
                AnimHelper.playerActionManager = ___mActMgr;
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        static void PostfixUpdate(Pathea.ActorNs.Actor __instance)
        {
            if (AnimHelper.IsPlayer(__instance))
            {
                AnimHelper.playerAnimator.speed = AnimHelper.currentPCSpeed;
            }
            // set npc speeds
            else
            {
                AnimHelper.setNPCSpeed(__instance);

            }
        }
    }


    [HarmonyPatch(typeof(Pathea.Player))]
    static class Pathea_Player_Patches
    {
        static float[] originalPlayerMotionValues = new float[3];

        [HarmonyPostfix]
        [HarmonyPatch("InitNew")]
        static void PostfixInitNew(Pathea.Player __instance, Pathea.ActorNs.Actor ___playingActor)
        {

            if (___playingActor == null)
                return;

            // meh
            if (originalPlayerMotionValues[0] == 0.0F) { originalPlayerMotionValues[0] = ___playingActor.motor.maxSpeed; }
            if (originalPlayerMotionValues[1] == 0.0F) { originalPlayerMotionValues[1] = ___playingActor.RunSpeed; }
            if (originalPlayerMotionValues[2] == 0.0F) { originalPlayerMotionValues[2] = ___playingActor.FastRunSpeed; }

            // add event handlers to match interaction targets
            __instance.OnInteractiveStart += AnimHelper.OnPlayerInteractiveStart;
            __instance.OnInteractiveSuccess += AnimHelper.OnPlayerInteractiveSuccess;


        }
        //  whenever player updates
        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        static void PostfixUpdate(Pathea.Player __instance, Pathea.ActorNs.Actor ___playingActor, Pathea.ActorNs.Actor ___itTarget)
        {
            if (___playingActor == null)
                return;

            if (Time.fixedTime - Main.timeTick >= 1.0F)
            {
                Main.timeTick = Time.fixedTime;

                // stuff to do every second
            }

            ___playingActor.motor.maxSpeed = originalPlayerMotionValues[0] * Main.modSettings.RunMultiplier;
            ___playingActor.RunSpeed = originalPlayerMotionValues[1] * Main.modSettings.RunMultiplier;
            ___playingActor.FastRunSpeed = originalPlayerMotionValues[2] * Main.modSettings.RunMultiplier;

            if (null != ___playingActor.animCtrl && null == AnimHelper.playerAnimator)
            {
                //Main.dump("has player animator 2");
                AnimHelper.playerAnimator = ___playingActor.animCtrl.GetComponentInChildren<UnityEngine.Animator>();
            }

            if (__instance.IsInteractiveRunning())
            {

                //Main.dump(__instance.interactiveID);
                if (null != ___itTarget)
                {
                    AnimHelper.currentPlayerInteractionTarget = ___itTarget.GetInstanceID();
                    //Main.dump("Interacting " + ___itTarget.name);
                    //AnimHelper.setNPCSpeed(___itTarget);
                }
            }
            else
            {
                AnimHelper.currentPlayerInteractionTarget = 0;
            }

            if (
                Main.modSettings.DrillDriftSpeed > 0.0F
                && __instance.IsAutoMove()
                && (
                  __instance.IsActionRunning(Pathea.ACT.ACType.Chainsaw)
                   || __instance.IsActionRunning(Pathea.ACT.ACType.Drilling)
                  )
                )
            {
                Vector3 direction = Main.modSettings.DrillDriftSpeed * Pathea.CameraSystemNs.CameraManager.Instance.SourceCamera.transform.forward;
                ___playingActor.motor.MoveBySpeed(direction);
            }

        }
    }

    [HarmonyPatch(typeof(Pathea.SkillNs.SkillRunner))]
    static class Pathea_SkillNs_SkillRunner_Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch("RunSkill")]
        [HarmonyPatch(new Type[] { typeof(Pathea.SkillNs.Skill), typeof(Pathea.SkillNs.Caster), typeof(Pathea.SkillNs.Target) })]
        static void PrefixRunSkill(ref Pathea.SkillNs.SkillRunner __instance, ref Pathea.SkillNs.Skill skill, ref Pathea.SkillNs.Caster caster, ref Pathea.SkillNs.Target receiver)
        {
            if (!System.Object.ReferenceEquals(caster.Castable, Pathea.Player.Self.actor))
            {
                return;
            }


            float animSpeed = -1.0F;
            string detectedAnimation = "";

            skill.ForEachCmd(delegate (Pathea.SkillNs.Cmd cmd)
            {

                if (animSpeed > 0.0F) { return; }

                if (cmd is Pathea.SkillNs.AnimationCmd)
                {

                    // stop at first animation cmd.
                    detectedAnimation = (string)Traverse.Create(cmd).Field("mAnimation").GetValue();
                    animSpeed = AnimHelper.GetSpeedForAnimName(detectedAnimation);
                }

            });

            // Main.dump(detectedAnimation);
            // Main.dump(animSpeed);

            if (animSpeed < 0.0F)
            {
                animSpeed = Main.modSettings.GlobalPlayerAnimSpeed;
            }
            // Main.dump(detectedAnimation);
            // Main.dump(animSpeed);

            Traverse.Create(skill).Field("totaltime").SetValue(skill.TotalTime / animSpeed);
            Traverse.Create(skill).Field("nextSkillTime").SetValue(skill.NextSkillTime / animSpeed);
            Traverse.Create(skill).Field("cdTime").SetValue(skill.CdTime / animSpeed);

        }
    }

    //[HarmonyPatch(typeof(Pathea.ACT.ACTAnimation))]
    //static class Pathea_ACT_ACTAnimation_Patches
    //{

    //    [HarmonyPrefix]
    //    [HarmonyPatch("PlayAnimation")]
    //    static void PrefixPlayAnimation(ref Pathea.ACT.ACTAnimation __instance, string name)
    //    {

    //        Main.dump("Pathea.ACT.ACTAnimation::PlayAnimation " + name);
    //    }

    //} 

    [HarmonyPatch(typeof(Pathea.ActorMotor))]
    static class Pathea_ActorMotor_Patches
    {

        [HarmonyPostfix]
        [HarmonyPatch("JumpStart")]
        static void PostfixOnStart(ref Pathea.ActorMotor __instance, ref Pathea.ActorNs.Actor ___actor, ref UnityEngine.Vector3 ___jumpSpeed)
        {
            if (!__instance.IsPlayer)
            {
                return;
            }
            ___jumpSpeed *= Main.modSettings.JumpInitialSpeed;
        }

    }


    //Pathea.EquipmentNs.TickableEquipment
    [HarmonyPatch(typeof(Pathea.EquipmentNs.TickableEquipment))]
    static class Pathea_EquipmentNs_TickableEquipment_Patches
    {



        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        static void PrefixUpdate(Pathea.Player __instance, ref Pathea.TimeCd ___tickCd)
        {
            if (___tickCd.maxTime != Main.modSettings.TickCooldown)
            {
                ___tickCd = new Pathea.TimeCd(Main.modSettings.TickCooldown);


            }
        }
    }

}
