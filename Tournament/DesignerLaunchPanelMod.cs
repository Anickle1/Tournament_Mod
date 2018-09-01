using Assets.Scripts.Gui;
using Assets.Scripts.Persistence;
using BrilliantSkies.Core.Id;
using BrilliantSkies.Core.Text;
using BrilliantSkies.Core.UiSounds;
using BrilliantSkies.FromTheDepths.Game;
using BrilliantSkies.Ftd.Planets;
using BrilliantSkies.Ftd.Planets.Instances;
using BrilliantSkies.Ftd.Planets.Instances.Headers;
using BrilliantSkies.ScriptableObjects;
using BrilliantSkies.Ui.Displayer;
using BrilliantSkies.Ui.Displayer.Types;
using BrilliantSkies.Ui.Layouts;
using BrilliantSkies.Ui.Tips;
using BrilliantSkies.Ui.TreeSelection;
using System;
using UnityEngine;

namespace Tournament
{
    public class DesignerLaunchPanelMod : ThrowAwayGui
    {
        private SO_CampaignLaunch SO = null;

        private TreeSelectorGuiElement<PlanetFile, PlanetFolder> _treeSelector = null;

        private Planet _selectedPlanet = null;

        private bool _selectOtherPlanets = false;

        private int _selectedInstance = -1;

        private Vector2 _previewScroll = new Vector2(0f, 0f);

        public void SelectionUpdated(PlanetFile file)
        {
            if (_treeSelector.CurrentData != null)
            {
                _selectedInstance = -1;
                try
                {
                    _selectedPlanet = _treeSelector.CurrentData.LoadPlanetWithHeaderAndDesigners();
                    InstanceSpecification @is = new InstanceSpecification();
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
                    _selectedPlanet.Designers.AddInstance(@is);
                    if (_selectedPlanet.Designers.InstanceCount > 0)
                    {
                        _selectedInstance = 0;
                    }
                }
                catch
                {
                    GUISoundManager.GetSingleton().PlayFailure();
                    _selectedPlanet = null;
                }
            }
            else
            {
                _selectedPlanet = null;
            }
        }

        public override void SetGuiSettings()
        {
            base.GuiSettings.PausesPlay = true;
        }

        public override void OnActivateGui()
        {
            BrilliantSkies.Ui.Special.Overlays.Console.WriteToConsole("loading");
            SO = LazyLoader.CampaignLaunch.Get();
            if (_treeSelector == null)
            {
                _treeSelector = FtdGuiUtils.GetFileBrowserFor(GameFolders.GetCombinedPlanetFolder());
                _treeSelector.GoToMatchingSub((PlanetFolder t) => string.Equals(t.Name, "Built In", StringComparison.CurrentCultureIgnoreCase));
            }
            _selectOtherPlanets = false;
            _selectedPlanet = Planet.i;
        }

        public override void OnDeactivateGui()
        {
            _selectOtherPlanets = false;
        }

        public override void OnGui()
        {
            if (!_selectOtherPlanets)
            {
                Gui_DisplaySinglePlanet();
            }
            else
            {
                Gui_DisplayMultiPlanet();
            }
            DisplayPreviewPanel();
            GUI.Label(SO.R_ExperienceMultiplierDesigner, string.Format("Experience gain multiplier is <b>5%!</b> in designer!", Array.Empty<object>()), SO.STYLE_ExperienceMultiplier);
        }

