﻿using ScienceChecklist.Lib.Adds;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.Localization;



namespace ScienceChecklist
{
    class StatusWindow : Window<ScienceChecklistAddon>
    {
        public event EventHandler NoiseEvent;
        private readonly Texture2D _emptyTexture;

        private readonly Texture2D _progressTexture;
        private readonly Texture2D _completeTexture;

        private readonly Texture2D _GfxTimeWarp;
        private readonly Texture2D _GfxTimeWarpOff;
        private readonly Texture2D _GfxAudioAlert;
        private readonly Texture2D _GfxAudioAlertOff;
        private readonly Texture2D _GfxResultsWindow;
        private readonly Texture2D _GfxResultsWindowOff;

        private readonly ExperimentFilter _filter;
        private readonly ScienceChecklistAddon _parent;
        private readonly Logger _logger;
        private int _previousNumExperiments;
        private float _previousUiScale;
        private float _previousSciThreshold;
        private bool _previousHideMinSciSlider;
        private GUIStyle _experimentButtonStyle;
        private GUIStyle _experimentLabelStyle;
        private GUIStyle _situationStyle;
        private GUIStyle _scienceThresholdLabelStyle;
        private GUIStyle _horizontalScrollbarOnboardStyle;
        private GUIStyle _progressLabelStyle;
        private IList<ModuleScienceExperiment> _moduleScienceExperiments;
        private IList<ModuleScienceExperiment> _DMModuleScienceAnimates;
        private IList<ModuleScienceExperiment> _DMModuleScienceAnimateGenerics;
        private Dictionary<string, bool> _availableScienceExperiments;
        private static string str_HereAndNow_Title = Localizer.Format("#XScience_HereAndNow_Title");
        private static string str_HereAndNow_MinScience = Localizer.Format("#XScience_HereAndNow_MinScience");
        private static string str_HereAndNow_TimeWarpStop = Localizer.Format("#XScience_HereAndNow_TimeWarpStop");
        private static string str_HereAndNow_TimeWarpNotStop = Localizer.Format("#XScience_HereAndNow_TimeWarpNotStop");
        private static string str_HereAndNow_AlertSound = Localizer.Format("#XScience_HereAndNow_AlertSound");
        private static string str_HereAndNow_NoSound = Localizer.Format("#XScience_HereAndNow_NoSound");
        private static string str_HereAndNow_ShowResults = Localizer.Format("#XScience_HereAndNow_ShowResults");
        private static string str_HereAndNow_SuppressResults = Localizer.Format("#XScience_HereAndNow_SuppressResults");
        private static string str_HereAndNow_ExperimentDescription = Localizer.Format("#XScience_HereAndNow_ExperimentDescription");

        public event EventHandler OnCloseEvent;
        public event EventHandler OnOpenEvent;

        public StatusWindow(ScienceChecklistAddon Parent)
            : base(str_HereAndNow_Title, 250, 30) // "[x] Science! Here and Now"
        {
            _parent = Parent;
            UiScale = ScienceChecklistAddon.Config.UiScale;
            _logger = new Logger(this);
            _filter = new ExperimentFilter(_parent);
            _filter.DisplayMode = DisplayMode.CurrentSituation;
            _filter.EnforceLabLanderMode = true;

            _emptyTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            _emptyTexture.SetPixels(new[] { Color.clear });
            _emptyTexture.Apply();
            _progressTexture = TextureHelper.FromResource("ScienceChecklist.icons.scienceProgress.png", 13, 13);
            _completeTexture = TextureHelper.FromResource("ScienceChecklist.icons.scienceComplete.png", 13, 13);

            _GfxTimeWarp = TextureHelper.FromResource("ScienceChecklist.icons.time-warp.png", 13, 13);
            _GfxTimeWarpOff = TextureHelper.FromResource("ScienceChecklist.icons.time-warp-x.png", 13, 13);
            _GfxAudioAlert = TextureHelper.FromResource("ScienceChecklist.icons.audio-alert.png", 13, 13);
            _GfxAudioAlertOff = TextureHelper.FromResource("ScienceChecklist.icons.audio-alert-off.png", 13, 13);
            _GfxResultsWindow = TextureHelper.FromResource("ScienceChecklist.icons.report.png", 13, 13);
            _GfxResultsWindowOff = TextureHelper.FromResource("ScienceChecklist.icons.report-x.png", 13, 13);

            _availableScienceExperiments = new Dictionary<string, bool>();

            ScienceChecklistAddon.Config.HideCompleteEventsChanged += (s, e) => RefreshFilter(s, e);
            ScienceChecklistAddon.Config.CompleteWithoutRecoveryChanged += (s, e) => RefreshFilter(s, e);

            _parent.ScienceEventHandler.FilterUpdateEvent += (s, e) => RefreshFilter(s, e);
            _parent.ScienceEventHandler.SituationChanged += (s, e) => UpdateSituation(s, e);

            this.Resizable = false;
            _filter.UpdateFilter();
            ScienceChecklistAddon.Config.UiScaleChanged += OnUiScaleChange;
        }

