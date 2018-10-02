using BrilliantSkies.FromTheDepths.Game;
using BrilliantSkies.Effects.Cameras;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.Instances.Headers;
using BrilliantSkies.Ftd.Planets.Factions;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Ftd.Avatar;
using BrilliantSkies.Core.Id;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using BrilliantSkies.Core.Returns.PositionAndRotation;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Core.UniverseRepresentation.Positioning.Frames.Points;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Core.Constants;
using BrilliantSkies.Core.FilesAndFolders;
using Newtonsoft.Json;
using BrilliantSkies.Core.UniverseRepresentation;
using BrilliantSkies.Ftd.Planets;
using BrilliantSkies.Ftd.Planets.World;
using BrilliantSkies.GridCasts;
using BrilliantSkies.Ftd.Planets.Instances.Factions;

namespace Tournament
{
	public class Tournament : BrilliantSkies.FromTheDepths.Game.UserInterfaces.InteractiveOverlay.InteractiveOverlay
	{
		public static class SPAWN
		{
			public enum DIR
			{
				Facing,
				Away,
				Left,
				Right
			}

			public enum LOC
			{
				Air,
				Sea,
				Sub,
				Land
			}
		}

		public Tournament _me;

		public TournamentGUI _GUI;

		public Font _Font;

		public GUIStyle _Top;

		public GUIStyle _Left;

        public GUIStyle _Left2;

        public GUIStyle _Right;

        public GUIStyle _Right2;

        public GUIStyle _RightWrap;

        public bool started;

		public GameObject cam;

		public MouseLook flycam;

        public MouseOrbit orbitcam;

		public Vector3 lastpos;

		public int orbittarget;

        public int orbitMothership;

        public int orbitindex;

        public bool cammode;

        public bool extraInfo;

		private float timerTotal;

		private float timerTotal2;

		private bool overtime;

		public float minalt;

		public float maxalt;

		public float maxdis;

		public float maxoob;

		public float maxtime;
		
		public float maxmat;

		public float matconv = -1f;

		public float spawndis;

		public float spawngap;

        public float spawngap2;

        public float offset;

        public Tournament.SPAWN.DIR Dir;

        public Tournament.SPAWN.LOC Loc;

        public int northSouthBoard;

        public int eastWestBoard;

        public int defaultKeys;

        public bool defaultKeysBool;

        public float oobReverse; // out of bounds and maoving away speed limit before dq time

        public float oobMaxBuffer; //out of bounds and moving away too fast buffer time in secs

        //Defaults
        public float minaltD = -50f;

        public float maxaltD = 500f;

        public float maxdisD = 1500f;

        public float maxoobD = 120f;

        public float maxtimeD = 900f;

        public float maxmatD = 10000f;

        public float matconvD = -1f;

        public float spawndisD = 1000f;

        public float spawngapD = 100f;

        public float spawngap2D = 0f;

        public float offsetD = 0f;

        public float timer;

        public Tournament.SPAWN.DIR DirD = Tournament.SPAWN.DIR.Facing;

        public Tournament.SPAWN.LOC LocD = Tournament.SPAWN.LOC.Sea;

        public int northSouthBoardD = 0;

        public int eastWestBoardD = 0;

        public int defaultKeysD = 1;

        public float oobReverseD = 0; // out of bounds and maoving away speed limit before dq time. 0 will add dq time if move away at all, positve increases away speed limit, negative you need to move towards at at least this speed or pick up dq

        public float oobMaxBufferD = 3; //out of bounds and moving away too fast buffer time in secs

        private SortedDictionary<int, SortedDictionary<string, TournamentParticipant>> HUDLog = new SortedDictionary<int, SortedDictionary<string, TournamentParticipant>>();

		public float t1_res;

		public float t2_res;

		public List<TournamentEntry> entry_t1 = new List<TournamentEntry>();

		public List<TournamentEntry> entry_t2 = new List<TournamentEntry>();

		public Tournament()
		{
			_me = this;
			_GUI = new TournamentGUI(_me);

			_Top = new GUIStyle(LazyLoader.HUD.Get().interactionStyle);
            _Top.alignment = (TextAnchor)4;
            _Top.richText = true;
            _Top.fontSize = 12;

            _Left = new GUIStyle(LazyLoader.HUD.Get().interactionStyle);
            _Left.alignment = 0;
			_Left.richText = true;
			_Left.fontSize = 12;
			_Left.wordWrap = false;
			_Left.clipping = (TextClipping)1;

            _Left2 = new GUIStyle();
            _Left2.alignment = 0;
            _Left2.richText = true;
            _Left2.fontSize = 12;
            _Left2.normal.textColor = Color.white;

            _Right2 = new GUIStyle();
            _Right2.alignment = (TextAnchor)2;
            _Right2.richText = true;
            _Right2.fontSize = 12;
            _Right2.normal.textColor = Color.white;

            _Right = new GUIStyle(LazyLoader.HUD.Get().interactionStyle);
			_Right.alignment = (TextAnchor)2;
			_Right.richText = true;
			_Right.fontSize = 12;
			_Right.wordWrap = false;
			_Right.clipping = (TextClipping)1;

            _RightWrap = new GUIStyle(LazyLoader.HUD.Get().interactionStyle);
            _RightWrap.alignment = (TextAnchor)2;
            _RightWrap.richText = true;
            _RightWrap.fontSize = 12;
            _RightWrap.wordWrap = true;
            _RightWrap.clipping = (TextClipping)1;

            loadSettings();
		}

