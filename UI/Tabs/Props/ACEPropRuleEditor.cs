﻿using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.AssetColorExpander.Data;
using Klyte.AssetColorExpander.Libraries;
using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Extensors;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Klyte.Commons.UI.DefaultEditorUILib;

namespace Klyte.AssetColorExpander.UI
{

    public class ACEPropRuleEditor : UICustomControl
    {
        public UIPanel MainContainer { get; protected set; }

        private const string DISTRICT_SELECTOR_TEMPLATE = "K45_ACE_DistrictSelectorTemplate";
        private const string COLOR_SELECTOR_TEMPLATE = "K45_ACE_ColorListSelectorTemplate";
        private int m_currentIdx = -1;

        private UITabstrip m_tabstrip;

        private UITextField m_name;

        private UIButton m_copySettings;
        private UIButton m_pasteSettings;

        private UIDropDown m_ruleFilter;
        private UIDropDown m_service;
        private UIDropDown m_subService;
        private UIDropDown m_level;
        private UIDropDown m_class;
        private UIDropDown m_parentClass;
        private UITextField m_assetFilter;
        private UIListBox m_popup;
        private UITextField m_assetFilterBuilding;
        private UIListBox m_popupBuilding;
        private UITextField m_assetFilterNet;
        private UIListBox m_popupNet;


        private UIDropDown m_colorMode;
        private UIPanel m_listColorContainer;
        private UIScrollablePanel m_colorListScroll;
        private UITemplateList<UIPanel> m_colorFieldTemplateListColors;
        private UIButton m_addColor;
        private UICheckBox m_allowRed;
        private UICheckBox m_allowGreen;
        private UICheckBox m_allowBlues;
        private UICheckBox m_allowNeutral;

        private UICheckBox m_districtWhiteList;
        private UICheckBox m_districtBlackList;
        private UIDropDown m_districtResolutionOrder;
        private UIPanel m_listContainer;
        private UIScrollablePanel m_districtList;
        private UITemplateList<UIPanel> m_checkboxTemplateListDistrict;