        protected override void ConfigureStyles()
        {
            base.ConfigureStyles();

            _progressLabelStyle = new GUIStyle(_skin.label)
            {
                fontStyle = FontStyle.BoldAndItalic,
                alignment = TextAnchor.MiddleCenter,
                fontSize = wScale(9),
                normal = {
                    textColor = new Color(0.322f, 0.298f, 0.004f)
                }
            };

            _horizontalScrollbarOnboardStyle = new GUIStyle(_skin.horizontalScrollbar)
            {
                normal = { background = _emptyTexture }
            };

            _situationStyle = new GUIStyle(_skin.label)
            {
                fontSize = wScale(13),
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Normal,
                fixedHeight = wScale(25),
                contentOffset = wScale(new Vector2(0, 6)),
                wordWrap = true,
                normal = {
                    textColor = new Color(0.7f, 0.8f, 0.8f)
                }
            };
            _scienceThresholdLabelStyle = new GUIStyle(_skin.label)
            {
                fontSize = wScale(14),
                alignment = TextAnchor.MiddleLeft,
                padding = wScale(new RectOffset(0, 0, -2, 0))
            };
            _experimentButtonStyle = new GUIStyle(_skin.button)
            {
                fontSize = wScale(12)
            };
            _experimentLabelStyle = new GUIStyle(_experimentButtonStyle)
            {
                fontSize = wScale(12),
                normal = { textColor = Color.black }
            };
        }



        private void OnUiScaleChange(object sender, EventArgs e)
        {
            UiScale = ScienceChecklistAddon.Config.UiScale;
            _progressLabelStyle = null;
            _horizontalScrollbarOnboardStyle = null;
            _situationStyle = null;
            _experimentButtonStyle = null;
            _experimentLabelStyle = null;

            base.OnUiScaleChange();
            ConfigureStyles();
        }