        public void loadCraft()
        {
            ClearArea();
            HUDLog.Clear();
            InstanceSpecification.i.Header.CommonSettings.EnemyBlockDestroyedResourceDrop = (matconv / 100f);
            t1_res = maxmat;
            foreach (TournamentEntry item in entry_t1)
            {
                item.Spawn(spawndis, spawngap, spawngap2, entry_t1.Count, entry_t1.IndexOf(item));
                item.team_id.FactionInst().ResourceStore.SetResources(maxmat);
            }
            t2_res = maxmat;
            foreach (TournamentEntry item2 in entry_t2)
            {
                item2.Spawn(spawndis, spawngap, spawngap2, entry_t2.Count, entry_t2.IndexOf(item2));
                item2.team_id.FactionInst().ResourceStore.SetResources(maxmat);
            }
        }

		public unsafe void StartMatch()
		{
            overtime = false;
            timer = 0f;
			timerTotal = 0f;
			timerTotal2 = Time.timeSinceLevelLoad;
			InstanceSpecification.i.Header.CommonSettings.ConstructableCleanUp = (ConstructableCleanUp)1;

			orbitindex = 0;
            orbittarget = 0;

            flycam.transform.position = (new Vector3(-500f, 50f, 0f));
            flycam.transform.rotation = (Quaternion.LookRotation(Vector3.right));
			cammode = false;
            foreach (MainConstruct constructable in StaticConstructablesManager.constructables)
			{
                int id = 0;
                if (constructable.Drones.loadedMothershipC != null)
                {
                    id = constructable.Drones.loadedMothershipC.UniqueId;
                }
                string key = "" + constructable.UniqueId + "," + id;

                if (!HUDLog.ContainsKey((constructable.GetTeam()).Id))
				{
					HUDLog.Add((constructable.GetTeam()).Id, new SortedDictionary<string, TournamentParticipant>());
				}
               
                if (!HUDLog[(constructable.GetTeam()).Id].ContainsKey(key))
				{
                    HUDLog[(constructable.GetTeam()).Id].Add(key, new TournamentParticipant
                    {
                        TeamId = constructable.GetTeam(),
                        TeamName = constructable.GetTeam().FactionSpec().AbreviatedName,
                        UniqueId = constructable.UniqueId,
                        MothershipId = id,
                        BlueprintName = constructable.GetName(),
                        AICount = constructable.BlockTypeStorage.MainframeStore.Blocks.Count,
						HP = ((constructable.BlockTypeStorage.MainframeStore.Blocks.Count > 0) ? (constructable.iMainStatus.GetFractionAliveBlocksIncludingSubConstructables() * 100f) : (constructable.iMainStatus.GetFractionAliveBlocks() * 100f)),
						HPCUR = (float)((constructable.BlockTypeStorage.MainframeStore.Blocks.Count > 0) ? constructable.iMainStatus.GetNumberAliveBlocksIncludingSubConstructables() : constructable.iMainStatus.GetNumberAliveBlocks()),
						HPMAX = (float)((constructable.BlockTypeStorage.MainframeStore.Blocks.Count > 0) ? constructable.iMainStatus.GetNumberBlocksIncludingSubConstructables() : constructable.iMainStatus.GetNumberBlocks())
					});
				}
			}
            started = true;
            GameEvents.UpdateEvent -= UpdateBoardSectionPreview;
            GameEvents.PreLateUpdate += LateUpdate;
            GameEvents.Twice_Second += SlowUpdate;
            GameEvents.FixedUpdateEvent += FixedUpdate;
            GameEvents.OnGui += OnGUI;
            Time.timeScale = 0f;
		}

		public unsafe void ClearArea()
		{
			(ForceManager.Instance).forces.ForEach(delegate(Force t)
			{
				ForceManager.Instance.DeleteForce(t);
			});
		}

		public unsafe void ResetCam()
		{
            foreach (PlayerSetupBase @object in (Objects.Instance.Players.Objects))
            {
                GameObject.Destroy(@object.gameObject);
            }
            cam = R_Avatars.JustOrbitCamera.InstantiateACopy().gameObject;
            cam.gameObject.transform.position = new Vector3(-500f, 50f, 0f);
            cam.gameObject.transform.rotation = Quaternion.LookRotation(Vector3.right);
            cam.AddComponent<MouseLook>();
            flycam = cam.GetComponent<MouseLook>();
			flycam.enabled = true;
			flycam.transform.position = new Vector3(-500f, 50f, 0f);
			flycam.transform.rotation = Quaternion.LookRotation(Vector3.right);
			orbitcam = cam.GetComponent<MouseOrbit>();
            orbitcam.OperateRegardlessOfUiOptions = true;
			orbitcam.distance = 100f;
			orbitcam.enabled = false;
            //orbittarget = StaticConstructablesManager.constructables[0].UniqueId;
            orbittarget = 0;
            orbitindex = 0;
            orbitMothership = -1;
            extraInfo = false;
			cammode = false;
            
		}

        public void MoveCam()
        {
            cam.transform.position = FramePositionOfBoardSection() + new Vector3(0, 50, 0);
        }

        public Vector3 FramePositionOfBoardSection()
        {
            return PlanetList.MainFrame.UniversalPositionToFramePosition(UniversalPositionOfBoardSection());
        }

