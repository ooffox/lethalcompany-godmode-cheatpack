﻿using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPlugin
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class TestPluginBase : BaseUnityPlugin
    {
        public static ConfigFile config;
        private const string modGUID = "Ofoz.TestMod";
        private const string modName = "Test Mod for Lethal Company by Ofoz";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        private static TestPluginBase Instance;

        internal ManualLogSource mls;

        void Awake()
        {
            config = base.Config;
            Patches.PlayerControllerBPatch.config = config;
            // Patches.PlayerControllerBPatch.healthAmount = config.Bind("Death", "HealthAmount", 10.0f, "Whether or not noclip should be enabled (activated by tapping N).");
            Patches.PlayerControllerBPatch.jumpForce = config.Bind("Movement", "Jumpforce", 5.0f, "Force when you jump.");
            Patches.PlayerControllerBPatch.runSpeed = config.Bind("Movement", "RunSpeed", 2.25f, "Running speed.");
            Patches.PlayerControllerBPatch.walkSpeed = config.Bind("Movement", "WalkSpeed", 1.0f, "Walking speed.");
            Patches.PlayerControllerBPatch.shouldEnableMoonjump = config.Bind("Movement", "MoonJump", false, "Whether or not moonjump should be enabled.");
            Patches.PlayerControllerBPatch.shouldEnableInfiniteStamina = config.Bind("Movement", "InfiniteStamina", false, "Whether or not infinite stamina should be enabled.");
            Patches.PlayerControllerBPatch.shouldEnableInvincibility = config.Bind("Damage", "Invincibility", false, "Whether or not invincibility should be enabled.");
            Patches.PlayerControllerBPatch.shouldOmitFallDamage = config.Bind("Damage", "NoFallDamage", false, "Whether or not fall damage should be omitted.");
            Patches.PlayerControllerBPatch.shouldEnableNoclip = config.Bind("Collision", "Noclip", false, "Whether or not noclip should be enabled (activated by tapping N).");
            
            if (Instance == null)
            {
                Instance = this;
            }

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo("Awake");

            harmony.PatchAll(typeof(TestPluginBase));
            harmony.PatchAll(typeof(Patches.PlayerControllerBPatch));

            mls.LogInfo("Patched succesfully");
        }
    }
}