        protected override void DrawWindowContents(int windowID)
        {
            GUILayout.BeginVertical();

            if (_filter.CurrentSituation != null && _parent.Science.CurrentVesselScience != null)
            {
                var desc = _filter.CurrentSituation.Description;
                GUILayout.Box
                (
                    new GUIContent
                    (
                        char.ToUpper(desc[0]) + desc.Substring(1),
                        MakeSituationToolTip()
                    ),
                    _situationStyle,
                    GUILayout.Width(wScale(250))
                );
            }


            if (!ScienceChecklistAddon.Config.HideMinScienceSlider)
            {
                GUILayout.Space(wScale(10));
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(str_HereAndNow_MinScience, _scienceThresholdLabelStyle); // "Min Science"

                    float scienceThreshold = 0f;
                    if (ScienceChecklistAddon.Config.VeryLowMinScience)
                    {
                        float minSci = 0.0001f;
                        float maxSci = 0.1f;
                        float prev_scienceThreshold = Math.Min(Math.Max(ScienceChecklistAddon.Config.ScienceThreshold, minSci), maxSci);
                        scienceThreshold = Adds.AcceleratedSlider(prev_scienceThreshold, minSci, maxSci, 2.7f, new[]
                        {
                            new Adds.StepRule(0.0001f, 0.005f),
                            new Adds.StepRule(0.001f, 0.01f),
                            new Adds.StepRule(0.01f, 0.1f),
                        });
                    }
                    else
                    {
                        float minSci = 0.1f;
                        float maxSci = 50f;
                        float prev_scienceThreshold = Math.Min(Math.Max(ScienceChecklistAddon.Config.ScienceThreshold, minSci), maxSci);
                        scienceThreshold = Adds.AcceleratedSlider(prev_scienceThreshold, minSci, maxSci, 1.8f, new[]
                        {
                            new Adds.StepRule(0.5f, 10f),
                            new Adds.StepRule(1f, 40f),
                            new Adds.StepRule(2f, 50f),
                        });
                    }
                    if (ScienceChecklistAddon.Config.ScienceThreshold != scienceThreshold)
                    {
                        ScienceChecklistAddon.Config.ScienceThreshold = scienceThreshold;
                        ScienceChecklistAddon.Config.Save();
                        _parent.Science.UpdateAllScienceInstances();
                        _filter.UpdateFilter();
                    }

                    string format = "F1";
                    int width = 26;
                    if (ScienceChecklistAddon.Config.VeryLowMinScience)
                    {
                        if (scienceThreshold < 0.005f)
                        {
                            format = "F4";
                        }
                        else if (scienceThreshold < 0.01f)
                        {
                            format = "F3";
                        }
                        else if (scienceThreshold < 0.1f)
                        {
                            format = "F2";
                        }
                        width = 40;
                    }
                    GUILayout.Label(ScienceChecklistAddon.Config.ScienceThreshold.ToString(format), _scienceThresholdLabelStyle, GUILayout.Width(wScale(width)));

                }
            }
            else if (   (   ScienceChecklistAddon.Config.VeryLowMinScience
                         && ScienceChecklistAddon.Config.ScienceThreshold > 0.1f
                        )
                     || (  !ScienceChecklistAddon.Config.VeryLowMinScience
                         && ScienceChecklistAddon.Config.ScienceThreshold < 0.1f
                        )
                    )
            {
                // VeryLowMinScience option was toggled while MinScience slider hidden.
                // Need to adjust ScienceThreshold accordingly (and re-classify experiments as completed/incomplete)!
                ScienceChecklistAddon.Config.ScienceThreshold = 0.1f;
                ScienceChecklistAddon.Config.Save();
                _parent.Science.UpdateAllScienceInstances();
                _filter.UpdateFilter();
            }

            int Top = wScale(90 - (ScienceChecklistAddon.Config.HideMinScienceSlider ? 25 : 0));
            if (_filter.DisplayScienceInstances != null)
            {
                for (var i = 0; i < _filter.DisplayScienceInstances.Count; i++)
                {
                    var experiment = _filter.DisplayScienceInstances[i];

                    if (experiment.NextScienceIncome >= ScienceChecklistAddon.Config.ScienceThreshold)
                    {
                        var rect = new Rect(wScale(5), Top, wScale(250), wScale(30));
                        DrawExperiment(experiment, rect);
                        Top += wScale(35);
                        GUILayout.Space(wScale(35));
                    }
                }
            }
            else
                _logger.Trace("DisplayExperiments is null");

            GUILayout.Space(wScale(10));

            GUILayout.BeginHorizontal();
            GUIContent Content = null;
            if (ScienceChecklistAddon.Config.StopTimeWarp)
                Content = new GUIContent(_GfxTimeWarp, str_HereAndNow_TimeWarpStop); // "Time warp will be stopped"
            else
                Content = new GUIContent(_GfxTimeWarpOff, str_HereAndNow_TimeWarpNotStop); // "Time warp will not be stopped"
            if (GUILayout.Button(Content, GUILayout.Width(wScale(36)), GUILayout.Height(wScale(32))))
            {
                ScienceChecklistAddon.Config.StopTimeWarp = !ScienceChecklistAddon.Config.StopTimeWarp;
                ScienceChecklistAddon.Config.Save();
            }



            if (ScienceChecklistAddon.Config.PlayNoise)
                Content = new GUIContent(_GfxAudioAlert, str_HereAndNow_AlertSound); // "Audio alert will sound"
            else
                Content = new GUIContent(_GfxAudioAlertOff, str_HereAndNow_NoSound); // "No audio alert"
            if (GUILayout.Button(Content, GUILayout.Width(wScale(36)), GUILayout.Height(wScale(32))))
            {
                ScienceChecklistAddon.Config.PlayNoise = !ScienceChecklistAddon.Config.PlayNoise;
                ScienceChecklistAddon.Config.Save();
            }



