﻿using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

using System.Reflection;
using KSP.UI.Screens;
using KSP.Localization;



namespace ScienceChecklist
{
    public enum Statuses { none, seat, pod }
    class ShipStateWindow : Window<ScienceChecklistAddon>
    {
        private GUIStyle labelStyle;
        private GUIStyle sectionStyle;
        private Vector2 scrollPosition;
        private readonly ScienceChecklistAddon _parent;
        private readonly Logger _logger;
        private MapObject SelectedObject;
        private static string str_ShipStateWindow_Title = Localizer.Format("#XScience_ShipStateWindow_Title");
        private static string str_UnknowObject = Localizer.Format("#XScience_ShipStateWindow_UnownedObject");
        private static string str_State_NoCommandPod = Localizer.Format("#XScience_ShipStateWindow_NoCommandPod");
        private static string str_State_HasCommandSeat = Localizer.Format("#XScience_ShipStateWindow_HasCommandSeat");
        public Statuses status = Statuses.none;


        public ShipStateWindow(ScienceChecklistAddon Parent)
            : base(str_ShipStateWindow_Title, 250, 30) // "[x] Science! Selected Object"
        {
            _parent = Parent;
            UiScale = ScienceChecklistAddon.Config.UiScale;
            scrollPosition = Vector2.zero;
            ScienceChecklistAddon.Config.UiScaleChanged += OnUiScaleChange;
            _logger = new Logger(this);
            SelectedObject = null;

            _parent.ScienceEventHandler.MapObjectSelected += (s, e) => MapObjectSelected(s, e);
        }



        protected override void ConfigureStyles()
        {
            base.ConfigureStyles();

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(_skin.label);
                labelStyle.wordWrap = true;
                labelStyle.fontStyle = FontStyle.Normal;
                labelStyle.normal.textColor = Color.white;
                labelStyle.stretchWidth = true;
                labelStyle.stretchHeight = false;
                labelStyle.margin.bottom -= wScale(2);
                labelStyle.padding.bottom -= wScale(2);
            }

            if (sectionStyle == null)
            {
                sectionStyle = new GUIStyle(labelStyle);
                sectionStyle.fontStyle = FontStyle.Bold;
            }
        }



        private void OnUiScaleChange(object sender, EventArgs e)
        {
            UiScale = ScienceChecklistAddon.Config.UiScale;
            labelStyle = null;
            sectionStyle = null;
            base.OnUiScaleChange();
            ConfigureStyles();
        }



        protected override void DrawWindowContents(int windowID)
        {
            switch (SelectedObject.type)
            {
                case MapObject.ObjectType.CelestialBody:
                    DrawBody();
                    break;
                case MapObject.ObjectType.Vessel:
                    DrawVessel();
                    break;
            }
        }

