﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Modding;
using System.Reflection;
using ModCommon.Util;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Collections;
using ModCommon;
using Object = System.Object;
using UnityEngine.UI;

namespace FiveKnights
{
    public class CustomWP : MonoBehaviour
    {
        private bool correctedTP;
        private bool isFromGodhome;
        public static Boss boss;
        public static CustomWP Instance;
        public bool wonLastFight;
        public int lev;
        public enum Boss { Ogrim, Dryya, Isma, Hegemol, All, None, Zemer };

        private IEnumerator Start()
        {
            Instance = this;
            On.GameManager.EnterHero += GameManager_EnterHero;
            On.GameManager.BeginSceneTransition += GameManager_BeginSceneTransition;
            On.BossChallengeUI.LoadBoss_int_bool += BossChallengeUI_LoadBoss_int_bool;
            ModHooks.Instance.TakeHealthHook += Instance_TakeHealthHook;
            boss = Boss.None;

            while (true)
            {
                yield return new WaitWhile(() => !Input.GetKeyUp(KeyCode.R));
                Log("PDATA " + PlayerData.instance.currentBossStatueCompletionKey);
                yield return null;
            }
        }

        private int Instance_TakeHealthHook(int damage)
        {
            return damage;
        }

        private void GameManager_BeginSceneTransition(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
        {
            if (info.SceneName == "White_Palace_09")
            {
                if (boss == Boss.Isma || boss == Boss.Ogrim)
                {
                    info.EntryGateName = "door_dreamReturnGGstatueStateIsma_GG_Statue_ElderHu(Clone)(Clone)";
                }
                else
                {
                    info.EntryGateName = "door_dreamReturnGGstatueState" + boss + "_GG_Statue_TraitorLord(Clone)(Clone)";
                }
            }

            if (self.sceneName == "GG_Workshop") isFromGodhome = true;
            else isFromGodhome = false;

            if (info.SceneName != "Dream_04_White_Defender" || correctedTP)
            {
                correctedTP = false;
                orig(self, info);
                return;
            }
            correctedTP = true; 
            ArenaFinder.defeats = PlayerData.instance.whiteDefenderDefeats;
            PlayerData.instance.whiteDefenderDefeats = 0;
            GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
            {
                SceneName = "Dream_04_White_Defender",
                EntryGateName = "door1",
                EntryDelay = 0,
                Visualization = GameManager.SceneLoadVisualizations.Default,
                WaitForSceneTransitionCameraFade = false,
                PreventCameraFadeOut = true
            });
        }

        private void GameManager_EnterHero(On.GameManager.orig_EnterHero orig, GameManager self, bool additiveGateSearch)
        {
            if (self.sceneName == "White_Palace_09")
            {
                CreateStatues();
                HubRemove();
                AddLift();
                CreateGateway("left test2", new Vector2(14f, 94.4f), new Vector2(1f, 4f),
                              "GG_Workshop", "left test", false, true, true, GameManager.SceneLoadVisualizations.Default);
                GameObject black = new GameObject("black_mask");
                SpriteRenderer sr = black.AddComponent<SpriteRenderer>();
                sr.sprite = FiveKnights.SPRITES[1];
                sr.material = new Material(Shader.Find("Sprites/Diffuse"));
                sr.material.renderQueue = 4000;
                black.transform.position = new Vector3(18.7f, 94.4f, -1000f);
                black.transform.localScale *= 100f;
                orig(self, false);
                SetupHub(black);
                return;
            }
            orig(self, false);
        }