            if (ScienceChecklistAddon.Config.ShowResultsWindow)
                Content = new GUIContent(_GfxResultsWindow, str_HereAndNow_ShowResults); //"Show results window"
            else
                Content = new GUIContent(_GfxResultsWindowOff, str_HereAndNow_SuppressResults); // "Suppress results window"
            if (GUILayout.Button(Content, GUILayout.Width(wScale(36)), GUILayout.Height(wScale(32))))
            {
                ScienceChecklistAddon.Config.ShowResultsWindow = !ScienceChecklistAddon.Config.ShowResultsWindow;
                ScienceChecklistAddon.Config.Save();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(wScale(2));

        }



        public override void DrawWindow()
        {
            // The window needs to get smaller when the number of experiments drops.
            // This allows that while preventing flickering.
            if (   _previousNumExperiments != _filter.DisplayScienceInstances.Count
                || ScienceChecklistAddon.Config.UiScale != _previousUiScale
                || ScienceChecklistAddon.Config.HideMinScienceSlider != _previousHideMinSciSlider
                || ScienceChecklistAddon.Config.ScienceThreshold != _previousSciThreshold
               )
            {
                windowPos.height = wScale(30) + ((_filter.DisplayScienceInstances.Count + 1) * wScale(35));
                windowPos.width = wScale(defaultWindowSize.x);
                _previousNumExperiments = _filter.DisplayScienceInstances.Count;
                _previousUiScale = ScienceChecklistAddon.Config.UiScale;
                _previousHideMinSciSlider = ScienceChecklistAddon.Config.HideMinScienceSlider;
                _previousSciThreshold = ScienceChecklistAddon.Config.ScienceThreshold;
            }

            base.DrawWindow();
        }



        private string MakeSituationToolTip()
        {
            string Text = "";



            if (_filter.CurrentSituation != null)
            {
                Body Body = _filter.CurrentSituation.Body;
                Text += Localizer.Format("#XScience_Status_BodyName", GameHelper.LocalizeBodyName(Body.CelestialBody.displayName)) + "\n"; // "Body: " + GameHelper.LocalizeBodyName(Body.CelestialBody.displayName) + "\n"
                Text += Body.Type;
                if (Body.IsHome)
                    Text += " - " + Localizer.Format("#XScience_Status_BodyisHome"); //Home! 
                Text += "\n\n";
                Text += Localizer.Format("#XScience_Status_SpaceHigh", Body.CelestialBody.scienceValues.spaceAltitudeThreshold / 1000) + "\n"; // "Space high: " + (Body.CelestialBody.scienceValues.spaceAltitudeThreshold / 1000) + "km\n"

                if (Body.HasAtmosphere)
                {
                    Text += Localizer.Format("#XScience_Status_AtmosDepth", Body.CelestialBody.atmosphereDepth / 1000) + "\n"; // "Atmos depth: " + (Body.CelestialBody.atmosphereDepth / 1000) + "km\n"
                    Text += Localizer.Format("#XScience_Status_FlyingHigh", Body.CelestialBody.scienceValues.flyingAltitudeThreshold / 1000) + "\n"; // "Flying high: " + (Body.CelestialBody.scienceValues.flyingAltitudeThreshold / 1000) + "km\n"
                    if (Body.HasOxygen)
                        Text += Localizer.Format("#XScience_Status_HasOxygen") + "\n"; // "Has oxygen - jets work\n"
                }
                else
                    Text += Localizer.Format("#XScience_Status_NoOxygen") + "\n"; //"No kind of atmosphere\n" 

                if (Body.HasSurface)
                {
                    if (Body.HasOcean)
                        Text += Localizer.Format("#XScience_Status_HasOceans") + "\n"; // Has oceans
                }
                else
                    Text += Localizer.Format("#XScience_Status_NoSurface") + "\n"; // No surface

                Text += "\n";
            }

            Text += Localizer.Format("#XScience_Status_CurrentVesselStored", _parent.Science.CurrentVesselScience.Count()); // "Current vessel: " + _parent.Science.CurrentVesselScience.Count() + " stored experiments"

            return Text;
        }