        public void Awake()
        {
            MainContainer = GetComponent<UIPanel>();
            MainContainer.autoLayout = true;
            MainContainer.clipChildren = true;
            MainContainer.autoLayoutDirection = LayoutDirection.Vertical;
            MainContainer.autoLayoutPadding = new RectOffset(0, 0, 4, 4);

            KlyteMonoUtils.CreateTabsComponent(out m_tabstrip, out UITabContainer m_tabContainer, MainContainer.transform, "TextEditor", new Vector4(0, 0, MainContainer.width, 40), new Vector4(0, 0, MainContainer.width, MainContainer.height - 40));
            UIPanel m_tabSettings = TabCommons.CreateNonScrollableTabLocalized(m_tabstrip, KlyteResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.K45_Settings), "K45_ACE_BASICTAB_BASIC_SETTINGS", "RcSettings");
            UIPanel m_tabAppearence = TabCommons.CreateNonScrollableTabLocalized(m_tabstrip, KlyteResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.K45_AutoColorIcon), "K45_ACE_BASICTAB_APPEARANCE_SETTINGS", "RcAppearence");
            UIPanel m_tabDistricts = TabCommons.CreateNonScrollableTabLocalized(m_tabstrip, "ToolbarIconDistrict", "K45_ACE_BASICTAB_DISTRICT_SETTINGS", "RcDistricts");
            UIPanel m_tabLib = TabCommons.CreateNonScrollableTabLocalized(m_tabstrip, KlyteResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.K45_Load), "K45_ACE_BASICTAB_LIB_SETTINGS", "RcLib");

            var helperSettings = new UIHelperExtension(m_tabSettings, LayoutDirection.Vertical);
            var helperAppearence = new UIHelperExtension(m_tabAppearence, LayoutDirection.Vertical);
            var helperDistricts = new UIHelperExtension(m_tabDistricts, LayoutDirection.Vertical);
            var helperLib = new UIHelperExtension(m_tabLib, LayoutDirection.Vertical);


            AddTextField(Locale.Get("K45_ACE_BASICTAB_NAME"), out m_name, helperSettings, OnSetName);
            helperSettings.AddSpace(5);

            AddDropdown(Locale.Get("K45_ACE_BASICTAB_RULEFILTER"), out m_ruleFilter, helperSettings, Enum.GetNames(typeof(RuleCheckTypeProp)).Select(x => Locale.Get("K45_ACE_RULECHECKTYPE", x)).ToArray(), OnChangeRuleCheckType);
            AddButtonInEditorRow(m_ruleFilter, CommonsSpriteNames.K45_QuestionMark, Help_RuleFilter);
            AddDropdown(Locale.Get("K45_ACE_BASICTAB_SERVICEFILTER"), out m_service, helperSettings, (Enum.GetValues(typeof(ItemClass.Service)) as ItemClass.Service[]).OrderBy(x => (int)x).Select(x => x == 0 ? Locale.Get("K45_ACE_ANYSERVICE_OPTION") : $"{x}").ToArray(), OnChangeServiceFilter);
            AddDropdown(Locale.Get("K45_ACE_BASICTAB_SUBSERVICEFILTER"), out m_subService, helperSettings, Enum.GetNames(typeof(ItemClass.SubService)).Select(x => $"{x}").ToArray(), OnChangeSubServiceFilter);
            AddDropdown(Locale.Get("K45_ACE_BASICTAB_LEVELFILTER"), out m_level, helperSettings, (Enum.GetValues(typeof(ItemClass.Level)) as ItemClass.Level[]).OrderBy(x => (int)x).Select(x => $"{x}").ToArray(), OnChangeLevelFilter);
            AddDropdown(Locale.Get("K45_ACE_BASICTAB_CLASSFILTER"), out m_class, helperSettings, new string[0], OnChangeClassFilter);
            AddDropdown(Locale.Get("K45_ACE_BASICTAB_CLASSFILTER"), out m_parentClass, helperSettings, new string[0], OnChangeParentClassFilter);
            AddTextField(Locale.Get("K45_ACE_PROPRULES_ASSETSELECT_BUILDING"), out m_assetFilterBuilding, helperSettings, null);

            KlyteMonoUtils.UiTextFieldDefaultsForm(m_assetFilterBuilding);
            m_popupBuilding = ConfigureListSelectionPopupForUITextField(m_assetFilterBuilding, () => AssetColorExpanderMod.Controller?.FilterBuildingsByText(m_assetFilterBuilding.text), OnAssetSelectedBuildingChanged, GetCurrentSelectionNameBuilding);
            m_popupBuilding.height = 290;
            m_popupBuilding.width -= 20;

            AddTextField(Locale.Get("K45_ACE_PROPRULES_ASSETSELECT_NET"), out m_assetFilterNet, helperSettings, null);

            KlyteMonoUtils.UiTextFieldDefaultsForm(m_assetFilterNet);
            m_popupNet = ConfigureListSelectionPopupForUITextField(m_assetFilterNet, () => AssetColorExpanderMod.Controller?.FilterNetsByText(m_assetFilterNet.text), OnAssetSelectedNetChanged, GetCurrentSelectionNameNet);
            m_popupNet.height = 290;
            m_popupNet.width -= 20;

            AddTextField(Locale.Get("K45_ACE_PROPRULES_ASSETSELECT"), out m_assetFilter, helperSettings, null);

            KlyteMonoUtils.UiTextFieldDefaultsForm(m_assetFilter);
            m_popup = ConfigureListSelectionPopupForUITextField(m_assetFilter, () => AssetColorExpanderMod.Controller?.FilterPropsByText(m_assetFilter.text), OnAssetSelectedChanged, GetCurrentSelectionName);
            m_popup.height = 290;
            m_popup.width -= 20;

            AddLibBox<ACEPropRuleLib, PropCityDataRuleXml>(helperLib, out m_copySettings, OnCopyRule, out m_pasteSettings, OnPasteRule, out _, null, OnLoadRule, GetRuleSerialized);

            AddDropdown(Locale.Get("K45_ACE_COLORMODE"), out m_colorMode, helperAppearence, Enum.GetNames(typeof(ColoringMode)).Select(x => Locale.Get("K45_ACE_COLORINGMODE", x)).ToArray(), OnChangeColoringMode);
            AddButtonInEditorRow(m_colorMode, CommonsSpriteNames.K45_QuestionMark, Help_ColorMode);

            AddCheckboxLocale("K45_ACE_COLORMODE_ALLOWREDTONES", out m_allowRed, helperAppearence, OnAllowRedChanged);
            AddCheckboxLocale("K45_ACE_COLORMODE_ALLOWGREENTONES", out m_allowGreen, helperAppearence, OnAllowGreenChanged);
            AddCheckboxLocale("K45_ACE_COLORMODE_ALLOWBLUETONES", out m_allowBlues, helperAppearence, OnAllowBlueChanged);
            AddCheckboxLocale("K45_ACE_COLORMODE_ALLOWNEUTRALTONES", out m_allowNeutral, helperAppearence, OnAllowNeutralChanged);


            KlyteMonoUtils.CreateUIElement(out m_listColorContainer, helperAppearence.Self.transform, "listColors", new UnityEngine.Vector4(0, 0, helperAppearence.Self.width, helperAppearence.Self.height - 80));
            KlyteMonoUtils.CreateScrollPanel(m_listColorContainer, out m_colorListScroll, out _, m_listColorContainer.width - 20, m_listColorContainer.height);
            m_colorListScroll.backgroundSprite = "OptionsScrollbarTrack";
            m_colorListScroll.autoLayout = true;
            m_colorListScroll.autoLayoutDirection = LayoutDirection.Horizontal;
            m_colorListScroll.wrapLayout = true;
            CreateTemplateColorItem();
            m_colorFieldTemplateListColors = new UITemplateList<UIPanel>(m_colorListScroll, COLOR_SELECTOR_TEMPLATE);

            KlyteMonoUtils.InitCircledButton(m_colorListScroll, out m_addColor, CommonsSpriteNames.K45_Plus, (x, y) => AddColor(), "", 36);


            AddCheckboxLocale("K45_ACE_BASICTAB_DISTRICTSELECTIONASWHITELIST", out m_districtWhiteList, helperDistricts, OnSetDistrictsAsWhitelist);
            AddCheckboxLocale("K45_ACE_BASICTAB_DISTRICTSELECTIONASBLACKLIST", out m_districtBlackList, helperDistricts, OnSetDistrictsAsBlacklist);
            AddDropdown(Locale.Get("K45_ACE_BASICTAB_DISTRICTRESTRICTIONSOLVEORDER"), out m_districtResolutionOrder, helperDistricts, Enum.GetNames(typeof(DistrictRestrictionOrder)).Select(x => Locale.Get("K45_ACE_DISTRICTRESTRICTIONORDER", x)).ToArray(), OnChangeDistrictRestrictionOrder);
            AddButtonInEditorRow(m_districtResolutionOrder, CommonsSpriteNames.K45_QuestionMark, Help_DistrictFilter);
            KlyteMonoUtils.CreateUIElement(out m_listContainer, helperDistricts.Self.transform, "previewPanel", new UnityEngine.Vector4(0, 0, helperDistricts.Self.width, helperDistricts.Self.height - 160));
            KlyteMonoUtils.CreateScrollPanel(m_listContainer, out m_districtList, out _, m_listContainer.width - 20, m_listContainer.height);
            m_districtList.backgroundSprite = "OptionsScrollbarTrack";
            m_districtList.autoLayout = true;
            m_districtList.autoLayoutDirection = LayoutDirection.Vertical;

            CreateTemplateDistrict();
            m_checkboxTemplateListDistrict = new UITemplateList<UIPanel>(m_districtList, DISTRICT_SELECTOR_TEMPLATE);


            MainContainer.isVisible = false;
            m_pasteSettings.isVisible = false;
        }

        private void Help_DistrictFilter() => K45DialogControl.ShowModalHelp("General.DistrictFilter", Locale.Get("K45_ACE_PROPRULES_DISTRICTFILTER"), 0);
        private void Help_ColorMode() => K45DialogControl.ShowModalHelp("Prop.ColoringMode", Locale.Get("K45_ACE_PROPRULES_COLORMODE"), 0);
        private void Help_RuleFilter() => K45DialogControl.ShowModalHelp("Prop.TypeOfRule", Locale.Get("K45_ACE_PROPRULES_RULEFILTER"), 0);
        private void AddColor() => SafeObtain((ref PropCityDataRuleXml x) =>
        {
            x.m_colorList.Add(Color.white);
            UpdateColorList(ref x);
        });
        public void Start()
        {
            m_class.items = AssetColorExpanderMod.Controller?.AllClassesProp?.Keys?.Select(x => x.name)?.OrderBy(x => x)?.ToArray() ?? new string[0];
            m_parentClass.items = AssetColorExpanderMod.Controller?.AllClassesBuilding?.Keys?.Union(AssetColorExpanderMod.Controller?.AllClassesNet?.Keys).Select(x => x.name)?.Distinct()?.OrderBy(x => x)?.ToArray() ?? new string[0];
            ACEPanel.Instance.PropTab.RuleList.EventSelectionChanged += OnChangeTab;
        }

        #region Prefab handling
        private string GetCurrentSelectionName()
        {
            string name = "";
            SafeObtain((ref PropCityDataRuleXml x) => name = x.AssetNameSelf);
            return name ?? "";
        }
        private string GetCurrentSelectionNameBuilding()
        {
            string name = "";
            SafeObtain((ref PropCityDataRuleXml x) => name = x.AssetNameBuilding);
            return name ?? "";
        }
        private string GetCurrentSelectionNameNet()
        {
            string name = "";
            SafeObtain((ref PropCityDataRuleXml x) => name = x.AssetNameNet);
            return name ?? "";
        }
        #endregion

        #region District
        private void UpdateDistrictList(ref PropCityDataRuleXml reference)
        {
            var districts = DistrictUtils.GetValidParks().ToDictionary(x => x.Key, x => 0x100 | x.Value).Union(DistrictUtils.GetValidDistricts()).OrderBy(x => x.Value == 0 ? "" : x.Key).ToDictionary(x => x.Key, x => x.Value);
            ref DistrictPark[] parkBuffer = ref Singleton<DistrictManager>.instance.m_parks.m_buffer;
            UIPanel[] districtChecks = m_checkboxTemplateListDistrict.SetItemCount(districts.Count);

            for (int i = 0; i < districts.Count; i++)
            {
                string districtName = districts.Keys.ElementAt(i);
                UICheckBox checkbox = districtChecks[i].GetComponentInChildren<UICheckBox>();
                checkbox.stringUserData = districts[districtName].ToString();
                if (checkbox.label.objectUserData == null)
                {
                    checkbox.eventCheckChanged += (x, y) =>
                    {
                        SafeObtain((ref PropCityDataRuleXml z) =>
                        {
                            if (ushort.TryParse(x.stringUserData, out ushort districtIdx))
                            {
                                if (y)
                                {
                                    z.SelectedDistricts.Add(districtIdx);
                                }
                                else
                                {
                                    z.SelectedDistricts.Remove(districtIdx);
                                }
                            }
                        });

                    };
                    KlyteMonoUtils.LimitWidthAndBox(checkbox.label, m_districtList.width - 50);


                    checkbox.label.objectUserData = true;
                }
                checkbox.text = districtName;
                if (districts[districtName] >= 256)
                {
                    int parkId = districts[districtName] & 0xFF;
                    if (parkBuffer[parkId].IsCampus)
                    {
                        checkbox.tooltip = Locale.Get("MAIN_TOOL", "PlayerEducation");
                        checkbox.label.textColor = Color.cyan;
                    }
                    else if (parkBuffer[parkId].IsIndustry)
                    {
                        checkbox.tooltip = Locale.Get("PARKSOVERVIEW_TOOLTIP", "Industry");
                        checkbox.label.textColor = Color.yellow;
                    }
                    else if (parkBuffer[parkId].IsPark)
                    {
                        checkbox.tooltip = Locale.Get("PARKSOVERVIEW_TOOLTIP", "Generic");
                        checkbox.label.textColor = Color.green;
                    }
                    else
                    {
                        checkbox.tooltip = Locale.Get("MAIN_AREAS");
                        checkbox.label.textColor = Color.Lerp(Color.magenta, Color.blue, 0.5f);
                    }
                }
                else
                {
                    checkbox.tooltip = Locale.Get("TUTORIAL_ADVISER_TITLE", "District");
                    checkbox.label.textColor = Color.white;
                }

            }
            for (int i = 0; i < m_checkboxTemplateListDistrict.items.Count; i++)
            {
                UICheckBox checkbox = m_checkboxTemplateListDistrict.items[i].GetComponentInChildren<UICheckBox>();
                if (ushort.TryParse(checkbox.stringUserData, out ushort districtIdx))
                {
                    checkbox.isChecked = reference.SelectedDistricts.Contains(districtIdx);
                }
            }
        }

        private void CreateTemplateDistrict()
        {
            if (!UITemplateUtils.GetTemplateDict().ContainsKey(DISTRICT_SELECTOR_TEMPLATE))
            {
                var go = new GameObject();
                UIPanel panel = go.AddComponent<UIPanel>();
                panel.size = new Vector2(m_districtList.width, 36);
                panel.autoLayout = true;
                panel.wrapLayout = false;
                panel.autoLayoutDirection = LayoutDirection.Horizontal;

                UICheckBox uiCheckbox = UIHelperExtension.AddCheckbox(panel, "AAAAAA", false);
                uiCheckbox.name = "AssetCheckbox";
                uiCheckbox.height = 29f;
                uiCheckbox.width = 290f;
                uiCheckbox.label.processMarkup = true;
                uiCheckbox.label.textScale = 0.8f;

                UITemplateUtils.GetTemplateDict()[DISTRICT_SELECTOR_TEMPLATE] = panel;
            }
        }
        #endregion

        #region ColorList

        private bool m_isLoadingColors = false;
        private void UpdateColorList(ref PropCityDataRuleXml reference)
        {
            UIPanel[] colorPickers = m_colorFieldTemplateListColors.SetItemCount(reference.m_colorList.Count);
            m_isLoadingColors = true;
            for (int i = 0; i < reference.m_colorList.Count; i++)
            {
                UIColorField colorField = colorPickers[i].GetComponentInChildren<UIColorField>();
                if (colorField.objectUserData == null)
                {
                    colorField.eventSelectedColorChanged += (x, y) =>
                        SafeObtain((ref PropCityDataRuleXml z) =>
                            {
                                if (!m_isLoadingColors && z.m_colorList.Count > x.parent.zOrder)
                                {
                                    m_isLoadingColors = true;
                                    if (y == default)
                                    {
                                        z.m_colorList.RemoveAt(x.parent.zOrder);
                                        UpdateColorList(ref z);
                                    }
                                    else
                                    {
                                        z.m_colorList[x.parent.zOrder] = y;
                                    }
                                    m_isLoadingColors = false;
                                }
                            });
                    colorField.eventColorPickerOpen += KlyteMonoUtils.DefaultColorPickerHandler;
                    colorField.objectUserData = true;
                }
                colorField.selectedColor = reference.m_colorList[i];
            }
            m_addColor.zOrder = 99999999;
            m_isLoadingColors = false;
        }

        private void CreateTemplateColorItem()
        {
            if (!UITemplateUtils.GetTemplateDict().ContainsKey(COLOR_SELECTOR_TEMPLATE))
            {
                var go = new GameObject();
                UIPanel panel = go.AddComponent<UIPanel>();
                panel.size = new Vector2(36, 36);
                panel.autoLayout = true;
                panel.wrapLayout = false;
                panel.padding = new RectOffset(4, 4, 4, 4);
                panel.autoLayoutDirection = LayoutDirection.Horizontal;

                KlyteMonoUtils.CreateColorField(panel);

                UITemplateUtils.GetTemplateDict()[COLOR_SELECTOR_TEMPLATE] = panel;
            }
        }
        #endregion

        private delegate void SafeObtainMethod(ref PropCityDataRuleXml x);
        private void SafeObtain(SafeObtainMethod action, int? targetTab = null)
        {
            int effTargetTab = Math.Max(-1, targetTab ?? m_currentIdx);
            if (effTargetTab < 0)
            {
                return;
            }

            if (effTargetTab < ACEPropConfigRulesData.Instance.Rules.m_dataArray.Length)
            {
                action(ref ACEPropConfigRulesData.Instance.Rules.m_dataArray[effTargetTab]);
                AssetColorExpanderMod.Controller.CleanCacheProp();
            }
        }
        private void OnChangeTab(int obj)
        {
            MainContainer.isVisible = obj >= 0;
            m_currentIdx = obj;
            ReloadData();
        }

        private void ReloadData()
        {
            SafeObtain((ref PropCityDataRuleXml x) =>
            {
                m_name.text = x.SaveName;

                m_districtWhiteList.isChecked = !x.SelectedDistrictsIsBlacklist;
                m_districtBlackList.isChecked = x.SelectedDistrictsIsBlacklist;
                m_districtResolutionOrder.selectedIndex = (int)x.DistrictRestrictionOrder;

                m_ruleFilter.selectedIndex = (int)x.RuleCheckType;

                m_service.selectedIndex = (int)x.Service;
                m_subService.selectedIndex = (int)x.SubService;
                m_level.selectedIndex = (int)x.Level;
                m_class.selectedValue = x.ItemClassName;
                m_parentClass.selectedValue = x.ItemClassName;

                string targetAsset = x.AssetNameSelf ?? "";
                KeyValuePair<string, string>? entry = AssetColorExpanderMod.Controller?.PropsLoaded.Where(y => y.Value == targetAsset).FirstOrDefault();
                m_assetFilter.text = entry?.Key ?? "";

                targetAsset = x.AssetNameBuilding ?? "";
                entry = AssetColorExpanderMod.Controller?.BuildingsLoaded.Where(y => y.Value == targetAsset).FirstOrDefault();
                m_assetFilterBuilding.text = entry?.Key ?? "";

                targetAsset = x.AssetNameNet ?? "";
                entry = AssetColorExpanderMod.Controller?.NetsLoaded.Where(y => y.Value == targetAsset).FirstOrDefault();
                m_assetFilterNet.text = entry?.Key ?? "";

                ApplyRuleCheck(x);

                m_colorMode.selectedIndex = (int)x.ColoringMode;
                m_allowRed.isChecked = (x.PastelConfig & PastelConfig.AVOID_REDS) == 0;
                m_allowGreen.isChecked = (x.PastelConfig & PastelConfig.AVOID_GREENS) == 0;
                m_allowBlues.isChecked = (x.PastelConfig & PastelConfig.AVOID_BLUES) == 0;
                m_allowNeutral.isChecked = (x.PastelConfig & PastelConfig.AVOID_NEUTRALS) == 0;
                UpdateColorList(ref x);

                ApplyColorUIRules(x);

                UpdateDistrictList(ref x);
            });
        }

        private void ApplyColorUIRules(PropCityDataRuleXml x)
        {
            bool isPastel = x.ColoringMode == ColoringMode.PASTEL_FULL_VIVID || x.ColoringMode == ColoringMode.PASTEL_HIGHER_SATURATION || x.ColoringMode == ColoringMode.PASTEL_ORIG;
            m_allowRed.isVisible = isPastel;
            m_allowGreen.isVisible = isPastel;
            m_allowBlues.isVisible = isPastel;
            m_allowNeutral.isVisible = isPastel;
            m_listColorContainer.isVisible = x.ColoringMode == ColoringMode.LIST;
        }

        private void ApplyRuleCheck(PropCityDataRuleXml x)
        {
            m_service.parent.isVisible =
                   x.RuleCheckType == RuleCheckTypeProp.SERVICE
                || x.RuleCheckType == RuleCheckTypeProp.SERVICE_LEVEL
                || x.RuleCheckType == RuleCheckTypeProp.SERVICE_SUBSERVICE
                || x.RuleCheckType == RuleCheckTypeProp.SERVICE_SUBSERVICE_LEVEL
                || x.RuleCheckType == RuleCheckTypeProp.PARENT_SERVICE
                || x.RuleCheckType == RuleCheckTypeProp.PARENT_SERVICE_LEVEL
                || x.RuleCheckType == RuleCheckTypeProp.PARENT_SERVICE_SUBSERVICE
                || x.RuleCheckType == RuleCheckTypeProp.PARENT_SERVICE_SUBSERVICE_LEVEL;
            m_subService.parent.isVisible = x.RuleCheckType == RuleCheckTypeProp.SERVICE_SUBSERVICE || x.RuleCheckType == RuleCheckTypeProp.SERVICE_SUBSERVICE_LEVEL || x.RuleCheckType == RuleCheckTypeProp.PARENT_SERVICE_SUBSERVICE || x.RuleCheckType == RuleCheckTypeProp.PARENT_SERVICE_SUBSERVICE_LEVEL;
            m_level.parent.isVisible = x.RuleCheckType == RuleCheckTypeProp.SERVICE_LEVEL || x.RuleCheckType == RuleCheckTypeProp.SERVICE_SUBSERVICE_LEVEL || x.RuleCheckType == RuleCheckTypeProp.PARENT_SERVICE_LEVEL || x.RuleCheckType == RuleCheckTypeProp.PARENT_SERVICE_SUBSERVICE_LEVEL;
            m_class.parent.isVisible = x.RuleCheckType == RuleCheckTypeProp.ITEM_CLASS;
            m_parentClass.parent.isVisible = x.RuleCheckType == RuleCheckTypeProp.PARENT_ITEM_CLASS;
            m_assetFilter.parent.isVisible = x.RuleCheckType == RuleCheckTypeProp.ASSET_NAME_SELF || x.RuleCheckType == RuleCheckTypeProp.ASSET_NAME_BUILDING_SELF || x.RuleCheckType == RuleCheckTypeProp.ASSET_NAME_NET_SELF;
            m_assetFilterBuilding.parent.isVisible = x.RuleCheckType == RuleCheckTypeProp.ASSET_NAME_BUILDING || x.RuleCheckType == RuleCheckTypeProp.ASSET_NAME_BUILDING_SELF;
            m_assetFilterNet.parent.isVisible = x.RuleCheckType == RuleCheckTypeProp.ASSET_NAME_NET || x.RuleCheckType == RuleCheckTypeProp.ASSET_NAME_NET_SELF;

            m_popup.height = x.RuleCheckType == RuleCheckTypeProp.ASSET_NAME_BUILDING_SELF || x.RuleCheckType == RuleCheckTypeProp.ASSET_NAME_NET_SELF ? 250 : 290;
        }

        private string m_clipboard;

        private string GetRuleSerialized()
        {
            int effTargetTab = Math.Max(-1, m_currentIdx);
            if (effTargetTab >= 0 && effTargetTab < ACEPropConfigRulesData.Instance.Rules.m_dataArray.Length)
            {
                return XmlUtils.DefaultXmlSerialize(ACEPropConfigRulesData.Instance.Rules.m_dataArray[effTargetTab]);
            }
            else
            {
                return null;
            }
        }

        private void OnLoadRule(string obj) => SafeObtain((ref PropCityDataRuleXml x) =>
        {
            x = XmlUtils.DefaultXmlDeserialize<PropCityDataRuleXml>(obj);
            ACEPanel.Instance.PropTab.RuleList.FixTabstrip();
            ReloadData();
        });
        private void OnPasteRule() => OnLoadRule(m_clipboard);
        private void OnCopyRule() => SafeObtain((ref PropCityDataRuleXml x) =>
        {
            m_clipboard = XmlUtils.DefaultXmlSerialize(x);
            m_pasteSettings.isVisible = true;
        });

        private void OnSetDistrictsAsBlacklist(bool isChecked) => SafeObtain((ref PropCityDataRuleXml x) => { x.SelectedDistrictsIsBlacklist = isChecked; m_districtWhiteList.isChecked = !isChecked; });
        private void OnSetDistrictsAsWhitelist(bool isChecked) => SafeObtain((ref PropCityDataRuleXml x) => { x.SelectedDistrictsIsBlacklist = !isChecked; m_districtBlackList.isChecked = !isChecked; });
        private void OnChangeDistrictRestrictionOrder(int sel) => SafeObtain((ref PropCityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.DistrictRestrictionOrder = (DistrictRestrictionOrder)sel;
            }
        });

        private void OnSetName(string text) => SafeObtain((ref PropCityDataRuleXml x) =>
        {
            if (!text.IsNullOrWhiteSpace())
            {
                x.SaveName = text;
                ACEPanel.Instance.PropTab.RuleList.FixTabstrip();
                OnChangeTab(m_currentIdx);
            }
            else
            {
                m_name.text = x.SaveName;
            }
        });

        private void OnChangeRuleCheckType(int sel) => SafeObtain((ref PropCityDataRuleXml x) =>
        {
            x.RuleCheckType = (RuleCheckTypeProp)sel;
            ApplyRuleCheck(x);
        });


        private void OnAssetSelectedChanged(int sel) => SafeObtain((ref PropCityDataRuleXml x) =>
        {
            if (sel >= 0 && AssetColorExpanderMod.Controller.PropsLoaded.TryGetValue(m_popup.items[sel], out string assetName))
            {
                x.AssetNameSelf = assetName;
                m_assetFilter.text = m_popup.items[sel];
            }
            else
            {
                string targetAsset = x.AssetNameSelf ?? "";
                System.Collections.Generic.KeyValuePair<string, string>? entry = AssetColorExpanderMod.Controller?.PropsLoaded.Where(y => y.Value == targetAsset).FirstOrDefault();
                m_assetFilter.text = entry?.Key ?? "";
            }
        });
        private void OnAssetSelectedBuildingChanged(int sel) => SafeObtain((ref PropCityDataRuleXml x) =>
        {
            if (sel >= 0 && AssetColorExpanderMod.Controller.BuildingsLoaded.TryGetValue(m_popupBuilding.items[sel], out string assetName))
            {
                x.AssetNameBuilding = assetName;
                m_assetFilterBuilding.text = m_popupBuilding.items[sel];
            }
            else
            {
                string targetAsset = x.AssetNameBuilding ?? "";
                KeyValuePair<string, string>? entry = AssetColorExpanderMod.Controller?.BuildingsLoaded.Where(y => y.Value == targetAsset).FirstOrDefault();
                m_assetFilterBuilding.text = entry?.Key ?? "";
            }
        });
        private void OnAssetSelectedNetChanged(int sel) => SafeObtain((ref PropCityDataRuleXml x) =>
        {
            if (sel >= 0 && AssetColorExpanderMod.Controller.NetsLoaded.TryGetValue(m_popupNet.items[sel], out string assetName))
            {
                x.AssetNameNet = assetName;
                m_assetFilterNet.text = m_popupNet.items[sel];
            }
            else
            {
                string targetAsset = x.AssetNameNet ?? "";
                KeyValuePair<string, string>? entry = AssetColorExpanderMod.Controller?.NetsLoaded.Where(y => y.Value == targetAsset).FirstOrDefault();
                m_assetFilterNet.text = entry?.Key ?? "";
            }
        });

        private void OnChangeClassFilter(int sel) => SafeObtain((ref PropCityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.ItemClassName = m_class.items[sel];
                if (m_parentClass.selectedValue != x.ItemClassName)
                {
                    m_parentClass.selectedValue = x.ItemClassName;
                }
            }
        });
        private void OnChangeParentClassFilter(int sel) => SafeObtain((ref PropCityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.ItemClassName = m_parentClass.items[sel];
                if (m_class.selectedValue != x.ItemClassName)
                {
                    m_class.selectedValue = x.ItemClassName;
                }
            }
        });
        private void OnChangeLevelFilter(int sel) => SafeObtain((ref PropCityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.Level = (ItemClass.Level)sel - 1;
            }
        });
        private void OnChangeSubServiceFilter(int sel) => SafeObtain((ref PropCityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.SubService = (ItemClass.SubService)sel;
            }
        });
        private void OnChangeServiceFilter(int sel) => SafeObtain((ref PropCityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.Service = (ItemClass.Service)sel;
            }
        });
        private void OnChangeColoringMode(int sel) => SafeObtain((ref PropCityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.ColoringMode = (ColoringMode)sel;
                ApplyColorUIRules(x);
            }
        });

        private void OnAllowRedChanged(bool isChecked) => SafeObtain((ref PropCityDataRuleXml x) =>
        {
            if (isChecked)
            {
                x.PastelConfig &= ~PastelConfig.AVOID_REDS;
            }
            else
            {
                x.PastelConfig |= PastelConfig.AVOID_REDS;
            }
        });
        private void OnAllowGreenChanged(bool isChecked) => SafeObtain((ref PropCityDataRuleXml x) =>
        {
            if (isChecked)
            {
                x.PastelConfig &= ~PastelConfig.AVOID_GREENS;
            }
            else
            {
                x.PastelConfig |= PastelConfig.AVOID_GREENS;
            }
        });
        private void OnAllowBlueChanged(bool isChecked) => SafeObtain((ref PropCityDataRuleXml x) =>
        {
            if (isChecked)
            {
                x.PastelConfig &= ~PastelConfig.AVOID_BLUES;
            }
            else
            {
                x.PastelConfig |= PastelConfig.AVOID_BLUES;
            }
        });
        private void OnAllowNeutralChanged(bool isChecked) => SafeObtain((ref PropCityDataRuleXml x) =>
        {
            if (isChecked)
            {
                x.PastelConfig &= ~PastelConfig.AVOID_NEUTRALS;
            }
            else
            {
                x.PastelConfig |= PastelConfig.AVOID_NEUTRALS;
            }
        });
    }

}