        private void SetupHub(GameObject black)
        {
            IEnumerator HubSet(GameObject black)
            {

                GameObject go = Instantiate(FiveKnights.preloadedGO["Warp"]);
                GameObject go2 = Instantiate(FiveKnights.preloadedGO["WarpBase"]);
                GameObject go3 = Instantiate(FiveKnights.preloadedGO["WarpAnim"]);
                go.SetActive(true);
                go2.SetActive(true);
                go3.SetActive(true);
                go.transform.position = new Vector3(24.5f,94.4f) - new Vector3(0f, 0.7f, 0f);
                go3.transform.position = go2.transform.position = new Vector3(go.transform.position.x + 0.1f, go.transform.position.y - 0.4f, -0.5f);
                var fsm = go.LocateMyFSM("Door Control");
                fsm.GetAction<BeginSceneTransition>("Change Scene", 3).sceneName = "GG_Workshop";
                fsm.GetAction<BeginSceneTransition>("Change Scene", 3).entryGateName = "door_dreamReturnGG_GG_Statue_Defender";

                SpriteRenderer sr = black.GetComponent<SpriteRenderer>();
                yield return new WaitWhile(() => !HeroController.instance);
                if (isFromGodhome) HeroController.instance.transform.position = new Vector2(12.5f, 94.5f);
                SetDialogue();
                yield return new WaitForSeconds(1.9f);
                for (float i = 1f; i >= 0f; i -= 0.1f)
                {
                    Color col = sr.color;
                    sr.color = new Color(col.r, col.g, col.b, i);
                    yield return new WaitForSeconds(0.05f);
                }
                Destroy(black);
            }

            void SetDialogue()
            {
                PlayMakerFSM fsm = null;

                IEnumerator LookForDialogClosed()
                {
                    PlayMakerFSM textYN = GameObject.Find("Text YN").LocateMyFSM("Dialogue Page Control");
                    while (textYN.ActiveStateName != "Ready for Input") yield return new WaitForEndOfFrame();
                    while (textYN.ActiveStateName == "Ready for Input") yield return new WaitForEndOfFrame();
                    GameObject.Find("DialogueManager").LocateMyFSM("Box Open YN").SendEvent("BOX DOWN YN");
                    fsm.enabled = true;
                    yield return new WaitForSeconds(0.5f);
                    PlayMakerFSM pm = GameCameras.instance.tk2dCam.gameObject.LocateMyFSM("CameraFade");
                    pm.SendEvent("FADE OUT");
                    yield return new WaitForSeconds(0.5f);
                    boss = Boss.All;
                    GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
                    {
                        SceneName = "Dream_04_White_Defender",
                        EntryGateName = "door1",
                        Visualization = GameManager.SceneLoadVisualizations.Dream,
                        WaitForSceneTransitionCameraFade = false,

                    });
                }

                IEnumerator Wait()
                {
                    GameObject go = Instantiate(FiveKnights.preloadedGO["throne"]);
                    go.SetActive(true);
                    go.transform.position = new Vector3(60.5f, 97.7f, 0.2f);
                    PlayMakerFSM fsm = go.LocateMyFSM("Sit");
                    yield return new WaitWhile(() => fsm.ActiveStateName != "Resting");
                    fsm.enabled = false;
                    Begin();
                }

                void Begin()
                {
                    GameObject.Find("DialogueManager").LocateMyFSM("Box Open YN").SendEvent("BOX UP YN");
                    GameObject.Find("Text YN").GetComponent<DialogueBox>().StartConversation("YN_THRONE", "YN_THRONE");
                    GameObject.Find("Text YN").GetComponent<MonoBehaviour>().StartCoroutine(LookForDialogClosed());
                }

                StartCoroutine(Wait());
            }

            StartCoroutine(HubSet(black));
        }

