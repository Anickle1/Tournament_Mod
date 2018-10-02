using BrilliantSkies.Ftd.Planets;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.Factions;
using BrilliantSkies.Ftd.Planets.Instances.Headers;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Modding;
using System;
using UnityEngine;

namespace Tournament
{
	public class TournamentPlugin : GamePlugin
	{
		private static Tournament _t;

		private static InstanceSpecification @is;

		public string name => "Tournament";

		public static string Name => "Tournament";

		public Version version => new Version("0.1.0");

		public unsafe void OnLoad()
		{
			_t = new Tournament();
            GameEvents.StartEvent += OnInstanceChange;
            GameEvents.UniverseChange += OnPlanetChange;
        }

		public void OnSave()
		{
		}

		public unsafe static void OnInstanceChange()
		{
            GameEvents.FixedUpdateEvent -= _t._me.FixedUpdate;
            GameEvents.OnGui -= _t._me.OnGUI;
            GameEvents.Twice_Second -= _t._me.SlowUpdate;
            GameEvents.PreLateUpdate -= _t._me.LateUpdate;
            GameEvents.UpdateEvent -= _t.UpdateBoardSectionPreview;
            if (@is.Header.Name == InstanceSpecification.i.Header.Name)
			{
				_t._me._GUI.ActivateGui(_t._me, 0);
			}
		}

		public static void OnPlanetChange()
		{
			@is = new InstanceSpecification();
			@is.Header.Name = "Tournament Creator";
			@is.Header.Summary = "Create custom tournament style matches.";
            @is.Header.Type = (InstanceType)4;
			@is.Header.CommonSettings.AvatarAvailability = (AvatarAvailability)1;
			@is.Header.CommonSettings.AvatarDamage = (AvatarDamage)1;
			@is.Header.CommonSettings.ConstructableCleanUp = 0;
			@is.Header.CommonSettings.HeartStoneRequirement = 0;
			@is.Header.CommonSettings.BuildModeRules = (BuildModeRules)1;
			@is.Header.CommonSettings.SavingOptions = 0;
			@is.Header.CommonSettings.BlueprintSpawningOptions = (BlueprintSpawningOptions)5;
			@is.Header.CommonSettings.EnemyBlockDestroyedResourceDrop = 0f;
			@is.Header.CommonSettings.LocalisedResourceMode = 0;
			@is.Header.CommonSettings.FogOfWarType = (FogOfWarType)1;
			@is.Header.CommonSettings.DesignerOptions = 0;
			@is.Header.CommonSettings.LuckyMechanic = (LuckyMechanic)1;
			Planet.i.Designers.AddInstance(@is);
			FactionSpecifications i = FactionSpecifications.i;
			FactionSpecificationFaction val = new FactionSpecificationFaction();
			val.Name = "KING";
			val.AbreviatedName = "KING";
			val.FleetColors = (Color[])new Color[4]
			{
				new Color(1f, 0.843137264f, 0f, 0.5f),
				new Color(0.854901969f, 0.647058845f, 0.1254902f, 0.5f),
				new Color(1f, 0.647058845f, 0f, 0.5f),
				new Color(0.854901969f, 0.549019635f, 0f, 0.5f)
			};
			i.AddNew(val);
			FactionSpecifications i2 = FactionSpecifications.i;
			FactionSpecificationFaction val2 = new FactionSpecificationFaction();
			val2.Name = "CHAL";
			val2.AbreviatedName = "CHAL";
			val2.FleetColors = (Color[])new Color[4]
			{
				new Color(1f, 0f, 0f, 0.5f),
				new Color(0.545098066f, 0f, 0f, 0.5f),
				new Color(0.698039234f, 0.13333334f, 0.13333334f, 0.5f),
				new Color(1f, 0.3882353f, 0.2784314f, 0.5f)
			};
			i2.AddNew(val2); //get id ?
		}
    }
}