        public void DisplayPreviewPanel()
        {
            if (_selectedPlanet != null)
            {
                Rect r_PreviewPanelDesigner = SO.R_PreviewPanelDesigner;
                GUILayout.BeginArea(r_PreviewPanelDesigner);
                GUILayout.BeginVertical(_selectedPlanet.File.Header.Name, GUI.skin.window, Array.Empty<GUILayoutOption>());
                GUILayout.Space(40f);
                _previewScroll = GUILayout.BeginScrollView(_previewScroll, Array.Empty<GUILayoutOption>());
                GUILayout.Box(_selectedPlanet.File.Header.Summary, SO.PlanetName, Array.Empty<GUILayoutOption>());
                if (MapEditorGuiCommon.InstanceForSelection("Vanilla", new ToolTip("Launch the default designer, that comes with every planet"), _selectedInstance == -1))
                {
                    _selectedInstance = -1;
                }
                for (int i = 0; i < _selectedPlanet.Designers.InstanceCount; i++)
                {
                    InstanceSpecification instanceSpecification = _selectedPlanet.Designers.Instances[i];
                    bool flag = i == _selectedInstance;
                    if (MapEditorGuiCommon.InstanceForSelection(instanceSpecification.Header.Name, new ToolTip(instanceSpecification.Header.Summary), flag))
                    {
                        _selectedInstance = (flag ? (-1) : i);
                    }
                }
                if (_selectedInstance != -1)
                {
                    InstanceSpecification instanceSpecification2 = _selectedPlanet.Designers.Instances[_selectedInstance];
                    GUILayout.Box(instanceSpecification2.Header.Summary, SO.Summary, Array.Empty<GUILayoutOption>());
                    foreach (HeaderAndParagraph descriptionParagraph in instanceSpecification2.Header.DescriptionParagraphs)
                    {
                        GUILayout.Label(descriptionParagraph.Header, SO.Header, Array.Empty<GUILayoutOption>());
                        GUILayout.Box(descriptionParagraph.Paragraph, SO.Paragraph, Array.Empty<GUILayoutOption>());
                    }
                }
                GUILayout.EndScrollView();
                GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                if (_selectedInstance != -1)
                {
                    if (GUILayout.Button($"Launch <color=blue>{_selectedPlanet.Designers.Instances[_selectedInstance].Header.Name}</color>!", "inventorymultichoice", GUILayout.ExpandHeight(false)))
                    {
                        ModeChanger.Instance.SwitchToVehicleDesigner(new DesignerLoadRequest
                        {
                            Planet = _selectedPlanet,
                            DesignerId = _selectedPlanet.Designers.Instances[_selectedInstance].Header.Id
                        });
                        _selectedPlanet = null;
                    }
                }
                else if (GUILayout.Button(new GUIContent("Launch <color=blue>Vanilla Designer</color>!", "vanilla"), "inventorymultichoice", GUILayout.ExpandHeight(false)))
                {
                    ModeChanger.Instance.SwitchToVehicleDesigner(new DesignerLoadRequest
                    {
                        Planet = _selectedPlanet,
                        DesignerId = ObjectId.NoLinkage
                    });
                    _selectedPlanet = null;
                }
                if (GUI.tooltip == "vanilla")
                {
                    TipDisplayer.Instance.SetTip(new ToolTip("A simple, automatically generated instance of the vehicle designer that features a small raft and a respawn beacon."));
                }
                GUILayout.EndHorizontal();
                if (GuiCommon.DisplayCloseButton((int)r_PreviewPanelDesigner.width))
                {
                    DeactivateGui(GuiDeactivateType.Standard);
                }
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndArea();
            }
        }

        private void Gui_DisplaySinglePlanet()
        {
            GUILayout.BeginArea(SO.R_CampaignSelectPanelBlerb, "Designer Selector", GUI.skin.window);
            GUILayout.Box($"You are currently on the Planet of <color=cyan>{_selectedPlanet.File.Header.Name}</color>. Click below to browse for any community made planets you might have installed in your profile folder.\n Saved Games are specific to the current planet", Array.Empty<GUILayoutOption>());
            if (GUILayout.Button(new GUIContent("Click here to allow selection of other planets"), Array.Empty<GUILayoutOption>()))
            {
                _selectOtherPlanets = true;
            }
            if (GuiCommon.DisplayCloseButton((int)SO.R_CampaignSelectPanel.width))
            {
                DeactivateGui(GuiDeactivateType.Standard);
            }
            GUILayout.EndArea();
        }

        private void Gui_DisplayMultiPlanet()
        {
            GUILayout.BeginArea(SO.R_CampaignSelectPanel, "Planet Selector", GUI.skin.window);
            _treeSelector.OnGui(SO.R_TreeSelectPanel, SelectionUpdated);
            if (GuiCommon.DisplayCloseButton((int)SO.R_CampaignSelectPanel.width))
            {
                DeactivateGui(GuiDeactivateType.Standard);
            }
            GUILayout.EndArea();
        }


    }
}