        public Vector3d UniversalPositionOfBoardSection()
        {
            return StaticCoordTransforms.BoardSectionToUniversalPosition(WorldSpecification.i.BoardLayout.BoardSections[eastWestBoard, northSouthBoard].BoardSectionCoords);
        }

        public void saveSettings()
        {
            if (defaultKeysBool == true)
            {
                defaultKeys = 1;
            }
            else
            {
                defaultKeys = 0;
            }

            string modFolder = Get.PerminentPaths.GetSpecificModDir("Tournament").ToString();
            FilesystemFileSource settingsFile = new FilesystemFileSource(modFolder + "settings.cfg");
            List<float> settingsList = new List<float>();
            settingsList.Add(minalt);
            settingsList.Add(maxalt);
            settingsList.Add(maxdis);
            settingsList.Add(maxoob);
            settingsList.Add(maxtime);
            settingsList.Add(maxmat);
            settingsList.Add(spawndis);
            settingsList.Add(spawngap);
            settingsList.Add(offset);
            settingsList.Add((float)Dir);
            settingsList.Add((float)Loc);
            settingsList.Add((float)defaultKeys);
            settingsList.Add((float)eastWestBoard);
            settingsList.Add((float)northSouthBoard);
            settingsList.Add(spawngap2);
            settingsList.Add(oobMaxBuffer);
            settingsList.Add(oobReverse);

            settingsFile.SaveData(settingsList, Formatting.None);
        }

        public void loadSettings()
        {
            string modFolder = Get.PerminentPaths.GetSpecificModDir("Tournament").ToString();
            FilesystemFileSource settingsFile = new FilesystemFileSource(modFolder + "settings.cfg");
            if (settingsFile.Exists)
            {
                List<float> settingsList = settingsFile.LoadData<List<float>>();

                minalt = settingsList[0];
                maxalt = settingsList[1];
                maxdis = settingsList[2];
                maxoob = settingsList[3];
                maxtime = settingsList[4];
                maxmat = settingsList[5];
                spawndis = settingsList[6];
                spawngap = settingsList[7];
                offset = settingsList[8];
                Dir = (SPAWN.DIR)settingsList[9];
                Loc = (SPAWN.LOC)settingsList[10];
                defaultKeys = (int)settingsList[11];
                eastWestBoard = (int)settingsList[12];
                northSouthBoard = (int)settingsList[13];
                spawngap2 = settingsList[14];
                oobMaxBuffer = settingsList[15];
                oobReverse = settingsList[16];

                if (defaultKeys == 1)
                {
                    defaultKeysBool = true;
                }
                else
                {
                    defaultKeysBool = false;
                }
            }
            else
            {
                loadDefaults();
            }
        }


        public void loadDefaults()
        {
            spawndis = spawndisD;
            spawngap = spawngapD;
            spawngap2 = spawngap2D;
            minalt = minaltD;
            maxalt = maxaltD;
            maxdis = maxdisD;
            maxoob = maxoobD;
            maxtime = maxtimeD;
            maxmat = maxmatD;
            Dir = DirD;
            Loc = LocD;
            offset = offsetD;
            defaultKeys = defaultKeysD;
            eastWestBoard = eastWestBoardD;
            northSouthBoard = northSouthBoardD;
            oobMaxBuffer = oobMaxBufferD;
            oobReverse = oobReverseD;
            if (defaultKeys == 1)
            {
                defaultKeysBool = true;
            }
            else
            {
                defaultKeysBool = false;
            }
        }

		public unsafe void OnGUI()
		{
			GUI.matrix = (Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f * (float)Screen.width / 1280f, 1f * (float)Screen.height / 800f, 1f)));
			GUI.backgroundColor = new Color(0f, 0f, 0f, 0.6f);
			GUI.Label(new Rect(590f, 0f, 100f, 30f), $"{Math.Floor((double)(timerTotal / 60f))}m {Math.Floor((double)timerTotal) % 60.0}s", _Top);
            