        private void PlayNoise()
        {
            if (NoiseEvent != null)
            {
                NoiseEvent(this, EventArgs.Empty);
            }
        }



        private void DrawExperiment(ScienceInstance exp, Rect rect)
        {
            bool ExperimentRunnable = CanRunExperiment(exp, true);
            Rect buttonRect = new Rect(rect) { xMax = wScale(200) };
            string scienceValueString = " (" + exp.NextScienceIncome.ToString(
                ScienceChecklistAddon.Config.VeryLowMinScience && exp.NextScienceIncome < 1 ? "F3" : "F1"
                ) + ")\n" + (exp.CompletedScience + exp.OnboardScience).ToString("F2");
            GUIContent expContent = new GUIContent(exp.ShortDescription + scienceValueString,str_HereAndNow_ExperimentDescription); // "Experiment description (next run value)\n\nRecovered+OnBoard value"

            if (ExperimentRunnable)
            {
                _experimentButtonStyle.normal.textColor = exp.IsComplete ? Color.green : Color.yellow;
                if (GUI.Button(buttonRect, expContent, _experimentButtonStyle))
                {
                    RunExperiment(exp);
                }
            }
            else
            {
                GUI.Label(buttonRect, expContent, _experimentLabelStyle);
            }
            int Dif = (int)(((rect.yMax - rect.yMin) - wScale(13)) / 2);
            Rect progressRect = new Rect(wScale(205), rect.yMin + Dif, wScale(50), wScale(13));
            ProgressBar(progressRect, exp.CompletedScience, exp.TotalScience, exp.CompletedScience + exp.OnboardScience);
        }



        private void ProgressBar(Rect rect, float curr, float total, float curr2)
        {
            var completeTexture = _completeTexture;
            var progressTexture = _progressTexture;
            var complete = curr > total || (total - curr < ScienceChecklistAddon.Config.ScienceThreshold);
            if (complete)
                curr = total;
            var progressRect = new Rect(rect.xMin, rect.yMin, rect.width, wScale(13));

            GUI.skin.horizontalScrollbar.fixedHeight = wScale(13);
            GUI.skin.horizontalScrollbarThumb.fixedHeight = wScale(13);

            if (curr2 != 0 && !complete)
            {
                var complete2 = false;
                if (curr2 > total || (total - curr2 < ScienceChecklistAddon.Config.ScienceThreshold))
                {
                    curr2 = total;
                    complete2 = true;
                }
                _skin.horizontalScrollbarThumb.normal.background = curr2 < ScienceChecklistAddon.Config.ScienceThreshold
                    ? _emptyTexture
                    : complete2
                        ? completeTexture
                        : progressTexture;

                GUI.HorizontalScrollbar(progressRect, 0, curr2 / total, 0, 1, _horizontalScrollbarOnboardStyle);
            }

            _skin.horizontalScrollbarThumb.normal.background = curr < ScienceChecklistAddon.Config.ScienceThreshold
                ? _emptyTexture
                : complete ? completeTexture : progressTexture;

            GUI.HorizontalScrollbar(progressRect, 0, curr / total, 0, 1);

            var showValues = true;
            if (showValues)
            {
                var labelRect = new Rect(progressRect)
                {
                    y = progressRect.y + 1,
                };
                GUI.Label(labelRect, string.Format("{0:0.##}/{1:0.##}", curr, total), _progressLabelStyle);
            }
        }



        #region Events called when science changes
        // Refreshes the experiment filter.
        // This is the lightest update used when the vessel changes
        public void RefreshFilter(object sender, EventArgs e)
        {
            _logger.Info("StatusWindow.RefreshFilter");

            if (!IsVisible())
            {
                return;
            }

            //			_logger.Trace( "RefreshFilter" );

            if (_moduleScienceExperiments != null)
                _moduleScienceExperiments.Clear();
            if (_availableScienceExperiments != null)
                _availableScienceExperiments.Clear();
            if (_DMModuleScienceAnimates != null)
                _DMModuleScienceAnimates.Clear();
            if (_DMModuleScienceAnimateGenerics != null)
                _DMModuleScienceAnimateGenerics.Clear();



            Vessel v = FlightGlobals.ActiveVessel;
            if (v != null && HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                _moduleScienceExperiments = v.FindPartModulesImplementing<ModuleScienceExperiment>();
                _DMModuleScienceAnimates = v.FindPartModulesImplementing<ModuleScienceExperiment>().Where(x => _parent.DMagic.inheritsFromOrIsDMModuleScienceAnimate(x)).ToList();
                _DMModuleScienceAnimateGenerics = v.FindPartModulesImplementing<ModuleScienceExperiment>().Where(x => _parent.DMagic.inheritsFromOrIsDMModuleScienceAnimateGeneric(x)).ToList();
            }

            //_filter.UpdateFilter(_DMModuleScienceAnimates);


            _filter.UpdateFilter(_DMModuleScienceAnimateGenerics);
        }



