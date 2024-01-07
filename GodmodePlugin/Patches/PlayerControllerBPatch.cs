using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.InputSystem;
using HarmonyLib;
using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using Unity.Netcode;
using Unity.Networking;
using UnityEngine.Animations;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPlugin.Patches // TODO: grabObjectAnimationTime
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {

        // All these variables are set in Plugin.cs
        public static ConfigFile config;


        public static ConfigEntry<float> healthAmount;
        public static ConfigEntry<float> jumpForce;
        public static ConfigEntry<float> runSpeed;
        public static ConfigEntry<float> walkSpeed;
        public static ConfigEntry<bool> shouldEnableInvincibility;
        public static ConfigEntry<bool> shouldOmitFallDamage;
        public static ConfigEntry<bool> shouldEnableNoclip;
        public static ConfigEntry<bool> shouldEnableMoonjump;
        public static ConfigEntry<bool> shouldEnableFloat;
        public static ConfigEntry<bool> shouldEnableInfiniteStamina;


        static bool isNoclipEnabled = false;
        static int n = 0; // read noclip
        static int layersToBeIncluded = 0;
        static IEnumerator JumpWait()
        {
            yield return new WaitForSeconds(0.25f);
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void JumpForcePatch(ref float ___jumpForce)
        {
            ___jumpForce = jumpForce.Value;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void InvincibilityPatch(ref float ___jumpForce)
        {
            StartOfRound.Instance.allowLocalPlayerDeath = !(shouldEnableInvincibility.Value);
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void SpeedPatch(ref bool ___isSprinting, ref float ___sprintMultiplier)
        {
            if (___isSprinting)
            {
                ___sprintMultiplier = runSpeed.Value;
            }
            else
            {
                ___sprintMultiplier = walkSpeed.Value;
            }
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void InfiniteSprintPatch(ref float ___sprintMeter)
        {
            if (!shouldEnableInfiniteStamina.Value) { return; }
            ___sprintMeter = 1.0f;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void MoonJumpPatch(ref float ___playerSlidingTimer, ref bool ___isJumping)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame && shouldEnableMoonjump.Value)
            {
                PlayerControllerB playerController = StartOfRound.Instance.localPlayerController;
                // ___playerSlidingTimer = 0.0f; // idk why we need this i just copy pasted it
                ___isJumping = true;
                playerController.StartCoroutine("PlayerJump");
            }
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void NoclipPatch(ref Collider ___playerCollider)
        {
            if (layersToBeIncluded == 0)
            {
                layersToBeIncluded = ___playerCollider.includeLayers;
            }

            if (!shouldEnableNoclip.Value) { return; }

            if (Keyboard.current.nKey.wasPressedThisFrame)
            {
                if (n == 0)
                {
                    n = 30;
                    isNoclipEnabled = !isNoclipEnabled;
                }
                // waits for 30 frames before it allows you to activate noclip again. this is needed because the BRILLIANT unity execs decided
                // that the function SPECIFICALLY MADE for checking if a key is held down during the current frame actually does NOT just check
                // one frame but rather MULTIPLE because of some context bullshit. THIS IS INTENDED BEHAVIOUR. I AM KILLING THE UNITY DEVS
            }
            if (n != 0) { n -= 1; }

            if (isNoclipEnabled)
            {
                ___playerCollider.excludeLayers = (LayerMask)Physics.AllLayers;
            }

            else
            {
                ___playerCollider.excludeLayers = ~((LayerMask)Physics.AllLayers);
            }
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void FallDamagePatch(ref bool ___takingFallDamage)
        {
            if (!shouldOmitFallDamage.Value) { return; }
            ___takingFallDamage = false;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void FloatPatch(ref float ___fallValue, ref float ___fallValueUncapped, ref bool ___isFallingFromJump, ref bool ___isFallingNoJump)
        {
            if (!shouldEnableFloat.Value || !Keyboard.current.spaceKey.isPressed || !(___isFallingFromJump || ___isFallingNoJump)) { return; }
            // ___fallValue = 0.0f;
            // ___fallValueUncapped = 0.0f;
        }
    }
}