        private void CreateGateway(string gateName, Vector2 pos, Vector2 size, string toScene, string entryGate,
                                  bool right, bool left, bool onlyOut, GameManager.SceneLoadVisualizations vis)
        {
            GameObject gate = new GameObject(gateName);
            gate.transform.SetPosition2D(pos);
            var tp = gate.AddComponent<TransitionPoint>();
            if (!onlyOut)
            {
                var bc = gate.AddComponent<BoxCollider2D>();
                bc.size = size;
                bc.isTrigger = true;
                tp.targetScene = toScene;
                tp.entryPoint = entryGate;
            }
            tp.alwaysEnterLeft = left;
            tp.alwaysEnterRight = right;
            GameObject rm = new GameObject("Hazard Respawn Marker");
            rm.transform.parent = tp.transform;
            rm.transform.position = new Vector2(rm.transform.position.x - 3f, rm.transform.position.y);
            var tmp = rm.AddComponent<HazardRespawnMarker>();
            tp.respawnMarker = rm.GetComponent<HazardRespawnMarker>();
            tp.sceneLoadVisualization = vis;
        }

        private void GetChildren(Transform trans, Action<Transform> myMethodName)
        {
            if (trans == null) return;
            foreach (Transform child in trans)
            {
                myMethodName(child);
                GetChildren(child, myMethodName);
            }
        }

        private void CreateStatues()
        {
            //48f 98.75f Hegemol Top Left
            GameObject stat = SetStatue(new Vector2(81.75f, 94.75f), new Vector2(-0.1f, 0.1f), new Vector2(0f,-0.5f), FiveKnights.preloadedGO["Statue"],
                                        "GG_White_Defender", FiveKnights.SPRITES[2], "ISMA_NAME", "ISMA_DESC", "statueStateIsma");
            GameObject stat2 = SetStatue(new Vector2(39.4f, 94.75f), new Vector2(-0.25f, -0.69f), new Vector2(-0f, -1f), FiveKnights.preloadedGO["StatueMed"],
                                        "GG_White_Defender", FiveKnights.SPRITES[3], "DRY_NAME", "DRY_DESC", "statueStateDryya");
            GameObject stat3 = SetStatue(new Vector2(73.3f, 98.75f), new Vector2(-0.13f, 2.03f), new Vector2(-0.3f, -0.8f), FiveKnights.preloadedGO["StatueMed"],
                                        "GG_White_Defender", FiveKnights.SPRITES[4], "ZEM_NAME", "ZEM_DESC", "statueStateZemer");
            GameObject stat4 = SetStatue(new Vector2(48f, 98.75f), new Vector2(-2f, 0.5f), new Vector2(-0.3f, -0.8f), FiveKnights.preloadedGO["StatueMed"],
                                        "GG_White_Defender", FiveKnights.SPRITES[5], "HEG_NAME", "HEG_DESC", "statueStateHegemol");
        }

        private void BossChallengeUI_LoadBoss_int_bool(On.BossChallengeUI.orig_LoadBoss_int_bool orig, BossChallengeUI self, int level, bool doHideAnim)
        {
            string name = self.transform.Find("Panel").Find("BossName_Text").GetComponent<Text>().text;
            foreach (Boss b in Enum.GetValues(typeof(Boss)))
            {
                if (name.Contains(b.ToString()))
                {
                    boss = b;
                    if (b != Boss.Isma) break;
                }
            }
            lev = level;
            orig(self, level, doHideAnim);
        }

        private void HubRemove()
        {
            foreach (var i in FindObjectsOfType<SpriteRenderer>().Where(x => x != null && x.name.Contains("SceneBorder"))) Destroy(i);
            string[] arr = { "Breakable Wall Waterways", "black_fader","White_Palace_throne_room_top_0000_2", "White_Palace_throne_room_top_0001_1",
                             "Glow Response floor_ring large2 (1)", "core_extras_0006_wp", "msk_station",
                             "core_extras_0028_wp (12)", "wp_additions_01", "BlurPlane",
                             "Inspect Region (1)", "core_extras_0021_wp (4)", "core_extras_0021_wp (5)","core_extras_0021_wp (1)", 
                             "core_extras_0021_wp (6)", "core_extras_0021_wp (7)","core_extras_0021_wp (2)"};
            foreach (var i in FindObjectsOfType<GameObject>().Where(x => x.activeSelf))
            {
                foreach (string j in arr)
                {
                    if (i.name.Contains(j))
                    {
                        Destroy(i);
                    }
                }
            }
            foreach (var i in FindObjectsOfType<GameObject>().Where(x => x.name.Contains("abyss") || x.name.Contains("Abyss"))) Destroy(i);
        }