        // Bung new situation into filter and recalculate everything
        public void UpdateSituation(object sender, NewSituationData e)
        {
            // Following commented out because if not visible, this was returning and never setting the CurrentSituation
#if false
			if (!IsVisible())
            {
                return;
            }
#endif
            // _logger.Trace( "StatusWindow.UpdateSituation" );
            if (e == null)
            {
                _filter.CurrentSituation = null;
                return;
            }
            else
            {
                _filter.CurrentSituation = new Situation(e._body, e._situation, e._biome, e._subBiome);
            }

            //_logger.Trace( "ScienceThisBiome: " + _filter.TotalCount + " / " + _filter.CompleteCount );

            if (_filter.TotalCount > 0)
            {
                var anyRunnableExperiments = false;
                for (var i = 0; i < _filter.DisplayScienceInstances.Count; i++)
                {
                    var experiment = _filter.DisplayScienceInstances[i];
                    var Id = experiment.ScienceExperiment.id;

                    if (experiment.NextScienceIncome < ScienceChecklistAddon.Config.ScienceThreshold)
                    {
                        continue;
                    }

                    if (Id == "crewReport" || Id == "surfaceSample" || Id == "evaReport") // Always pop UI for Kerbal experiments
                    {
                        anyRunnableExperiments = true;
                        break;
                    }
                    else
                    {
                        if (CanRunExperiment(experiment))
                        {
                            anyRunnableExperiments = true;
                            break;
                        }
                    }
                }

                if (anyRunnableExperiments)
                {
                    if (IsVisible())
                    {
                        if (ScienceChecklistAddon.Config.StopTimeWarp)
                            GameHelper.StopTimeWarp();
                        if (ScienceChecklistAddon.Config.PlayNoise)
                            PlayNoise();
                        if (ScienceChecklistAddon.Config.StopTimeWarp || ScienceChecklistAddon.Config.PlayNoise)
                            ScreenMessages.PostScreenMessage(Localizer.Format(" #XScience_PostMessage_NewSituation", _filter.CurrentSituation.Description)); // "New Situation: " + _filter.CurrentSituation.Description
                    }
                }
            }
        }
        #endregion



        public bool CanRunExperiment(ScienceInstance s, bool runSingleUse = true)
        {
            bool IsAvailable = false;
            if (_availableScienceExperiments.ContainsKey(s.ScienceExperiment.id))
                return _availableScienceExperiments[s.ScienceExperiment.id];

            IEnumerable<ModuleScienceExperiment> dlm = FindDMAnimateGenericsForExperiment(s.ScienceExperiment.id);
            if (dlm != null && dlm.Any())
            {
                DMModuleScienceAnimateGeneric NewDMagicInstance = _parent.DMagic.GetDMModuleScienceAnimateGeneric();
                IsAvailable = dlm.Any(x =>
                    (int)x.Fields.GetValue("experimentsLimit") > 1 ? NewDMagicInstance.canConduct(x) : NewDMagicInstance.canConduct(x) &&
                    (x.rerunnable || runSingleUse));

                _availableScienceExperiments[s.ScienceExperiment.id] = IsAvailable;
                return IsAvailable;
            }

            if (_moduleScienceExperiments != null && _moduleScienceExperiments.Count > 0)
            {
                IEnumerable<ModuleScienceExperiment> lm = _moduleScienceExperiments.Where(x => (
                    x.experimentID == s.ScienceExperiment.id &&
                    !(x.GetScienceCount() > 0) &&
                    (x.rerunnable || runSingleUse) &&
                    !x.Inoperable
                    ));

                IsAvailable = lm.Count() != 0;
                _availableScienceExperiments[s.ScienceExperiment.id] = IsAvailable;
            }
            return IsAvailable;
        }



