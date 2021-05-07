using UnityEngine;
using UnityModManagerNet;
using HarmonyLib;
using System.Reflection;
using Pathea;
using Pathea.ACT;
using Pathea.ActorNs;
using System.Collections.Generic;
using System.Linq;

namespace KetwarooPortiaModActorRescale
{
    public class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry mod;
        public static Settings modSettings;
        public static Vector3 resetVector = Vector3.one;
        public static Vector3 headScaleVector;
        public static Vector3 actorScaleVector;
        public static Vector3 playerScaleVector;
        public static Vector3 hipScaleVector;
        public static Vector3 hipOffsetVector;
        public static Dictionary<string, Vector3> headScaleVectorByTag = new Dictionary<string, Vector3>() {
            { "Player_Male",Vector3.one },
            { "Player_Female",Vector3.one },
            { "Npc_Thin",Vector3.one },
            { "Npc_Medium",Vector3.one },
            { "Npc_Tall",Vector3.one },
            { "Npc_Strong",Vector3.one },
            { "Npc_Fat",Vector3.one },
            { "Npc_Little",Vector3.one },
            { "Npc_Baby",Vector3.one },

        };

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            modSettings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            UpdateStuff();
            mod = modEntry;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnUpdate = OnUpdate;

            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
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

        public static void UpdateStuff()
        {
            // save some cpu cycles
            modSettings.ActorHeadScale = Mathf.Clamp(modSettings.ActorHeadScale, 0.1F, 99999F);

            modSettings.ActorScaleMultiplier = Mathf.Clamp(modSettings.ActorScaleMultiplier, 0.1F, 99999F);
            modSettings.ActorHeadScalePlayer_Male = Mathf.Clamp(modSettings.ActorHeadScalePlayer_Male, 0.1F, 99999F);
            modSettings.ActorHeadScalePlayer_Female = Mathf.Clamp(modSettings.ActorHeadScalePlayer_Female, 0.1F, 99999F);
            modSettings.ActorHeadScaleNpc_Baby = Mathf.Clamp(modSettings.ActorHeadScaleNpc_Baby, 0.1F, 99999F);
            modSettings.ActorHeadScaleNpc_Fat = Mathf.Clamp(modSettings.ActorHeadScaleNpc_Fat, 0.1F, 99999F);
            modSettings.ActorHeadScaleNpc_Little = Mathf.Clamp(modSettings.ActorHeadScaleNpc_Little, 0.1F, 99999F);
            modSettings.ActorHeadScaleNpc_Medium = Mathf.Clamp(modSettings.ActorHeadScaleNpc_Medium, 0.1F, 99999F);
            modSettings.ActorHeadScaleNpc_Strong = Mathf.Clamp(modSettings.ActorHeadScaleNpc_Strong, 0.1F, 99999F);
            modSettings.ActorHeadScaleNpc_Tall = Mathf.Clamp(modSettings.ActorHeadScaleNpc_Tall, 0.1F, 99999F);
            modSettings.ActorHeadScaleNpc_Thin = Mathf.Clamp(modSettings.ActorHeadScaleNpc_Thin, 0.1F, 99999F);


            modSettings.PlayerScaleMultiplier = Mathf.Clamp(modSettings.PlayerScaleMultiplier, 0.1F, 99999F);

            //modSettings.ActorFemaleHipScaleZ = Mathf.Clamp(modSettings.ActorFemaleHipScaleZ, 1.0F, 3.0F);
            //modSettings.ActorFemaleHipScaleY = Mathf.Clamp(modSettings.ActorFemaleHipScaleY, 1.0F, 3.0F);

            headScaleVector = Vector3.one * modSettings.ActorHeadScale;

            headScaleVectorByTag["Player_Male"] = Vector3.one * modSettings.ActorHeadScalePlayer_Male;
            headScaleVectorByTag["Player_Female"] = Vector3.one * modSettings.ActorHeadScalePlayer_Female;
            headScaleVectorByTag["Npc_Baby"] = Vector3.one * modSettings.ActorHeadScaleNpc_Baby;
            headScaleVectorByTag["Npc_Fat"] = Vector3.one * modSettings.ActorHeadScaleNpc_Fat;
            headScaleVectorByTag["Npc_Little"] = Vector3.one * modSettings.ActorHeadScaleNpc_Little;
            headScaleVectorByTag["Npc_Medium"] = Vector3.one * modSettings.ActorHeadScaleNpc_Medium;
            headScaleVectorByTag["Npc_Strong"] = Vector3.one * modSettings.ActorHeadScaleNpc_Strong;
            headScaleVectorByTag["Npc_Tall"] = Vector3.one * modSettings.ActorHeadScaleNpc_Tall;
            headScaleVectorByTag["Npc_Thin"] = Vector3.one * modSettings.ActorHeadScaleNpc_Thin;

            actorScaleVector = Vector3.one * modSettings.ActorScaleMultiplier;
            playerScaleVector = Vector3.one * modSettings.PlayerScaleMultiplier;
            // affect width mostly, z axis. add some depth?
            modSettings.ActorFemaleHipScale.x = Mathf.Clamp(modSettings.ActorFemaleHipScale.x, 0.5F, 2.5F);
            modSettings.ActorFemaleHipScale.y = Mathf.Clamp(modSettings.ActorFemaleHipScale.y, 0.9F, 2.5F);
            modSettings.ActorFemaleHipScale.z = Mathf.Clamp(modSettings.ActorFemaleHipScale.z, 0.9F, 2.5F);

            hipScaleVector = modSettings.ActorFemaleHipScale;
            //hipOffsetVector = new Vector3(0.0F, modSettings.ActorFemaleHipOffsetY, modSettings.ActorFemaleHipOffsetZ);
            //Pathea_ActorNs_Actor_Patches.femaleHipProcessed.Clear();
        }

    }

    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        [Header("Actor Scale")]
        [Draw("Head Size Default", Precision = 3, Min = 0.01F)] public float ActorHeadScale = 1.0F;
        [Draw("Head Size Player_Male", Precision = 3, Min = 0.01F)] public float ActorHeadScalePlayer_Male = 1.0F;
        [Draw("Head Size Player_Female", Precision = 3, Min = 0.01F)] public float ActorHeadScalePlayer_Female = 1.0F;
        [Draw("Head Size Npc_Thin", Precision = 3, Min = 0.01F)] public float ActorHeadScaleNpc_Thin = 1.0F;
        [Draw("Head Size Npc_Medium", Precision = 3, Min = 0.01F)] public float ActorHeadScaleNpc_Medium = 1.0F;
        [Draw("Head Size Npc_Tall", Precision = 3, Min = 0.01F)] public float ActorHeadScaleNpc_Tall = 1.0F;
        [Draw("Head Size Npc_Strong", Precision = 3, Min = 0.01F)] public float ActorHeadScaleNpc_Strong = 1.0F;
        [Draw("Head Size Npc_Fat", Precision = 3, Min = 0.01F)] public float ActorHeadScaleNpc_Fat = 1.0F;
        [Draw("Head Size Npc_Little", Precision = 3, Min = 0.01F)] public float ActorHeadScaleNpc_Little = 1.0F;
        [Draw("Head Size Npc_Baby", Precision = 3, Min = 0.01F)] public float ActorHeadScaleNpc_Baby = 1.0F;

        [Draw("Actor Scale", Precision = 3, Min = 0.01F)] public float ActorScaleMultiplier = 1.0F;
        [Draw("Player Scale Multiplier", Precision = 3, Min = 0.001F)] public float PlayerScaleMultiplier = 1.0F;

        [Draw("Actor Female Hip Scale")] public Vector3 ActorFemaleHipScale = Vector3.one;
        [Draw("Actor Female Hip Z (width) Scale", Precision = 3, Min = 1.0F, Max = 2.0F)] public float ActorFemaleHipScaleZ = 1.0F;
        [Draw("Actor Female Hip Y (thickness) Scale", Precision = 3, Min = 1.0F, Max = 2.0F)] public float ActorFemaleHipScaleY = 1.0F;

        [Draw("Actor Female Hip Scale Offset Y (not working)")] public float ActorFemaleHipOffsetY = 0.0F;
        [Draw("Actor Female Hip Scale Offset Z (not working)")] public float ActorFemaleHipOffsetZ = 0.0F;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {
            Main.UpdateStuff();
        }
    }

    // Pathea.ActorNs.Actor
    [HarmonyPatch(typeof(Pathea.ActorNs.Actor))]
    static class Pathea_ActorNs_Actor_Patches
    {
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
        [HarmonyPatch("ApplySubModel")]
        static void PostUpdateApplySubModel(ref Pathea.ActorNs.Actor __instance)
        {
            //femaleHipProcessed.Remove(__instance.GetInstanceID());
            rescaleActor(__instance);
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
        static void PostfixUpdateSubModel(ref Pathea.ActorNs.Actor __instance)
        {
            //rescaleActor(__instance);
        }

        //public static List<int> femaleHipProcessed = new List<int>();
        public static string[] skipFemaleHipMod = { "Pinky" };

        // rescale an actor if possible
        public static void rescaleActor(Pathea.ActorNs.Actor __instance)
        {

            if (__instance == null)
            {
                return;
            }


            if (
            __instance.IsActorType(ActorTag.Npc) || __instance.IsActorType(ActorTag.Player)

           )
            {
                Transform head = __instance.GetHead();
                if (null != head && head.localScale != Main.headScaleVector)
                {

                    // todo make preset.
                    string modelType = Pathea.ActorNs.ActorMgr.Self.GetActorInfo(__instance.TmpltId).modelType;
                    if (Main.headScaleVectorByTag.ContainsKey(modelType))
                    {
                        head.localScale = Main.headScaleVectorByTag[modelType];
                    }
                    else
                    {
                        head.localScale = Main.headScaleVector;
                    }
                }
            }

            if (null != __instance.modelRoot)
            {
                bool female = false;
                // reset scale when sleeping to prevent clipping.
                // might get weird.
                if (
                    __instance.IsSleeping()
                    || __instance.IsActionRunning(ACType.Sit)
                )
                {
                    __instance.modelRoot.transform.localScale = Main.resetVector;
                }
                else if (System.Object.ReferenceEquals(__instance, Player.Self.actor))
                {
                    __instance.modelRoot.transform.localScale = Main.playerScaleVector;
                    if (Player.Self.GetGender() == Gender.Female)
                    {
                        female = true;
                    }
                }
                else if (__instance.IsActorType(ActorTag.Npc))
                {
                    __instance.modelRoot.transform.localScale = Main.actorScaleVector;
                    if (Pathea.NpcRepositoryNs.NpcRepository.Self.GetNpcGender(__instance.InstanceId) == Gender.Female)
                    {
                        female = true;
                    }
                }

                // attempt to give wider hip.
                if (
                    female
                    && !skipFemaleHipMod.Contains(__instance.name)
                    && null != __instance.ikController
                    && null != __instance.ikController.ik
                    && (
                    __instance.ikController.ik.references.pelvis.localScale != Main.hipScaleVector
                    // || __instance.ikController.ik.references.pelvis.localPosition != Main.hipOffsetVector
                    )
                   )
                {
                    //Bip001 Pelvis

                    //IEnumerable<Transform> bipPelvis = __instance.GetComponents<Transform>().Where(b => b.name == "Bip001 Pelvis");
                    //foreach (Transform pelvisBone in bipPelvis)
                    //{
                    //    Main.mod.Logger.Log(string.Format("bipPelvis {0} {1}", __instance.name, pelvisBone.ToString()));

                    //    Transform[] children = pelvisBone.Cast<Transform>().ToArray();
                    //    pelvisBone.DetachChildren();
                    //    pelvisBone.localScale = Main.hipScaleVector;
                    //    pelvisBone.localPosition = Main.hipOffsetVector;

                    //    foreach (Transform child in children)
                    //    {
                    //        child.parent = pelvisBone;

                    //    }
                    //}

                    Transform pelvisBone = __instance.ikController.ik.references.pelvis;
                    if (null != pelvisBone)
                    {
                        Transform[] children = pelvisBone.Cast<Transform>().ToArray();

                        pelvisBone.DetachChildren();
                        pelvisBone.localScale = Main.hipScaleVector;
                        //pelvisBone.localPosition = Main.hipOffsetVector;

                        foreach (Transform child in children)
                        {
                            child.parent = pelvisBone;

                        }
                    }
                }
            }
        }

        // taken from http://answers.unity.com/answers/1527837/view.html
        public static void ScaleAround(Transform target, Vector3 pivot, Vector3 newScale)
        {
            Vector3 A = target.localPosition;
            Vector3 B = pivot;

            Vector3 C = A - B; // diff from object pivot to desired pivot/origin

            float RS = newScale.z / target.localScale.z; // relataive scale factor

            // calc final position post-scale
            Vector3 FP = B + C * RS;
            Main.mod.Logger.Log(string.Format("local {0}", A.ToString()));
            Main.mod.Logger.Log(string.Format("pivot {0}", pivot.ToString()));
            Main.mod.Logger.Log(string.Format("FP {0}", FP.ToString()));
            // finally, actually perform the scale/translation
            target.localScale = newScale;
            target.localPosition = FP;
        }

    }

}
