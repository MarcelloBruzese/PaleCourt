﻿using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using SFCore.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vasi;
using Random = UnityEngine.Random;

namespace FiveKnights
{
    public class PurityTimer : MonoBehaviour
    {
        private float timer = 0;
        private bool timerRunning = false;
        private float duration;
        private const float PURITY_DURATION_DEFAULT = 1f;
        private const float PURITY_DURATION_18 = 1.25f;
        private const float PURITY_DURATION_13 = 1.70f;
        private const float PURITY_DURATION_18_13 = 2.25f;
        private const float ATTACK_COOLDOWN_DEFAULT = .41f;
        private const float ATTACK_COOLDOWN_DEFAULT_32 = .25f;
        private const float ATTACK_COOLDOWN_44 = .55f;
        private const float ATTACK_COOLDOWN_44_32 = .34f;
        private const float ATTACK_DURATION_DEFAULT = .36f;
        private const float ATTACK_DURATION_DEFAULT_32 = .25f;
        private const float ATTACK_DURATION_44 = .50f;
        private const float ATTACK_DURATION_44_32 = .25f;
        private const float COOLDOWN_CAP_44 = .17f;
        private const float COOLDOWN_CAP_44_32 = .13f;
        private HeroController _hc = HeroController.instance;
        private List<NailSlash> nailSlashes;
        private void OnEnable()
        {
            nailSlashes = new List<NailSlash>()
            {
                HeroController.instance.normalSlash,
                HeroController.instance.alternateSlash,
                HeroController.instance.downSlash,
                HeroController.instance.upSlash,
                HeroController.instance.wallSlash,
            };
            On.HealthManager.TakeDamage += IncrementSpeed;
            ModHooks.CharmUpdateHook += SetDuration;
            On.NailSlash.SetLongnail += CancelLongnail;
            On.NailSlash.SetMantis += CancelMantis;

            _hc.ATTACK_COOLDOWN_TIME = ATTACK_COOLDOWN_44;
            _hc.ATTACK_COOLDOWN_TIME_CH = ATTACK_COOLDOWN_44_32;
            _hc.ATTACK_DURATION = ATTACK_DURATION_44;
            _hc.ATTACK_DURATION_CH = ATTACK_DURATION_44_32;
        }




        private void OnDisable()
        {
            On.HealthManager.TakeDamage -= IncrementSpeed;
            ModHooks.CharmUpdateHook -= SetDuration;
            On.NailSlash.SetMantis -= CancelMantis;
            On.NailSlash.SetLongnail -= CancelLongnail;

            _hc.ATTACK_COOLDOWN_TIME = ATTACK_COOLDOWN_DEFAULT;
            _hc.ATTACK_COOLDOWN_TIME_CH = ATTACK_COOLDOWN_DEFAULT_32;
            _hc.ATTACK_DURATION = ATTACK_DURATION_DEFAULT;
            _hc.ATTACK_DURATION_CH = ATTACK_DURATION_DEFAULT_32;
        }
        
        private void Update()
        {
            if (timerRunning)
            {
                timer += Time.deltaTime;
                if (timer >= duration)
                {
                    timerRunning = false;
                    _hc.ATTACK_COOLDOWN_TIME = ATTACK_COOLDOWN_44;
                    _hc.ATTACK_COOLDOWN_TIME_CH = ATTACK_COOLDOWN_44_32;
                    _hc.ATTACK_DURATION = ATTACK_DURATION_44;
                    _hc.ATTACK_DURATION_CH = ATTACK_DURATION_44_32;
                    timer = 0;
                }
            }
        }
        private void IncrementSpeed(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            orig(self, hitInstance);

            timer = 0;
            timerRunning = true;

            _hc.ATTACK_COOLDOWN_TIME -= .038f;
            _hc.ATTACK_COOLDOWN_TIME_CH -= .038f;
            _hc.ATTACK_DURATION -= .038f;
            _hc.ATTACK_DURATION_CH -= .038f;

            if (_hc.ATTACK_COOLDOWN_TIME <= COOLDOWN_CAP_44) { _hc.ATTACK_COOLDOWN_TIME = COOLDOWN_CAP_44; }
            if (_hc.ATTACK_COOLDOWN_TIME_CH <= COOLDOWN_CAP_44_32) { _hc.ATTACK_COOLDOWN_TIME_CH = COOLDOWN_CAP_44_32; }
            if (_hc.ATTACK_DURATION <= COOLDOWN_CAP_44) { _hc.ATTACK_DURATION = COOLDOWN_CAP_44; }
            if (_hc.ATTACK_DURATION_CH <= COOLDOWN_CAP_44_32) { _hc.ATTACK_DURATION_CH = COOLDOWN_CAP_44_32; }
        }

        private void SetDuration(PlayerData data, HeroController controller)
        {
            bool ln = data.equippedCharm_18;
            bool mop = data.equippedCharm_13;

            if (mop && ln) duration = PURITY_DURATION_18_13;
            else if (mop && !ln) duration = PURITY_DURATION_13;
            else if (ln && !mop) duration = PURITY_DURATION_18;
            else if (!ln && !mop) duration = PURITY_DURATION_DEFAULT;


            foreach (NailSlash nailSlash in nailSlashes)
            {
                if (nailSlash == null) break;
                switch (nailSlash.name)
                {
                    case "Slash":
                        nailSlash.SetLongnail(false);
                        nailSlash.SetMantis(false);
                        nailSlash.scale = new Vector3(1.62f, 1.645244f, 1.277858f);
                        break;
                    case "AltSlash":
                        nailSlash.SetLongnail(false);
                        nailSlash.SetMantis(false);
                        nailSlash.scale = new Vector3(1.257f, 1.4224f, 1.12369f);
                        break;
                    case "DownSlash":
                        nailSlash.SetLongnail(false);
                        nailSlash.SetMantis(false);
                        nailSlash.scale = new Vector3(1.125f, 1.28f, 1f);
                        break;
                    case "UpSlash":
                        nailSlash.SetLongnail(false);
                        nailSlash.SetMantis(false);
                        nailSlash.scale = new Vector3(1.15f, 1.4f, 1.2455f);
                        break;
                        nailSlash.SetLongnail(false);
                        nailSlash.SetMantis(false);
                    case "WallSlash":
                        nailSlash.SetLongnail(false);
                        nailSlash.SetMantis(false);
                        nailSlash.scale = new Vector3(-1.62f, 1.6452f, 1f);
                        break;
                }

            }

        }
        private void CancelMantis(On.NailSlash.orig_SetMantis orig, NailSlash self, bool set) { orig(self, false); }
        private void CancelLongnail(On.NailSlash.orig_SetLongnail orig, NailSlash self, bool set) { orig(self, false); }



        private void Log(object message) => Modding.Logger.Log("[FiveKnights][Purity Timer] " + message);
    }
    
}