        public ModuleScienceExperiment FindExperiment(ScienceInstance s, bool runSingleUse = true)
        {
            ModuleScienceExperiment m = null;
            if (_moduleScienceExperiments != null && _moduleScienceExperiments.Count > 0)
            {
                IEnumerable<ModuleScienceExperiment> lm = _moduleScienceExperiments.Where(x => (
                    x.experimentID == s.ScienceExperiment.id &&
                    !(x.GetScienceCount() > 0) &&
                    (x.rerunnable || runSingleUse) &&
                    !x.Inoperable
                    ));
                if (lm.Count() != 0)
                    m = lm.First();
            }
            return m;
        }



        public void RunExperiment(ScienceInstance s, bool runSingleUse = true)
        {
            //_logger.Trace( "Finding Module for Science Report: " + s.ScienceExperiment.id );
            ModuleScienceExperiment m = null;



            // If possible run with DMagic new API
            IEnumerable<ModuleScienceExperiment> lm = FindDMAnimateGenericsForExperiment(s.ScienceExperiment.id);
            if (lm != null && lm.Any())
            {
                DMModuleScienceAnimateGeneric NewDMagicInstance = _parent.DMagic.GetDMModuleScienceAnimateGeneric();
                m = lm.FirstOrDefault(x =>
                    (int)x.Fields.GetValue("experimentsLimit") > 1 ? NewDMagicInstance.canConduct(x) : NewDMagicInstance.canConduct(x) &&
                    (x.rerunnable || runSingleUse));

                if (m != null)
                {
                    _logger.Debug("Running DMModuleScienceAnimateGenerics Experiment " + m.experimentID + " on part " + m.part.partInfo.name);
                    NewDMagicInstance.gatherScienceData(m, !ScienceChecklistAddon.Config.ShowResultsWindow);
                }

                return;
            }



            // If possible run with DMagic DMAPI
            if (_DMModuleScienceAnimates != null && _DMModuleScienceAnimates.Count > 0)
            {
                DMAPI DMAPIInstance = _parent.DMagic.GetDMAPI();
                if (DMAPIInstance != null)
                {
                    IEnumerable<ModuleScienceExperiment> lm2 = _DMModuleScienceAnimates.Where(x => x.experimentID == s.ScienceExperiment.id);
                    if (lm2.Any())
                    {
                        m = lm2.FirstOrDefault(x =>
                        {
                            return !x.Inoperable &&
                            ((int)x.Fields.GetValue("experimentLimit") > 1 ? DMAPIInstance.experimentCanConduct(x) : DMAPIInstance.experimentCanConduct(x) &&
                            (x.rerunnable || runSingleUse));
                        });

                        if (m != null)
                        {
                            //_logger.Trace("Running DMModuleScienceAnimates Experiment " + m.experimentID + " on part " + m.part.partInfo.name);
                            DMAPIInstance.deployDMExperiment(m, !ScienceChecklistAddon.Config.ShowResultsWindow);
                        }

                        return;
                    }
                }
            }



            // Do stock run
            m = FindExperiment(s, runSingleUse);
            if (m != null)
            {
                //_logger.Trace( "Running Experiment " + m.experimentID + " on part " + m.part.partInfo.name );
                RunStandardModuleScienceExperiment(m);
                return;
            }


        }



        public void RunStandardModuleScienceExperiment(ModuleScienceExperiment exp)
        {
            if (exp.Inoperable) return;

            if (ScienceChecklistAddon.Config.ShowResultsWindow)
                exp.DeployExperiment();
            else
            {
                if (!exp.useStaging)
                {
                    exp.useStaging = true;
                    exp.OnActive();
                    exp.useStaging = false;
                }
                else
                    exp.OnActive();
            }
        }



        public IEnumerable<ModuleScienceExperiment> FindDMAnimateGenericsForExperiment(string experimentId)
        {
            if (_DMModuleScienceAnimateGenerics != null && _DMModuleScienceAnimateGenerics.Count > 0)
            {
                DMModuleScienceAnimateGeneric NewDMagicInstance = _parent.DMagic.GetDMModuleScienceAnimateGeneric();
                if (NewDMagicInstance != null)
                {
                    return _DMModuleScienceAnimateGenerics.Where(x => x.experimentID == experimentId);
                }
            }

            return null;
        }