        private void AddLift()
        {
            GameObject lift = Instantiate(FiveKnights.preloadedGO["lift"]);
            lift.transform.position = new Vector2(14f, 91.8f);
            lift.SetActive(true);
            lift.transform.localScale *= 1.15f;
            Vector2 sc = lift.transform.localScale;
            lift.transform.localScale = new Vector2(sc.x * 1.15f, sc.y);
            lift.LocateMyFSM("Control").enabled = false;
        }

        private GameObject SetStatue(Vector2 pos, Vector2 offset, Vector2 nameOffset,
                                    GameObject go, string sceneName, Sprite spr, 
                                    string name, string desc, string state)
        {
            //Used 56's pale prince code here
            GameObject statue = Instantiate(go);
            statue.transform.SetPosition3D(pos.x, pos.y, 0f);
            var scene = ScriptableObject.CreateInstance<BossScene>();
            scene.sceneName = sceneName;
            var bs = statue.GetComponent<BossStatue>();
            bs.bossScene = scene;
            bs.statueStatePD = state;
            bs.SetPlaquesVisible(bs.StatueState.isUnlocked && bs.StatueState.hasBeenSeen);
            var details = new BossStatue.BossUIDetails();
            details.nameKey = details.nameSheet = name;
            details.descriptionKey = details.descriptionSheet = desc;
            bs.bossDetails = details;
            foreach (Transform i in statue.transform)
            {
                if (i.name.Contains("door"))
                {
                    i.name = "door_dreamReturnGG" + state;
                }
            }
            GameObject appearance = statue.transform.Find("Base").Find("Statue").gameObject;
            appearance.SetActive(true);
            SpriteRenderer sr = appearance.transform.Find("GG_statues_0006_5").GetComponent<SpriteRenderer>();
            sr.enabled = true;
            sr.sprite = spr;
            var scaleX = sr.transform.GetScaleX();
            var scaleY = sr.transform.GetScaleY();
            float scaler = state.Contains("Hegemol") ? 2f : 1.4f;
            sr.transform.localScale *= scaler;
            sr.transform.SetPosition3D(sr.transform.GetPositionX() + offset.x, sr.transform.GetPositionY() + offset.y, 2f);
            if (state.Contains("Isma"))
            {
                SetStatue2(statue, "GG_White_Defender", "statueStateIsma2");
                bs.SetDreamVersion(FiveKnights.Instance.SaveSettings.BoolValues["AltStatueIsma"], false, false);
            }
            if (bs.StatueState.isUnlocked && bs.StatueState.hasBeenSeen)
            {

                statue.transform.Find("Base").gameObject.AddComponent<StatueControl>();
                
            }
            var tmp = statue.transform.Find("Inspect").Find("Prompt Marker").position;
            statue.transform.Find("Inspect").Find("Prompt Marker").position = new Vector3(tmp.x + nameOffset.x, tmp.y + nameOffset.y, tmp.z);
            statue.transform.Find("Inspect").gameObject.SetActive(true);
            statue.transform.Find("Spotlight").gameObject.SetActive(true);
            statue.SetActive(true);
            
            if (wonLastFight && state.Contains("Isma") && (boss == Boss.Isma || boss == Boss.Ogrim))
            {
                Log("IN2");
                BossStatue.Completion temp = (boss == Boss.Isma) ? bs.StatueState : bs.DreamStatueState;
                if (lev == 0) temp.completedTier1 = true;
                else if (lev == 1) temp.completedTier2 = true;
                else if (lev == 2) temp.completedTier3 = true;
                if (temp.completedTier1 && temp.completedTier2 && !temp.seenTier3Unlock) temp.seenTier3Unlock = true;
                if (boss == Boss.Isma)
                {
                    PlayerData.instance.currentBossStatueCompletionKey = bs.statueStatePD;
                    bs.StatueState = temp;
                    bs.SetPlaqueState(bs.StatueState, bs.altPlaqueL, bs.statueStatePD);
                }
                else
                {
                    PlayerData.instance.currentBossStatueCompletionKey = bs.dreamStatueStatePD;
                    bs.DreamStatueState = temp;
                    bs.SetPlaqueState(bs.DreamStatueState, bs.altPlaqueR, bs.dreamStatueStatePD);
                }
                wonLastFight = false;
            }
            if (wonLastFight && state.Contains(boss.ToString()))
            {
                Log("IN");
                BossStatue.Completion temp = bs.StatueState;
                if (lev == 0) temp.completedTier1 = true;
                else if (lev == 1) temp.completedTier2 = true;
                else if (lev == 2) temp.completedTier3 = true;
                if (temp.completedTier1 && temp.completedTier2 && !temp.seenTier3Unlock) temp.seenTier3Unlock = true;
                bs.StatueState = temp;
                PlayerData.instance.currentBossStatueCompletionKey = bs.statueStatePD;
                bs.SetPlaqueState(temp, bs.regularPlaque, bs.statueStatePD);
                wonLastFight = false;
            }

            return statue;
        }

