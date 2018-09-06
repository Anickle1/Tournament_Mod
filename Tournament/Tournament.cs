using BrilliantSkies.FromTheDepths.Game;
using BrilliantSkies.Effects.Cameras;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.Instances.Headers;
using BrilliantSkies.Ftd.Planets.Factions;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Ftd.Avatar;
using BrilliantSkies.Core.Id;
using BrilliantSkies.Ftd.Avatar.Movement;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using BrilliantSkies.Core.Returns.PositionAndRotation;
using BrilliantSkies.PlayerProfiles;
using BrilliantSkies.Ui.Displayer;
using BrilliantSkies.Core.UniverseRepresentation.Positioning.Frames.Points;
using BrilliantSkies.Core.Types;

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

		public static class OPTIONS
		{
			public enum STANDARDRULES
			{
				CustomRules,
				StandardRules
			}

			public enum AIPENALTY
			{
				NoAIPenaltyOff,
				NoAIPenaltyOn
			}

			public enum HPMODE
			{
				ResourceHP,
				StandardHP
			}
		}

		public Tournament _me;

		public TournamentGUI _GUI;

		public Font _Font;

		public GUIStyle _Top;

		public GUIStyle _Left;

		public GUIStyle _Right;

		public bool started;

		public GameObject cam;

		public MouseLook flycam;

        public MouseOrbit orbitcam;

		public Vector3 lastpos;

		public int orbittarget;

		public int orbitindex;

		public bool cammode;

		private float timerTotal;

		private float timerTotal2;

		private bool overtime;

		public float minalt = -50f;

		public float maxalt = 500f;

		public float maxdis = 1500f;

		public float maxoob = 120f;

		public float maxtime = 900f;

		public float maxcost = 150000f;

		public float maxmat = 10000f;

		public float matconv = -1f;

		public float spawndis = 1000f;

		public float spawngap = 100f;

		public float penaltyhp = 50f;

		public float detection = 70f;

        public float offset = 0f;

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
            _Left = new GUIStyle(LazyLoader.HUD.Get().interactionStyle);;
            _Left.alignment = 0;
			_Left.richText = true;
			_Left.fontSize = 12;
			_Left.wordWrap = false;
			_Left.clipping = (TextClipping)1;
			_Right = new GUIStyle(LazyLoader.HUD.Get().interactionStyle);
			_Right.alignment = (TextAnchor)2;
			_Right.richText = true;
			_Right.fontSize = 12;
			_Right.wordWrap = false;
			_Right.clipping = (TextClipping)1;
		}

        public void loadCraft()
        {
            ClearArea();
            HUDLog.Clear();
            InstanceSpecification.i.Header.CommonSettings.EnemyBlockDestroyedResourceDrop = (matconv / 100f);
            t1_res = maxmat;
            foreach (TournamentEntry item in entry_t1)
            {
                item.Spawn(spawndis, spawngap, entry_t1.Count, entry_t1.IndexOf(item));
                item.team_id.FactionInst().ResourceStore.SetResources(maxmat);
            }
            t2_res = maxmat;
            foreach (TournamentEntry item2 in entry_t2)
            {
                item2.Spawn(spawndis, spawngap, entry_t2.Count, entry_t2.IndexOf(item2));
                item2.team_id.FactionInst().ResourceStore.SetResources(maxmat);
            }
        }

		public unsafe void StartMatch()
		{
			timerTotal = 0f;
			timerTotal2 = Time.timeSinceLevelLoad;
			InstanceSpecification.i.Header.CommonSettings.ConstructableCleanUp = (ConstructableCleanUp)1;

			orbitindex = 0;
            orbittarget = 0;
            //orbittarget = StaticConstructablesManager.constructables.ToArray()[0].UniqueId;
            flycam.transform.position = (new Vector3(-500f, 50f, 0f));
            flycam.transform.rotation = (Quaternion.LookRotation(Vector3.right));
			cammode = false;
            foreach (MainConstruct constructable in StaticConstructablesManager.constructables)
			{
                /*Debug.Log("Startmatch ID: "+constructable.UniqueId);
                if (constructable.Drone.loadedMothershipC != null)
                {
                    Debug.Log("Startmatch mothership ID: " + constructable.Drone.loadedMothershipC.UniqueId);
                    constructable.GenUniqueID();
                    Debug.Log("Startmatch gen'd ID: " + constructable.UniqueId);
                    foreach (MainConstruct mother in from t in StaticConstructablesManager.constructables
                                                     where t.UniqueId == constructable.Drone.loadedMothershipC.UniqueId
                                                     select t)
                    {
                        Debug.Log("Startmatch adding: " + constructable.UniqueId + " to "+mother.uniqueID);
                        mother.Drone.myJustLoadedDrones.Clear();
                        mother.Drone.myJustLoadedDrones.Add(constructable);
                        mother.Drone.FirstFrameSetup();
                    }
                    
                }*/
                int id = 0;
                if (constructable.Drone.loadedMothershipC != null)
                {
                    id = constructable.Drone.loadedMothershipC.UniqueId;
                }
                string key = "" + constructable.UniqueId + "," + id;

                if (!HUDLog.ContainsKey((constructable.GetTeam()).Id))
				{
					HUDLog.Add((constructable.GetTeam()).Id, new SortedDictionary<string, TournamentParticipant>());
				}
                //constructable.GenUniqueID();
               
                if (!HUDLog[(constructable.GetTeam()).Id].ContainsKey(key))
				{
                    HUDLog[(constructable.GetTeam()).Id].Add(key, new TournamentParticipant
                    {
                        TeamId = constructable.GetTeam(),
                        TeamName = constructable.GetTeam().FactionSpec().AbreviatedName,
                        UniqueId = constructable.UniqueId,
                        MothershipId = id,
                        //BlueprintName = constructable.GetBlueprintName(),
                        BlueprintName = constructable.GetName(),
                        AICount = constructable.BlockTypeStorage.MainframeStore.Blocks.Count,
						HP = ((constructable.BlockTypeStorage.MainframeStore.Blocks.Count > 0) ? (constructable.iMainStatus.GetFractionAliveBlocksIncludingSubConstructables() * 100f) : (constructable.iMainStatus.GetFractionAliveBlocks() * 100f)),
						HPCUR = (float)((constructable.BlockTypeStorage.MainframeStore.Blocks.Count > 0) ? constructable.iMainStatus.GetNumberAliveBlocksIncludingSubConstructables() : constructable.iMainStatus.GetNumberAliveBlocks()),
						HPMAX = (float)((constructable.BlockTypeStorage.MainframeStore.Blocks.Count > 0) ? constructable.iMainStatus.GetNumberBlocksIncludingSubConstructables() : constructable.iMainStatus.GetNumberBlocks())
					});
				}
			}
            started = true;
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
			orbittarget = 0;
			orbitindex = 0;
			cammode = false;
            
		}

		public unsafe void OnGUI()
		{
			GUI.matrix = (Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f * (float)Screen.width / 1280f, 1f * (float)Screen.height / 800f, 1f)));
			GUI.backgroundColor = new Color(0f, 0f, 0f, 0.6f);
			GUI.Label(new Rect(590f, 0f, 100f, 30f), $"{Math.Floor((double)(timerTotal / 60f))}m {Math.Floor((double)timerTotal) % 60.0}s", _Top);
			foreach (KeyValuePair<int, SortedDictionary<string, TournamentParticipant>> item in HUDLog)
			{
				int num = 0;
				int id = entry_t1[0].team_id.Id;
				KeyValuePair<int, SortedDictionary<string, TournamentParticipant>> keyValuePair = item;
				bool flag = id == keyValuePair.Key;
				string text = "";
				int num2 = 1;
				SortedDictionary<int, SortedDictionary<string, TournamentParticipant>> hUDLog = HUDLog;
				KeyValuePair<int, SortedDictionary<string, TournamentParticipant>> keyValuePair2 = item;
				int count = hUDLog[keyValuePair2.Key].Values.Count;
				float num3 = 0f;
				float num4 = 0f;
				KeyValuePair<int, SortedDictionary<String, TournamentParticipant>> keyValuePair3 = item;
				foreach (KeyValuePair<string, TournamentParticipant> item2 in keyValuePair3.Value)
				{
					if (!item2.Value.Disqual && !item2.Value.Scrapping && item2.Value.AICount != 0)
					{
						num3 += item2.Value.HPCUR;
						num4 += item2.Value.HPMAX;
						text = ((!flag) ? (text + string.Format("\n{2} {1,4} {0,6}", Math.Floor((double)(item2.Value.OoBTime / 60f)) + "m" + Math.Floor((double)item2.Value.OoBTime) % 60.0 + "s", Math.Round((double)item2.Value.HP, 1) + "%", item2.Value.BlueprintName)) : (text + string.Format("\n{0,-6} {1,-4} {2}", Math.Floor((double)(item2.Value.OoBTime / 60f)) + "m" + Math.Floor((double)item2.Value.OoBTime) % 60.0 + "s", Math.Round((double)item2.Value.HP, 1) + "%", item2.Value.BlueprintName)));
					}
					else
					{
						num4 += item2.Value.HPMAX;
						text = ((!flag) ? (text + string.Format("\n{1}{0,16}", "DQ", item2.Value.BlueprintName)) : (text + string.Format("\n{0,-16}{1}", "DQ", item2.Value.BlueprintName)));
					}
					if (num2 == count)
					{
						if (flag)
						{
							GUI.Label(new Rect(0f, 0f, 200f, 38f + 16f * (float)count), string.Format("{0,-6} <color=#ffa500ff>{1,-4}</color> <color=cyan>{2}M</color>\n{3}", "Team 1", Math.Round((double)(num3 / num4 * 100f), 1) + "%", (object)FactionSpecifications.i.Factions.Find(delegate(FactionSpecificationFaction f)
							{
								
								int id3 = f.Id.Id;
								KeyValuePair<int, SortedDictionary<string, TournamentParticipant>> keyValuePair5 = item;
								return id3 == keyValuePair5.Key;
							}).InstanceOfFaction.ResourceStore.Material, text), _Left);
						}
						else
						{
							GUI.Label(new Rect(1080f, 0f, 200f, 38f + 16f * (float)count), string.Format("<color=cyan>{2}M</color> <color=#ffa500ff>{1,4}</color> {0,6}\n{3}", "Team 2", Math.Round((double)(num3 / num4 * 100f), 1) + "%", (object)FactionSpecifications.i.Factions.Find(delegate(FactionSpecificationFaction f)
							{
								int id2 = f.Id.Id;
								KeyValuePair<int, SortedDictionary<string, TournamentParticipant>> keyValuePair4 = item;
								return id2 == keyValuePair4.Key;
							}).InstanceOfFaction.ResourceStore.Material, text), _Right);
						}
					}
					checked
					{
						num2++;
						num++;
					}
				}
			}
		}

		public void LateUpdate()
		{
			
			if (Input.GetKeyUp((KeyCode)292))
			{
                //Time.set_timeScale((Time.get_timeScale() > 0f) ? 0f : 1f);
                Time.timeScale = ((Time.timeScale > 0f) ? 0f : 1f);
            }
			if (Input.GetAxis("Mouse ScrollWheel") != 0f)
			{
				if (Input.GetKey((KeyCode)303) || Input.GetKey((KeyCode)304))
				{
					orbitcam.distance = ((orbitcam.distance - Input.GetAxis("Mouse ScrollWheel") > 0f) ? (orbitcam.distance - Input.GetAxis("Mouse ScrollWheel") * 100f) : 0f);
				}
				else
				{
					orbitcam.distance = ((orbitcam.distance - Input.GetAxis("Mouse ScrollWheel") > 0f) ? (orbitcam.distance - Input.GetAxis("Mouse ScrollWheel") * 50f) : 0f);
				}
			}
            if (StaticConstructablesManager.constructables.ToArray()[orbitindex].UniqueId != orbittarget && orbittarget != 0)
            {      
                int index = StaticConstructablesManager.constructables.FindIndex(0, m => m.UniqueId == orbittarget);
                if(index >= 0) { orbitindex = index; }
                else { orbitindex = 0; }
            }
			checked
			{
				if (Input.GetKeyUp((KeyCode)101))
				{
					Debug.Log(unchecked((object)(StaticConstructablesManager.constructables.Count + " " + orbitindex)));
					if (orbitindex + 1 >= StaticConstructablesManager.constructables.Count)
					{
						orbitindex = 0;
						orbittarget = StaticConstructablesManager.constructables.ToArray()[orbitindex].UniqueId;
					}
					else
					{
						orbitindex++;
						orbittarget = StaticConstructablesManager.constructables.ToArray()[orbitindex].UniqueId;
					}
				}
				if (Input.GetKeyUp((KeyCode)113))
				{
					if (orbitindex == 0)
					{
						orbitindex = StaticConstructablesManager.constructables.Count - 1;
						orbittarget = StaticConstructablesManager.constructables.ToArray()[orbitindex].UniqueId;
					}
					else
					{
						orbitindex--;
						orbittarget = StaticConstructablesManager.constructables.ToArray()[orbitindex].UniqueId;
					}
				}
				if (Input.GetKeyUp((KeyCode)323))
				{
					flycam.enabled = false;
					orbitcam.enabled = true;
					cammode = true;
				}
				else if (Input.GetKeyUp((KeyCode)324))
				{
					orbitcam.enabled = false;
					flycam.enabled = true;
					flycam.transform.rotation = orbitcam.transform.rotation;
					cammode = false;
				}
				if (!cammode)
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
                    if (Input.GetKey((KeyCode)303) || Input.GetKey((KeyCode)304))
					{
						val = Vector3.Scale(val, new Vector3(5f, 5f, 5f)); // increase vector with shift
                    }
                    flycam.transform.position = flycam.transform.position + flycam.transform.localRotation * val;
                }
				else if (orbittarget != StaticConstructablesManager.constructables.ToArray()[orbitindex].UniqueId)
				{
					orbittarget = StaticConstructablesManager.constructables.ToArray()[orbitindex].UniqueId;
				}
				else
				{
                    //orbitcam.targetPos = StaticConstructablesManager.constructables.ToArray()[orbitindex].GameObject.FastPosition;
                    Vector3d position = new Vector3d(StaticConstructablesManager.constructables.ToArray()[orbitindex].CentreOfMass);
                    Quaternion rotation = StaticConstructablesManager.constructables.ToArray()[orbitindex].GameObject.transform.rotation;
                    orbitcam.OrbitTarget = new PositionAndRotationReturnUniverseCoord(new UniversalTransform(position,rotation));
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
                    if (val.Drone.loadedMothershipC != null)
                    {
                        id = val.Drone.loadedMothershipC.UniqueId;
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
							if (num > maxdis && num < num2)
							{
								HUDLog[(val.GetTeam()).Id][key].OoBTime += Time.timeSinceLevelLoad - timerTotal - timerTotal2;
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
                if (val.Drone.loadedMothershipC != null)
                {
                    id = val.Drone.loadedMothershipC.UniqueId;
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
                            if (c.Drone.loadedMothershipC != null)
                            {
                                id3 = c.Drone.loadedMothershipC.UniqueId;
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
                                if (c.Drone.loadedMothershipC != null)
                                {
                                    id2 = c.Drone.loadedMothershipC.UniqueId;
                                }
                                string uniqueId2 = "" + c.UniqueId + "," + id2;
                                KeyValuePair<string, TournamentParticipant> keyValuePair8 = item2;
								return uniqueId2 == keyValuePair8.Key;
							}
							return false;
						}).iMain.CentreOfMass;
						GameObject.Instantiate<GameObject>(Resources.Load("Detonator-MushroomCloud") as GameObject, centreOfMass, Quaternion.identity);
						StaticConstructablesManager.constructables.Find(delegate(MainConstruct c)
						{
							ObjectId team = c.GetTeam();
							KeyValuePair<string, TournamentParticipant> keyValuePair5 = item2;
							if (team == keyValuePair5.Value.TeamId)
							{
                                int id1 = 0;
                                if (c.Drone.loadedMothershipC != null)
                                {
                                    id1 = c.Drone.loadedMothershipC.UniqueId;
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