        public WindowSettings BuildSettings()
        {
            //_logger.Info( "BuildSettings" );
            WindowSettings W = new WindowSettings(ScienceChecklistAddon.WINDOW_NAME_STATUS);
            W.Set("Top", (int)windowPos.yMin);
            W.Set("Left", (int)windowPos.xMin);
            W.Set("Visible", IsVisible());

            return W;
        }



        public void ApplySettings(WindowSettings W)
        {
            windowPos.yMin = W.GetInt("Top", 40);
            windowPos.xMin = W.GetInt("Left", 40);
            windowPos.yMax = windowPos.yMin + wScale(30);
            windowPos.xMax = windowPos.xMin + wScale(250);


            bool TempVisible = false;
            TempVisible = W.GetBool("Visible", false);
            if (TempVisible)
                OnOpenEvent(this, EventArgs.Empty);
            else
                OnCloseEvent(this, EventArgs.Empty);
        }




        /*

                /// </summary>


                /// <summary>
                /// Run one instance of each type of experiment on the vessel
                /// </summary>
                /// <param name="onlyIncomplete">
                /// Only run experiments that aren't complete.
                /// </param>
                public void RunExperimentsOnce(bool onlyIncomplete)
                {
                    IList<ScienceInstance> dsi = _filter.DisplayScienceInstances.Where(x => (!x.IsCollected || !onlyIncomplete)).ToList();

                    foreach (ScienceInstance s in dsi)
                    {
                        RunExperiment(s);
                    }
                }

                /// <summary>
                /// Run every single experiment on a vessel
                /// </summary>
                /// <param name="onlyIncomplete">
                /// Only run experiments that aren't complete.
                /// </param>
                public void RunEveryExperiment(bool onlyIncomplete)
                {
                    IList<ScienceInstance> dsi = _filter.DisplayScienceInstances.Where(x => (!x.IsCollected || !onlyIncomplete)).ToList();

                    foreach (ScienceInstance s in dsi)
                    {
                        if (ModuleScienceExperiments != null && ModuleScienceExperiments.Count > 0)
                        {
                            IEnumerable<ModuleScienceExperiment> lm = ModuleScienceExperiments.Where(x => (x.experimentID == s.ScienceExperiment.id && !(x.GetScienceCount() > 0) && !x.Inoperable));
                            foreach (ModuleScienceExperiment mse in lm)
                            {
                                _logger.Trace("Running Experiment " + mse.experimentID + " on part " + mse.part.partInfo.name);
                                RunStandardModuleScienceExperiment(mse);
                            }
                        }

                        if (DMModuleScienceAnimates != null && DMModuleScienceAnimates.Count > 0)
                        {
                            IEnumerable<ModuleScienceExperiment> lm = DMModuleScienceAnimates.Where(x => (x.experimentID == s.ScienceExperiment.id && !x.Inoperable && DMagic.DMAPI.experimentCanConduct(x)));
                            foreach (ModuleScienceExperiment mse in lm)
                            {
                                _logger.Trace("Running DMModuleScienceAnimates Experiment " + mse.experimentID + " on part " + mse.part.partInfo.name);
                                DMagic.DMAPI.deployDMExperiment(mse);
                            }
                        }

                        if (DMModuleScienceAnimateGenerics != null && DMModuleScienceAnimateGenerics.Count > 0)
                        {
                            IEnumerable<ModuleScienceExperiment> lm = DMModuleScienceAnimateGenerics.Where(x => (x.experimentID == s.ScienceExperiment.id && !x.Inoperable && DMagic.DMModuleScienceAnimateGeneric.canConduct(x)));
                            foreach (ModuleScienceExperiment mse in lm)
                            {
                                _logger.Trace("Running DMModuleScienceAnimateGenerics Experiment " + mse.experimentID + " on part " + mse.part.partInfo.name);
                                DMagic.DMModuleScienceAnimateGeneric.gatherScienceData(mse);
                            }
                        }
                    }
                }

        }*/

















    }
}