        private void SetStatue2(GameObject statue, string sceneN, string stateN)
        {
            BossScene scene = ScriptableObject.CreateInstance<BossScene>();
            scene.sceneName = sceneN;

            BossStatue bs = statue.GetComponent<BossStatue>();
            bs.dreamBossScene = scene;
            bs.dreamStatueStatePD = stateN;

            /* 56's code { */

            //bs.SetPlaquesVisible(bs.StatueState.isUnlocked && bs.StatueState.hasBeenSeen || bs.isAlwaysUnlocked);

            Destroy(statue.FindGameObjectInChildren("StatueAlt"));

            GameObject displayStatue = bs.statueDisplay;

            GameObject alt = Instantiate
            (
                displayStatue,
                displayStatue.transform.parent,
                true
            );
            alt.SetActive(bs.UsingDreamVersion);
            alt.GetComponentInChildren<SpriteRenderer>(true).flipX = true;
            alt.name = "StatueAlt";
            bs.statueDisplayAlt = alt;

            /* } 56's code */

            BossStatue.BossUIDetails details = new BossStatue.BossUIDetails();
            details.nameKey = details.nameSheet = "DD_ISMA_NAME";
            details.descriptionKey = details.descriptionSheet = "DD_ISMA_DESC";
            bs.dreamBossDetails = details;

            GameObject altLever = statue.FindGameObjectInChildren("alt_lever");
            altLever.SetActive(true);
            GameObject switchBracket = altLever.FindGameObjectInChildren("GG_statue_switch_bracket");
            switchBracket.SetActive(true);

            GameObject switchLever = altLever.FindGameObjectInChildren("GG_statue_switch_lever");
            switchLever.SetActive(true);

            BossStatueLever toggle = statue.GetComponentInChildren<BossStatueLever>();
            toggle.SetOwner(bs);
            toggle.SetState(true);
        }

        private void OnDestroy()
        {
            On.GameManager.EnterHero -= GameManager_EnterHero;
            On.GameManager.BeginSceneTransition -= GameManager_BeginSceneTransition;
            On.BossChallengeUI.LoadBoss_int_bool -= BossChallengeUI_LoadBoss_int_bool;
            ModHooks.Instance.TakeHealthHook -= Instance_TakeHealthHook;
        }
        
        public static void Log(object o)
        {
            Modding.Logger.Log("[WP] " + o);
        }
    }
}