        protected void DrawBody()
        {
            string Title = "";
            string Text = "";
            if (SelectedObject.celestialBody)
            {
                Body Body = _parent.Science.BodyList[SelectedObject.celestialBody];
                Title += Localizer.Format("#XScience_Status_BodyName", GameHelper.LocalizeBodyName(Body.CelestialBody.displayName)) + "\n"; //"Body: " + GameHelper.LocalizeBodyName(Body.CelestialBody.displayName)
                Title += Body.Type;
                if (Body.IsHome)
                    Title += " - " + Localizer.Format("#XScience_Status_BodyisHome"); // Home!



                Text += Localizer.Format("#XScience_Status_SpaceHigh", Body.CelestialBody.scienceValues.spaceAltitudeThreshold / 1000); // "Space high: " + (Body.CelestialBody.scienceValues.spaceAltitudeThreshold / 1000) + "km"
                if (Body.HasAtmosphere)
                {
                    Text += "\n" + Localizer.Format("#XScience_Status_AtmosDepth", Body.CelestialBody.atmosphereDepth / 1000); // Atmos depth: " + (Body.CelestialBody.atmosphereDepth / 1000) + "km"
                    Text += "\n" + Localizer.Format("#XScience_Status_FlyingHigh", Body.CelestialBody.scienceValues.flyingAltitudeThreshold / 1000); // Flying high: " + (Body.CelestialBody.scienceValues.flyingAltitudeThreshold / 1000) + "km
                    if (Body.HasOxygen)
                        Text += "\n" + Localizer.Format("#XScience_Status_HasOxygen"); // Has oxygen - jets work
                }
                else
                    Text += "\n" + Localizer.Format("#XScience_Status_NoOxygen"); // No kind of atmosphere

                if (Body.HasSurface)
                {
                    if (Body.HasOcean)
                        Text += "\n" + Localizer.Format("#XScience_Status_HasOceans"); // Has oceans
                }
                else
                    Text += "\n" + Localizer.Format("#XScience_Status_NoSurface"); // No surface
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            GUILayout.Label(Title, sectionStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(wScale(16));
            GUILayout.Label(Text, labelStyle, GUILayout.ExpandWidth(true));

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.Space(wScale(8));
        }



        protected void DrawVessel()
        {
            string Title = "";
            string Text = "";
            if (SelectedObject.vessel != null && SelectedObject.vessel.protoVessel != null)
            {
                if (SelectedObject.vessel.DiscoveryInfo.Level != DiscoveryLevels.Owned)
                    Title = str_UnknowObject; // "Unowned object"
                else
                {
                    Title = SelectedObject.vessel.GetName();
                    ProtoVessel proto = SelectedObject.vessel.protoVessel;
                    double mass = 0;
                    var res = new SortedDictionary<string, xResourceData>();
                    foreach (ProtoPartSnapshot p in proto.protoPartSnapshots)
                    {
                        foreach (var r in p.resources)
                        {
                            xResourceData d;
                            if (res.ContainsKey(r.resourceName))
                                d = res[r.resourceName];
                            else
                            {
                                d = new xResourceData(r.resourceName);
                            }
                            d.current += r.amount;
                            d.max += r.maxAmount;
                            res[r.resourceName] = d;
                        }
                        mass += p.mass;
                        CheckCommand(p);
                    }



                    var texts = res.Values.ToList().ConvertAll(d => d.ToString());

                    if (!SelectedObject.vessel.isEVA)
                    {
                        texts.Add("");
                        var crew = proto.GetVesselCrew().Count();
                        mass += res.Values.Sum(d => d.GetMass());
                        var parts = proto.protoPartSnapshots.Count();
                        texts.Add(Localizer.Format("#XScience_ShipStateWindow_showinfo", crew, parts, mass)); // string.Format("Crew: {0}, Parts: {1}, Mass: {2:f2}t", crew, parts, mass)



                        switch (this.status)
                        {
                            case Statuses.pod:
                                break;
                            case Statuses.none:
                                texts.Add(str_State_NoCommandPod); // "No command pod"
                                break;
                            case Statuses.seat:
                                texts.Add(str_State_HasCommandSeat); // "Has command seat"
                                break;
                        }
                    }

                    Text = string.Join("\n", texts.ToArray());

                }



            }





            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            GUILayout.Label(Title, sectionStyle, GUILayout.ExpandWidth(true));
            GUILayout.Space(wScale(16));
            GUILayout.Label(Text, labelStyle, GUILayout.ExpandWidth(true));

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.Space(wScale(8));
        }


        public void CheckCommand(ProtoPartSnapshot part)
        {
            if (status == Statuses.pod)
                return;

            foreach (var m in part.modules)
            {
                if (m.moduleName == "ModuleCommand")
                {
                    status = Statuses.pod;
                    return;
                }
                if (m.moduleName == "KerbalSeat")
                {
                    status = Statuses.seat;
                }
            }
        }


        public void MapObjectSelected(object sender, NewSelectionData SelectionData)
        {
            //_logger.Trace( "MapObjectSelected" );
            //_logger.Trace( SelectionData._selectedObject.type.ToString( ) );

            if (!ScienceChecklistAddon.Config.SelectedObjectWindow)
                return;

            switch (SelectionData._selectedObject.type)
            {
                case MapObject.ObjectType.CelestialBody:
                    SetVisible(true);
                    SelectedObject = SelectionData._selectedObject;
                    if (!_parent.Science.BodyList.ContainsKey(SelectedObject.celestialBody))
                        _parent.Science.Reset();
                    break;
                case MapObject.ObjectType.Vessel:
                    SetVisible(true);
                    SelectedObject = SelectionData._selectedObject;
                    break;
                default:
                    SetVisible(false);
                    break;
            }

        }



        public WindowSettings BuildSettings()
        {
            //_logger.Info( "BuildSettings" );
            WindowSettings W = new WindowSettings(ScienceChecklistAddon.WINDOW_NAME_SHIP_STATE);
            W.Set("Top", (int)windowPos.yMin);
            W.Set("Left", (int)windowPos.xMin);
            W.Set("Width", (int)windowPos.width);
            W.Set("Height", (int)windowPos.height);

            return W;
        }



        public void ApplySettings(WindowSettings W)
        {
            windowPos.yMin = W.GetInt("Top", 40);
            windowPos.xMin = W.GetInt("Left", 40);
            windowPos.width = W.GetInt("Width", 200);
            windowPos.height = W.GetInt("Height", 200);

            if (windowPos.width < 100)
                windowPos.width = 100;

            if (windowPos.width < 50)
                windowPos.width = 50;

        }
    }



    class xResourceData
    {
        public double current, max;

        public readonly string name;
        readonly PartResourceDefinition def;

        public xResourceData(string name)
        {
            this.name = name;
            this.def = PartResourceLibrary.Instance.GetDefinition(name);
        }

        public double GetMass()
        {
            return def == null ? 0 : def.density * current;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1} / {2}", name, s(current), s(max));
        }

        private static string s(double d)
        {
            return d.ToString(d < 100 ? "f2" : "f0");
        }
    }









}