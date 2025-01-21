using System;
using UnityEngine;
using MelonLoader;
using HarmonyLib;
using MyOWOTactsuit;

[assembly: MelonInfo(typeof(OWO_Wands.OWO_Wands), "OWO_Wands", "0.0.1", "OWO Game")]
[assembly: MelonGame("Cortopia Studios", "Wands")]

namespace OWO_Wands
{
    public class OWO_Wands : MelonMod
    {
        public static OWOSkin owoSkin;

        public override void OnInitializeMelon()
        {
            owoSkin = new OWOSkin();
            owoSkin.LOG("Initialize - SENSATION: HeartBeat");

            owoSkin.Feel("HeartBeat", 0);
        }

        #region Teleport and Health

        [HarmonyPatch(typeof(Cortopia.Scripts.Player.PlayerTeleportHandler), "Teleport", new Type[] { })]
        public class OWO_PlayerTeleport
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.LOG("Teleport");

                owoSkin.Feel("TeleportThrough", 3);
            }
        }

        [HarmonyPatch(typeof(Cortopia.Scripts.Player.PlayerControl), "UpdateHealth", new Type[] { typeof(float) })]
        public class OWO_UpdateHealth
        {
            [HarmonyPostfix]
            public static void Postfix(Cortopia.Scripts.Player.PlayerControl __instance, float changeValue)
            {
                owoSkin.LOG("UpdateHealth");


                if (changeValue > 0f)
                {
                    if (!owoSkin.IsPlaying("Healing")) owoSkin.Feel("Healing", 3);
                }
                if (__instance.Health == 0f) { owoSkin.StopHeartBeat(); return; }
                if (__instance.Health <= 25f) owoSkin.StartHeartBeat();
                else owoSkin.StopHeartBeat();
            }
        }

        [HarmonyPatch(typeof(Cortopia.Scripts.Player.PlayerControl), "UpdateMana", new Type[] { typeof(float) })]
        public class OWO_UpdateMana
        {
            [HarmonyPostfix]
            public static void Postfix(Cortopia.Scripts.Player.PlayerControl __instance, float changeValue)
            {
                owoSkin.LOG("UpdateMana");

                if (changeValue > 0f)
                {
                    if (!owoSkin.IsPlaying("Healing")) owoSkin.Feel("Healing", 0);
                }
            }
        }

        [HarmonyPatch(typeof(Cortopia.Scripts.Player.PlayerControl), "OnMatchEnded", new Type[] { typeof(CortopiaEvents.Events.MatchEndedEvent) })]
        public class OWO_MatchEnded
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.StopAllHapticFeedback();
            }
        }

        [HarmonyPatch(typeof(Cortopia.Scripts.Player.PlayerControl), "OnMatchReset", new Type[] { typeof(CortopiaEvents.Events.MatchResetEvent) })]
        public class OWO_MatchReset
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.StopAllHapticFeedback();
            }
        }

        [HarmonyPatch(typeof(Cortopia.Scripts.Player.PlayerControl), "OnPlayerDisconnected", new Type[] { typeof(int) })]
        public class OWO_PlayerDisconnected
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                owoSkin.StopAllHapticFeedback();
            }
        }

        #endregion

        #region Damage and casting

        [HarmonyPatch(typeof(Cortopia.Scripts.Player.PlayerControl), "TakeDamage", new Type[] { typeof(Assets.Scripts.Enums.DamageType), typeof(float), typeof(Vector2) })]
        public class OWO_TakeDamage
        {
            [HarmonyPostfix]
            public static void Postfix(Cortopia.Scripts.Player.PlayerControl __instance, Assets.Scripts.Enums.DamageType damageType, float damage, Vector2 hitDirection)
            {                           
                owoSkin.Feel("Impact", 5);
            }
        }

        [HarmonyPatch(typeof(Cortopia.Scripts.Wand.WandManager), "SpellRelease", new Type[] { typeof(Assets.Scripts.Enums.WandHand), typeof(int) })]
        public class OWO_SpellRelease
        {
            [HarmonyPostfix]
            public static void Postfix(Cortopia.Scripts.Wand.WandManager __instance, Assets.Scripts.Enums.WandHand wandHand, int spellSlotIndex)
            {
                bool isRightHand = false;
                if (wandHand == Assets.Scripts.Enums.WandHand.Right) isRightHand = true;

                string postfix = "_L";
                if (isRightHand) { postfix = "_R"; }
                string spell = $"SpellFire{postfix}";

                owoSkin.Feel(spell, 2);
            }
        }
        #endregion

    }
}
