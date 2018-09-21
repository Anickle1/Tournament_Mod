using Assets.Scripts;
using Assets.Scripts.Persistence;
using BrilliantSkies.Core.Id;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.Instances.Factions;
using System;
using System.Collections.Generic;
using UnityEngine;
using BrilliantSkies.Core.UniverseRepresentation;
namespace Tournament
{
	public class TournamentEntry
	{
		private BlueprintFile _bpf;

		public Blueprint bp;

		public bool IsKing
		{
			get;
			set;
		}

		public Tournament.SPAWN.DIR spawn_direction
		{
			get;
			set;
		}

		public Tournament.SPAWN.LOC spawn_location
		{
			get;
			set;
		}

		public ObjectId team_id
		{
			get;
			set;
		}

		public float res
		{
			get;
			set;
		}

        public float offset
        {
            get;
            set;
        }

        public BlueprintFile bpf
		{
			get
			{
				return _bpf;
			}
			set
			{
				_bpf = value;
				bp = bpf.Load(true);
			}
		}

		public unsafe string[] labelCost
		{
			get
			{
				if (bp != null)
				{
					List<Blueprint> list = bp.SCs.FindAll((Blueprint x) => !x.IsSubConstructable());
					int count = list.Count;
					if (count > 0)
					{
						string[] array = new string[checked(count + 1)];
						float num = 0f;
						for (int i = 0; i < count; i = checked(i + 1))
						{
							float material = (list[i].CalculateResourceCost(false, true)).Material;
							array[checked(i + 1)] = $"{list[i].blueprintName} <color=cyan>{material}</color>";
							num += material;
						}
						array[0] = $"{bp.blueprintName} <color=cyan>{(bp.CalculateResourceCost(false, true)).Material - num}</color>";
						return array;
					}
					return new string[1]
					{
						$"{bp.blueprintName} <color=cyan>{(bp.CalculateResourceCost(false, true)).Material}</color>"
					};
				}
				return null;
			}
		}

		public unsafe string[] label
		{
			get
			{
				if (bp != null)
				{
					List<Blueprint> list = bp.SCs.FindAll((Blueprint x) => !x.IsSubConstructable());
					int count = list.Count;
					if (count > 0)
					{
						string[] array = new string[checked(count + 1)];
						float num = 0f;
						float num2 = 0f;
						for (int i = 0; i < count; i = checked(i + 1))
						{
							float material = (list[i].CalculateResourceCost(false, true)).Material;
							float material2 = (list[i].CalculateResourceCost(true, true)).Material;
							array[checked(i + 1)] = $"{list[i].blueprintName} {Math.Round((double)(material2 / material * 100f), 1)}";
							num += material;
							num2 += material2;
						}
						array[0] = $"{bp.blueprintName} {Math.Round((double)(((bp.CalculateResourceCost(true, true)).Material - num2) / ((bp.CalculateResourceCost(false, true)).Material - num) * 100f), 1)}";
						return array;
					}
					return new string[1]
					{
						$"{bp.blueprintName} {Math.Round((double)((bp.CalculateResourceCost(true, true)).Material / (bp.CalculateResourceCost(false, true)).Material * 100f), 1)}"
					};
				}
				return null;
			}
		}

		public void Spawn(float dis, float gap, float gap2, int count, int pos)
		{
            MainConstruct val = BlueprintConverter.Convert(bp, 0, true);
            /*val.Drone.myJustLoadedDrones.Clear();
            foreach (MainConstruct m in val.MainArrayBasics.SVList)
            {
                m.GenUniqueID();
                val.Drone.myJustLoadedDrones.Add(m);
            }*/
            team_id = (IsKing ? InstanceSpecification.i.Factions.Factions.Find((InstanceFaction f) => f.FactionSpec.Name == "KING").Id : 
                InstanceSpecification.i.Factions.Factions.Find((InstanceFaction f) => f.FactionSpec.Name == "CHAL").Id);
            //BrilliantSkies.Core.Types.Vector3d vector3D = new BrilliantSkies.Core.Types.Vector3d(VLoc(gap, count, pos, dis));
            //vector3D.y += offset;
            BlueprintConverter.Initiate(val, PlanetList.MainFrame.FramePositionToUniversalPosition(VLoc(gap, gap2, count, pos, dis, offset)), VDir(), team_id, null, 0);
        }

        public Vector3 VLoc(float gap, float gap2, int count, int pos, float dis, float offset)
		{
            float x = (count - 1f) * gap / 2f - pos * gap;
            float z = IsKing ? (dis / 2f) + (pos*gap2) : (dis / 2f - dis) - (pos*gap2);
			
			switch (spawn_location)
			{
			case Tournament.SPAWN.LOC.Sea:
				return new Vector3(x, 1f + offset, z);
			case Tournament.SPAWN.LOC.Air:
				return new Vector3(x, 100f + offset, z);
			case Tournament.SPAWN.LOC.Sub:
				return new Vector3(x, -20f + offset, z);
			case Tournament.SPAWN.LOC.Land:
				return new Vector3(x, 51f + offset, z);
			default:
				return new Vector3(x, 0f + offset, z);
			}
		}

		public Quaternion VDir()
		{
			
			switch (spawn_direction)
			{
			case Tournament.SPAWN.DIR.Facing:
				return Quaternion.LookRotation(new Vector3(0f, 0f, (float)((!IsKing) ? 1 : (-1))));
			case Tournament.SPAWN.DIR.Away:
				return Quaternion.LookRotation(new Vector3(0f, 0f, (float)(IsKing ? 1 : (-1))));
			case Tournament.SPAWN.DIR.Left:
				return Quaternion.LookRotation(new Vector3((float)(IsKing ? 1 : (-1)), 0f, 0f));
			case Tournament.SPAWN.DIR.Right:
				return Quaternion.LookRotation(new Vector3((float)((!IsKing) ? 1 : (-1)), 0f, 0f));
			default:
				return Quaternion.LookRotation(new Vector3(0f, 0f, (float)((!IsKing) ? 1 : (-1))));
			}
		}
	}
}