            foreach (KeyValuePair<int, SortedDictionary<string, TournamentParticipant>> team in HUDLog)
            {
                bool teamOne = entry_t1[0].team_id.Id == team.Key;

                float xOffset = 0f;
                float oobOffset = 2f;
                float hpOffset = 42f;
                float nameOffset = 78f;

                GUIStyle style = _Left2;
                if (!teamOne)
                {
                    style = _Right2;
                    xOffset = 1080f;
                    oobOffset = 160f;
                    hpOffset = 128f;
                    nameOffset = 2f;
                }

                float height = 38f + 16f * HUDLog[team.Key].Values.Count;

                float teamCurrentHP = 0f;
                float teamMaxHP = 0f;

                string teamName = $"Team {(teamOne ? 1f : 2f)}";
                string materials = FactionSpecifications.i.Factions.Where(x => x.Id.Id == team.Key).First().InstanceOfFaction.ResourceStore.Material.ToString()+"M";

                GUILayout.BeginArea(new Rect(xOffset, 0f, 200f, height), "", _Top);

                int entries = 0;

                foreach (KeyValuePair<string, TournamentParticipant> entry in team.Value)
                {
                    string oob = "";
                    string percentHP = "";
                    string nameBP = $"{entry.Value.BlueprintName}";
                    string dq = "DQ";
                    bool dqed = true;

                    teamMaxHP += entry.Value.HPMAX;

                    if (!entry.Value.Disqual && !entry.Value.Scrapping && entry.Value.AICount != 0)
                    {
                        dqed = false;
                        teamCurrentHP += entry.Value.HPCUR;
                        oob = $"{Math.Floor((double)(entry.Value.OoBTime / 60f))}m{Math.Floor((double)entry.Value.OoBTime) % 60.0}s";
                        percentHP = $"{Math.Round((double)entry.Value.HP, 1)}%";
                        //text = ((!flag) ? (text + string.Format("\n{2} {1,4} {0,6}", Math.Floor((double)(item2.Value.OoBTime / 60f)) + "m" + Math.Floor((double)item2.Value.OoBTime) % 60.0 + "s", Math.Round((double)item2.Value.HP, 1) + "%", item2.Value.BlueprintName)) : (text + string.Format("\n{0,-6} {1,-4} {2}", Math.Floor((double)(item2.Value.OoBTime / 60f)) + "m" + Math.Floor((double)item2.Value.OoBTime) % 60.0 + "s", Math.Round((double)item2.Value.HP, 1) + "%", item2.Value.BlueprintName)));
                    }

                    // member name, hp + oob time or DQ
                    if (!dqed)
                    {
                        GUI.Label(new Rect(hpOffset, 38f + entries * 16f, 30f, 16f), percentHP, style);
                        GUI.Label(new Rect(oobOffset, 38f + entries * 16f, 38f, 16f), oob, style);
                    }
                    else
                    {
                        GUI.Label(new Rect(oobOffset, 38f + entries * 16f, 30f, 16f), dq, style);
                    }
                    
                    //GUI.Label(new Rect(nameOffset, 38f + entries*16f, 124f, 16f), nameBP, style);

                    float scrollSpeed = 30;
                    float t = Time.realtimeSinceStartup * scrollSpeed;

                    //float dimensions = _Left2.fontSize * nameBP.Length;
                    var dimensions = _Left2.CalcSize(new GUIContent(nameBP));
                    float width = dimensions.x+120f;

                    if (dimensions.x <= 120f)
                    {
                        GUI.Label(new Rect(nameOffset, 38f + entries * 16f, 120f, 16f), nameBP, style);
                    }
                    else
                    {
                        GUI.BeginScrollView(new Rect(nameOffset, 38f + entries * 16f, 120f, 16f), new Vector2(t % width, 0), new Rect(-width, 0, 2*dimensions.x+120f, 16f), GUIStyle.none, GUIStyle.none);
                        GUI.Label(new Rect(-dimensions.x, 0, dimensions.x, 16f), nameBP, style);
                        GUI.EndScrollView();
                    }

                    entries++;
                }

                // team name , hp , mats
                string teamHP = Math.Round(teamCurrentHP / teamMaxHP * 100, 1) + "%";
                string topLabel = "";
                if (teamOne)
                {
                    topLabel = $"{teamName}  <color=#ffa500ff>{teamHP}</color>  <color=cyan>{materials}</color>"; 
                }
                else
                {
                    topLabel = $"<color=cyan>{materials}</color>  <color=#ffa500ff>{teamHP}</color>  {teamName}";
                }
                GUI.Label(new Rect(4f, 6f, 192f, 32f), topLabel, style);

                GUILayout.EndArea();
            }

            // extra info panel
            if (extraInfo)
            {
                //int target = getTarget();
                IMainConstructBlock target = getTarget();
                if (target != null)
                {
                    //int targetIndex = StaticConstructablesManager.constructables.FindIndex(0, m => m.UniqueId == target);
                    MainConstruct targetConstruct = StaticConstructablesManager.constructables.Where(x => x.iMain == target).First();
                    //MainConstruct targetConstruct = StaticConstructablesManager.constructables[targetIndex];

                    string name = targetConstruct.blueprintName;
                    string hp = $"{Math.Round(targetConstruct.iMainStatus.GetFractionAliveBlocksIncludingSubConstructables() * 100f, 1).ToString()}%";
                    string ammo = $"{Math.Round(targetConstruct.Ammo.Ammo.Quantity, 0)}/{Math.Round(targetConstruct.Ammo.Ammo.Maximum, 0)}";
                    string fuel = $"{Math.Round(targetConstruct.PowerUsageCreationAndFuel.Fuel.Quantity, 0)}/{Math.Round(targetConstruct.PowerUsageCreationAndFuel.Fuel.Maximum, 0)}";
                    string battery = $"{Math.Round(targetConstruct.PowerUsageCreationAndFuel.Energy.Quantity, 0)}/{Math.Round(targetConstruct.PowerUsageCreationAndFuel.Energy.Maximum, 0)}";
                    //power as current output / max
                    //string power = $"{Math.Round(StaticConstructablesManager.constructables[targetIndex].PowerUsageCreationAndFuel.MaxPower - StaticConstructablesManager.constructables[targetIndex].PowerUsageCreationAndFuel.Power, 0)}" +
                    //    $"/{Math.Round(StaticConstructablesManager.constructables[targetIndex].PowerUsageCreationAndFuel.MaxPower, 0)}";
                    // power as available /max#
                    string power = $"{Math.Round(targetConstruct.PowerUsageCreationAndFuel.Power, 0)} / {Math.Round(targetConstruct.PowerUsageCreationAndFuel.MaxPower, 0)}";
                    string altitude = Math.Round(targetConstruct.CentreOfMass.y, 0).ToString();

                    float closest = -1f;
                    foreach (MainConstruct construct in StaticConstructablesManager.constructables.ToArray())
                    {  
                        if (construct.GetTeam() != targetConstruct.GetTeam())
                        {
                            float distance = Vector3.Distance(construct.CentreOfMass, targetConstruct.CentreOfMass);
                            if (closest < 0f || distance < closest)
                            {
                                closest = distance;
                            }
                        }
                    }
                    string nearest = Math.Round(closest,0).ToString();


                    float xOffsetLabel;
                    float xOffsetValue;

                    int kingId = InstanceSpecification.i.Factions.Factions.Find((InstanceFaction f) => f.FactionSpec.Name == "KING").Id.Id;
                    if(targetConstruct.GetTeam().Id == kingId) // team 1
                    {
                        xOffsetLabel = 200;
                        xOffsetValue = 290;
                    }
                    else
                    {
                        xOffsetLabel = 880;
                        xOffsetValue = 970;
                    }
              
                    GUI.Label(new Rect(xOffsetLabel, 0f, 90f, 38f), "Name:", _Left);
                    GUI.Label(new Rect(xOffsetLabel, 38f, 90f, 38f), "HP:", _Left);
                    GUI.Label(new Rect(xOffsetLabel, 76f, 90f, 38f), "Ammo:", _Left);
                    GUI.Label(new Rect(xOffsetLabel, 114f, 90f, 38f), "Fuel:", _Left);
                    GUI.Label(new Rect(xOffsetLabel, 152f, 90f, 38f), "Battery:", _Left);
                    GUI.Label(new Rect(xOffsetLabel, 190f, 90f, 38f), "Power:", _Left);
                    GUI.Label(new Rect(xOffsetLabel, 228f, 90f, 38f), "Altitude:", _Left);
                    GUI.Label(new Rect(xOffsetLabel, 266f, 90f, 38f), "Nearest Enemy:", _Left);

                    GUI.Label(new Rect(xOffsetValue, 0f, 110f, 38f), name, _RightWrap);
                    GUI.Label(new Rect(xOffsetValue, 38f, 110f, 38f), hp, _Right);
                    GUI.Label(new Rect(xOffsetValue, 76f, 110f, 38f), ammo, _Right);
                    GUI.Label(new Rect(xOffsetValue, 114f, 110f, 38f), fuel, _Right);
                    GUI.Label(new Rect(xOffsetValue, 152f, 110f, 38f), battery, _Right);
                    GUI.Label(new Rect(xOffsetValue, 190f, 110f, 38f), power, _Right);
                    GUI.Label(new Rect(xOffsetValue, 228f, 110f, 38f), altitude, _Right);
                    GUI.Label(new Rect(xOffsetValue, 266f, 110f, 38f), nearest, _Right);





                }
            }
		}

        public IMainConstructBlock getTarget()
        {
            IMainConstructBlock target = null;

            Transform myTransform = flycam.enabled ? flycam.transform : orbitcam.transform;
            GridCastReturn gridCastReturn = GridCasting.GridCastAllConstructables(new GridCastReturn(myTransform.position, myTransform.forward, 500.0f, 10, true));
            if (gridCastReturn.HitSomething)
            {
                if (gridCastReturn.FirstHit.BlockHit.IsOnSubConstructable)
                {
                    target = gridCastReturn.FirstHit.BlockHit.ParentConstruct.iMain;
                }
                else
                {
                    target = gridCastReturn.FirstHit.BlockHit.MainConstruct;
                }
            }
            return target;
        }

        public void UpdateBoardSectionPreview(ITimeStep dt)
        {
            cam.transform.Rotate(0, (float)(15 * dt.PhysicalDeltaTime.Seconds), 0);//15° rotation per second
        }

        public void LateUpdate()
		{
            FtdKeyMap ftdKeyMap = ProfileManager.Instance.GetModule<FtdKeyMap>();

            bool pause = false;
            bool next = false;
            bool previous = false;
            bool shift = false;
            bool freecamOn = false;
            bool orbitcamOn = false;
            bool changeExtraInfo = false;

            switch (defaultKeysBool)
            {
                case false:
                    pause = Input.GetKeyUp(ftdKeyMap.GetKeyDef(KeyInputsFtd.PauseGame).Key);
                    shift = Input.GetKey(KeyCode.LeftShift) | Input.GetKey(KeyCode.RightShift);
                    next = Input.GetKeyUp(ftdKeyMap.GetKeyDef(KeyInputsFtd.InventoryUi).Key);
                    previous = Input.GetKeyUp(ftdKeyMap.GetKeyDef(KeyInputsFtd.Interact).Key);
                    changeExtraInfo = Input.GetKeyUp(ftdKeyMap.GetKeyDef(KeyInputsFtd.CharacterSheetUi).Key);
                    freecamOn = Input.GetMouseButtonUp(1); // technically same as default atm
                    orbitcamOn = Input.GetMouseButtonUp(0); // technically same as default atm
                    break;
                case true:
                    pause = Input.GetKeyUp((KeyCode)292); // default f11
                    shift = Input.GetKey((KeyCode)303) || Input.GetKey((KeyCode)304);
                    next = Input.GetKeyUp((KeyCode)101); // default e
                    previous = Input.GetKeyUp((KeyCode)113); // default q
                    changeExtraInfo = Input.GetKeyUp((KeyCode)122); // default z
                    freecamOn = Input.GetKeyUp((KeyCode)324); // default left click
                    orbitcamOn = Input.GetKeyUp((KeyCode)323); // default right click
                    break;
            }

            if (pause)
			{
                //Time.set_timeScale((Time.get_timeScale() > 0f) ? 0f : 1f);
                Time.timeScale = ((Time.timeScale > 0f) ? 0f : 1f);
            }
            if (shift)
            {
                orbitcam.xSpeed = 1000;
                orbitcam.ySpeed = 480;
            }
            else
            {
                orbitcam.xSpeed = 250;
                orbitcam.ySpeed = 120;
            }
            if (changeExtraInfo)
            {
                extraInfo =  (extraInfo == true) ? false : true;
            }
			if (Input.GetAxis("Mouse ScrollWheel") != 0f)
			{
				if (shift)
				{
					orbitcam.distance = ((orbitcam.distance - Input.GetAxis("Mouse ScrollWheel") > 0f) ? (orbitcam.distance - Input.GetAxis("Mouse ScrollWheel") * 100f) : 0f);
				}
				else
				{
					orbitcam.distance = ((orbitcam.distance - Input.GetAxis("Mouse ScrollWheel") > 0f) ? (orbitcam.distance - Input.GetAxis("Mouse ScrollWheel") * 50f) : 0f);
				}
			}
            if (StaticConstructablesManager.constructables.Count > 0)
            {
                if (orbitindex >= StaticConstructablesManager.constructables.Count)
                {
                    orbitindex = 0;
                }
                if (StaticConstructablesManager.constructables.ToArray()[orbitindex].UniqueId != orbittarget && orbittarget != 0 ||
                    (orbitMothership != -1 && StaticConstructablesManager.constructables.ToArray()[orbitindex].Drones.loadedMothershipC.uniqueID != orbitMothership))
                {
                    int index;
                    if (orbitMothership == -1)
                    {
                        index = StaticConstructablesManager.constructables.FindIndex(0, m => m.UniqueId == orbittarget);
                    }
                    else
                    {
                        index = StaticConstructablesManager.constructables.FindIndex(0, m => m.UniqueId == orbittarget && m.Drones.loadedMothershipC.uniqueID == orbitMothership);
                    }
                    if (index >= 0) { orbitindex = index; }
                    else { orbitindex = 0; }
                }
                if (next)
                {
                    if (orbitindex + 1 >= StaticConstructablesManager.constructables.Count)
                    {
                        orbitindex = 0;
                        //orbittarget = StaticConstructablesManager.constructables.ToArray()[orbitindex].UniqueId;
                    }
                    else
                    {
                        orbitindex++;
                        //orbittarget = StaticConstructablesManager.constructables.ToArray()[orbitindex].UniqueId;
                    }
                }
                if (previous)
                {
                    if (orbitindex == 0)
                    {
                        orbitindex = StaticConstructablesManager.constructables.Count - 1;
                        //orbittarget = StaticConstructablesManager.constructables.ToArray()[orbitindex].UniqueId;
                    }
                    else
                    {
                        orbitindex--;
                        //orbittarget = StaticConstructablesManager.constructables.ToArray()[orbitindex].UniqueId;
                    }
                }

                if (orbittarget != StaticConstructablesManager.constructables.ToArray()[orbitindex].UniqueId ||
                    (StaticConstructablesManager.constructables.ToArray()[orbitindex].Drones.loadedMothershipC != null &&
                     orbitMothership != StaticConstructablesManager.constructables.ToArray()[orbitindex].Drones.loadedMothershipC.uniqueID))
                {
                    orbittarget = StaticConstructablesManager.constructables.ToArray()[orbitindex].UniqueId;
                    if (StaticConstructablesManager.constructables.ToArray()[orbitindex].Drones.loadedMothershipC != null)
                    {
                        orbitMothership = StaticConstructablesManager.constructables.ToArray()[orbitindex].Drones.loadedMothershipC.uniqueID;
                    }
                    else
                    { 
                        orbitMothership = -1;
                    }
                }
            }
            if (orbitcamOn && StaticConstructablesManager.constructables.Count != 0)
			{
				flycam.enabled = false;
				orbitcam.enabled = true;
			}
			else if (freecamOn)
			{
				orbitcam.enabled = false;
				flycam.enabled = true;
				flycam.transform.rotation = orbitcam.transform.rotation;
			}
			if (flycam.enabled && defaultKeysBool)
			{
                //Vector3 val = new Vector3(Input.GetAxisRaw("Sidestep"), Input.GetKey((KeyCode)32) ? 1f : ((Input.GetKey((KeyCode)308) | Input.GetKey((KeyCode)307)) ? (-1f) : 0f), Input.GetAxisRaw("ForwardsBackwards"));
                float x = 0;
                float y = 0;
                float z = 0;
                if(Input.GetKey((KeyCode)32)) //space  = up
                {
                    y += 1;
                }
                if(Input.GetKey((KeyCode)308) | Input.GetKey((KeyCode)307)) // alt = down
                {
                    y -= 1;
                }
                if (Input.GetKey((KeyCode)97)) // a = left
                {
                    x -= 1;
                }
                if (Input.GetKey((KeyCode)100)) // d= right
                {
                    x += 1;
                }
                if (Input.GetKey((KeyCode)119)) // w = forward
                {
                    z += 1;
                }
                if (Input.GetKey((KeyCode)115)) // s = back
                {
                    z -= 1;
                }
                Vector3 val = new Vector3(x, y, z);
                if (shift)
				{
					val = Vector3.Scale(val, new Vector3(5f, 5f, 5f)); // increase vector with shift
                }
                flycam.transform.position = flycam.transform.position + flycam.transform.localRotation * val;
            }
            if (flycam.enabled && !defaultKeysBool)
            {
                Vector3 movement = ftdKeyMap.GetMovemementDirection() * (shift ? 5 : 1);
                flycam.transform.position += flycam.transform.localRotation * movement;
            }
            else if (orbitcam.enabled)
            {
                if (StaticConstructablesManager.constructables.Count == 0)
                {
                    flycam.enabled = true;
                    orbitcam.enabled = false;
                }
                else
                {
                    Vector3d position = PlanetList.MainFrame.FramePositionToUniversalPosition(StaticConstructablesManager.constructables.ToArray()[orbitindex].CentreOfMass);
                    Quaternion rotation = StaticConstructablesManager.constructables.ToArray()[orbitindex].SafeRotation;
                    orbitcam.OrbitTarget = new PositionAndRotationReturnUniverseCoord(new UniversalTransform(position, rotation));
                }
            }
		}

		public unsafe void FixedUpdate(ITimeStep dt)
		{
			
			if (Time.timeScale > 0f)
			{
				if (matconv == -1f)
				{
					if ((double)t1_res < entry_t1[0].team_id.FactionInst().ResourceStore.Material.Quantity)
					{
						entry_t1[0].team_id.FactionInst().ResourceStore.SetResources(t1_res);
					}
					else
					{
						t1_res = (float)entry_t1[0].team_id.FactionInst().ResourceStore.Material.Quantity;
					}
					if ((double)t2_res < entry_t2[0].team_id.FactionInst().ResourceStore.Material.Quantity)
					{
						entry_t2[0].team_id.FactionInst().ResourceStore.SetResources(t2_res);
					}
					else
					{
						t2_res = (float)entry_t2[0].team_id.FactionInst().ResourceStore.Material.Quantity;
					}
				}
				MainConstruct[] array = StaticConstructablesManager.constructables.ToArray();
				foreach (MainConstruct val in array)
				{
                    //Debug.Log("FixedUpdate ID: " + val.UniqueId);
                    int id = 0;
                    if (val.Drones.loadedMothershipC != null)
                    {
                        id = val.Drones.loadedMothershipC.UniqueId;
                    }
                    string key = "" + val.UniqueId + "," + id;
                    //Debug.Log("FixedUpdate mothership ID: " + val.Drone.loadedMothershipC.UniqueId);
                    //if (!HUDLog[((IntPtr)(void*)val.GetTeam()).Id][val.get_UniqueId()].Disqual || !HUDLog[((IntPtr)(void*)val.GetTeam()).Id][val.get_UniqueId()].Scrapping)
                    if (!HUDLog[(val.GetTeam()).Id][key].Disqual || !HUDLog[(val.GetTeam()).Id][key].Scrapping)
					{
						HUDLog[(val.GetTeam()).Id][key].AICount = val.BlockTypeStorage.MainframeStore.Blocks.Count;
						if ((val.CentreOfMass).y < minalt || (val.CentreOfMass).y > maxalt)
						{
							HUDLog[(val.GetTeam()).Id][key].OoBTime += Time.timeSinceLevelLoad - timerTotal - timerTotal2;
						}
						else
						{
							float num = -1f;
							float num2 = -1f;
							MainConstruct[] array2 = StaticConstructablesManager.constructables.ToArray();
							foreach (MainConstruct val2 in array2)
							{
								if (val != val2 && val.GetTeam() != val2.GetTeam())
								{
									float num3 = Vector3.Distance(val.CentreOfMass, val2.CentreOfMass);
									if (num < 0f)
									{
										num = num3;
                                        //heading towards
                                        //num2 = Vector3.Distance(val.CentreOfMass + val.get_MainPhysics().get_iVelocities().get_VelocityVector(), val2.CentreOfMass;
                                        num2 = Vector3.Distance(val.CentreOfMass + val.Velocity, val2.CentreOfMass);
                                            
                                    }
									else if (Vector3.Distance(val.CentreOfMass, val2.CentreOfMass) < num)
									{
										num = num3;
										//num2 = Vector3.Distance(val.get_CentreOfMass() + val.get_MainPhysics().get_iVelocities().get_VelocityVector(), val2.get_CentreOfMass());
                                        num2 = Vector3.Distance(val.CentreOfMass + val.Velocity, val2.CentreOfMass);
                                    }
								}
							}
							if (num > maxdis && num < num2-oobReverse) //out of bounds and moving away faster than  oobReverse m/s
							{
                                HUDLog[(val.GetTeam()).Id][key].OoBTimeBuffer += Time.timeSinceLevelLoad - timerTotal - timerTotal2;
                                if(HUDLog[(val.GetTeam()).Id][key].OoBTimeBuffer > oobMaxBuffer)
                                {
                                    HUDLog[(val.GetTeam()).Id][key].OoBTime += Time.timeSinceLevelLoad - timerTotal - timerTotal2;
                                } 
							}
                            else
                            {
                                HUDLog[(val.GetTeam()).Id][key].OoBTimeBuffer = 0;
                            }
						}
						HUDLog[(val.GetTeam()).Id][key].Disqual = (HUDLog[(val.GetTeam()).Id][key].OoBTime > maxoob);
					}
				}
				timerTotal += Time.timeSinceLevelLoad - timerTotal - timerTotal2;
			}
		}

		public unsafe void SlowUpdate(ITimeStep dt)
		{
			
			MainConstruct[] array = StaticConstructablesManager.constructables.ToArray();
			foreach (MainConstruct val in array)
			{
                int id = 0;
                if (val.Drones.loadedMothershipC != null)
                {
                    id = val.Drones.loadedMothershipC.UniqueId;
                }
                string key = "" + val.UniqueId + "," + id;
                if (!HUDLog[(val.GetTeam()).Id][key].Disqual || !HUDLog[(val.GetTeam()).Id][key].Scrapping)
				{
					HUDLog[(val.GetTeam()).Id][key].HPCUR = (float)val.iMainStatus.GetNumberAliveBlocksIncludingSubConstructables();
					HUDLog[(val.GetTeam()).Id][key].HP = val.iMainStatus.GetFractionAliveBlocksIncludingSubConstructables() * 100f;
				}
				else
				{
					HUDLog[(val.GetTeam()).Id][key].HPCUR = 0f;
					HUDLog[(val.GetTeam()).Id][key].HP = 0f;
				}
			}
			foreach (KeyValuePair<int, SortedDictionary<string, TournamentParticipant>> item in HUDLog)
			{
				foreach (KeyValuePair<string, TournamentParticipant> item2 in HUDLog[item.Key])
				{
					List<MainConstruct> constructables = StaticConstructablesManager.constructables;
					List<MainConstruct> constructables2 = StaticConstructablesManager.constructables;
					Predicate<MainConstruct> match = delegate(MainConstruct c)
					{
						ObjectId team3 = c.GetTeam();
						KeyValuePair<string, TournamentParticipant> keyValuePair9 = item2;
						if (team3 == keyValuePair9.Value.TeamId)
						{
                            int id3 = 0;
                            if (c.Drones.loadedMothershipC != null)
                            {
                                id3 = c.Drones.loadedMothershipC.UniqueId;
                            }
                            string uniqueId3 = "" + c.UniqueId + "," + id3;
							KeyValuePair<string, TournamentParticipant> keyValuePair10 = item2;
							return uniqueId3 == keyValuePair10.Key;
						}
						return false;
					};
					if (constructables.Contains(constructables2.Find(match)))
					{
						SortedDictionary<string, TournamentParticipant> sortedDictionary = HUDLog[item.Key];
						KeyValuePair<string, TournamentParticipant> keyValuePair = item2;
						if (!sortedDictionary[keyValuePair.Key].Disqual)
						{
							continue;
						}
					}
					SortedDictionary<string, TournamentParticipant> sortedDictionary2 = HUDLog[item.Key];
					KeyValuePair<string, TournamentParticipant> keyValuePair2 = item2;
					if (!sortedDictionary2[keyValuePair2.Key].Scrapping)
					{
						SortedDictionary<string, TournamentParticipant> sortedDictionary3 = HUDLog[item.Key];
						KeyValuePair<string, TournamentParticipant> keyValuePair3 = item2;
						sortedDictionary3[keyValuePair3.Key].HPCUR = 0f;
						SortedDictionary<string, TournamentParticipant> sortedDictionary4 = HUDLog[item.Key];
						KeyValuePair<string, TournamentParticipant> keyValuePair4 = item2;
						sortedDictionary4[keyValuePair4.Key].Scrapping = true;
						Vector3 centreOfMass = StaticConstructablesManager.constructables.Find(delegate(MainConstruct c)
						{
							ObjectId team2 = c.GetTeam();
							KeyValuePair<string, TournamentParticipant> keyValuePair7 = item2;
							if (team2 == keyValuePair7.Value.TeamId)
							{
                                int id2 = 0;
                                if (c.Drones.loadedMothershipC != null)
                                {
                                    id2 = c.Drones.loadedMothershipC.UniqueId;
                                }
                                string uniqueId2 = "" + c.UniqueId + "," + id2;
                                KeyValuePair<string, TournamentParticipant> keyValuePair8 = item2;
								return uniqueId2 == keyValuePair8.Key;
							}
							return false;
						}).CentreOfMass;
						GameObject.Instantiate<GameObject>(Resources.Load("Detonator-MushroomCloud") as GameObject, centreOfMass, Quaternion.identity);
						StaticConstructablesManager.constructables.Find(delegate(MainConstruct c)
						{
							ObjectId team = c.GetTeam();
							KeyValuePair<string, TournamentParticipant> keyValuePair5 = item2;
							if (team == keyValuePair5.Value.TeamId)
							{
                                int id1 = 0;
                                if (c.Drones.loadedMothershipC != null)
                                {
                                    id1 = c.Drones.loadedMothershipC.UniqueId;
                                }
                                string uniqueId1 = "" + c.UniqueId + "," + id1;
                                KeyValuePair<string, TournamentParticipant> keyValuePair6 = item2;
								return uniqueId1 == keyValuePair6.Key;
							}
							return false;
						}).DestroyCompletely(true);
					}
				}
			}
			if (timerTotal > maxtime && !overtime)
			{
				Time.timeScale = 0f;
				overtime = true;
			}
		}
	}
}
