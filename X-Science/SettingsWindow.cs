using System;
using UnityEngine;
using KSP.Localization;


namespace ScienceChecklist
{
	class SettingsWindow : Window<ScienceChecklistAddon>
	{
		private readonly string version;
		private GUIStyle labelStyle;
		private GUIStyle toggleStyle;
		private GUIStyle sliderStyle;
		private GUIStyle editStyle;
		private GUIStyle versionStyle;
		private GUIStyle selectionStyle;

		private readonly Logger _logger;
		private readonly ScienceChecklistAddon _parent;

		// Localization strings
		private static string SettingWindow_title = Localizer.Format("#XScience_SettingWindow_Title");
		private static string SettingWindow_toggle1 = Localizer.Format("#XScience_SettingWindow_toggle1_title");
		private static string SettingWindow_toggle1_tip = Localizer.Format("#XScience_SettingWindow_toggle1_title");
		private static string SettingWindow_toggle2 = Localizer.Format("#XScience_SettingWindow_toggle2_title");
		private static string SettingWindow_toggle2_tip = Localizer.Format("#XScience_SettingWindow_toggle2_tip");
		private static string SettingWindow_toggle3 = Localizer.Format("#XScience_SettingWindow_toggle3_title");
		private static string SettingWindow_toggle3_tip = Localizer.Format("#XScience_SettingWindow_toggle3_tip");
		private static string SettingWindow_toggle4 = Localizer.Format("#XScience_SettingWindow_toggle4_title");
		private static string SettingWindow_toggle4_tip = Localizer.Format("#XScience_SettingWindow_toggle4_tip");
		private static string SettingWindow_toggle5 = Localizer.Format("#XScience_SettingWindow_toggle5_title");
		private static string SettingWindow_toggle5_tip = Localizer.Format("#XScience_SettingWindow_toggle5_tip");
		private static string SettingWindow_toggle6 = Localizer.Format("#XScience_SettingWindow_toggle6_title");
		private static string SettingWindow_toggle6_tip = Localizer.Format("#XScience_SettingWindow_toggle6_tip");
		private static string SettingWindow_toggle7 = Localizer.Format("#XScience_SettingWindow_toggle7_title");
		private static string SettingWindow_toggle7_tip = Localizer.Format("#XScience_SettingWindow_toggle7_tip");
		private static string SettingWindow_toggle8 = Localizer.Format("#XScience_SettingWindow_toggle8_title");
		private static string SettingWindow_toggle8_tip = Localizer.Format("#XScience_SettingWindow_toggle8_tip");
		private static string SettingWindow_toggle9 = Localizer.Format("#XScience_SettingWindow_toggle9_title");
		private static string SettingWindow_toggle9_tip = Localizer.Format("#XScience_SettingWindow_toggle9_tip");
		private static string SettingWindow_toggle10 = Localizer.Format("#XScience_SettingWindow_toggle10_title");
		private static string SettingWindow_toggle10_tip = Localizer.Format("#XScience_SettingWindow_toggle10_tip");
		private static string SettingWindow_RightClicktips = Localizer.Format("#XScience_SettingWindow_RightClicktips");
		private static string SettingWindow_MuteMusic = Localizer.Format("#XScience_SettingWindow_MuteMusic");
		private static string SettingWindow_MuteMusic_tip = Localizer.Format("#XScience_SettingWindow_MuteMusic_tip");
		private static string SettingWindow_OpenHNWindow = Localizer.Format("#XScience_SettingWindow_OpenHNWindow");
		private static string SettingWindow_OpenHNWindow_tip = Localizer.Format("#XScience_SettingWindow_OpenHNWindow_tip");
		private static string SettingWindow_AdjustUI = Localizer.Format("#XScience_SettingWindow_AdjustUI");
		private static string SettingWindow_AdjustUI_tip = Localizer.Format("#XScience_SettingWindow_AdjustUI_tip");



        // Constructor
        public SettingsWindow( ScienceChecklistAddon Parent )
			: base( SettingWindow_title, 240, 360 ) // "[x] Science! Settings"
		{
			_logger = new Logger( this );
			_parent = Parent;
			UiScale = 1; // Don't let this change
			version = Utilities.GetDllVersion( this );
		}


		// For our Window base class
		protected override void ConfigureStyles( )
		{
			base.ConfigureStyles( );

			if( labelStyle == null )
			{
				labelStyle = new GUIStyle( _skin.label );
				labelStyle.wordWrap = false;
				labelStyle.fontStyle = FontStyle.Normal;
				labelStyle.normal.textColor = Color.white;

				toggleStyle = new GUIStyle( _skin.toggle );
				sliderStyle = new GUIStyle( _skin.horizontalSlider );
				editStyle = new GUIStyle( _skin.textField );
				versionStyle = Utilities.GetVersionStyle( );
				selectionStyle = new GUIStyle( _skin.button );
				selectionStyle.margin = new RectOffset( 30, 0, 0, 0 );
			}
		}



		// For our Window base class
		protected override void DrawWindowContents( int windowID )
		{
			GUILayout.BeginVertical( );

			bool save = false;
			var toggle = GUILayout.Toggle( ScienceChecklistAddon.Config.HideCompleteExperiments, new GUIContent( SettingWindow_toggle1,  SettingWindow_toggle1_tip), toggleStyle ); // "Hide complete experiments""Experiments considered complete will not be shown."
			if( toggle != ScienceChecklistAddon.Config.HideCompleteExperiments )
			{
				ScienceChecklistAddon.Config.HideCompleteExperiments = toggle;
				save = true;
			}

			toggle = GUILayout.Toggle( ScienceChecklistAddon.Config.CompleteWithoutRecovery, new GUIContent( SettingWindow_toggle2,  SettingWindow_toggle2_tip), toggleStyle ); // "Complete without recovery""Show experiments as completed even if they have not been recovered yet.\nYou still need to recover the science to get the points!\nJust easier to see what is left."
			if( toggle != ScienceChecklistAddon.Config.CompleteWithoutRecovery )
			{
				ScienceChecklistAddon.Config.CompleteWithoutRecovery = toggle;
				save = true;
			}

			toggle = GUILayout.Toggle(ScienceChecklistAddon.Config.CheckUnloadedVessels, new GUIContent( SettingWindow_toggle3, SettingWindow_toggle3_tip), toggleStyle); // "Check unloaded vessels""Unloaded vessels will be checked for recoverable science."
			if( toggle != ScienceChecklistAddon.Config.CheckUnloadedVessels )
			{
				ScienceChecklistAddon.Config.CheckUnloadedVessels = toggle;
				save = true;
			}

			toggle = GUILayout.Toggle(ScienceChecklistAddon.Config.CheckDebris, new GUIContent(SettingWindow_toggle4, SettingWindow_toggle4_tip), toggleStyle); // "Check debris""Vessels marked as debris will be checked for recoverable science."
			if (toggle != ScienceChecklistAddon.Config.CheckDebris)
			{
				ScienceChecklistAddon.Config.CheckDebris = toggle;
				save = true;
			}

			toggle = GUILayout.Toggle(ScienceChecklistAddon.Config.HideMinScienceSlider, new GUIContent(SettingWindow_toggle5, SettingWindow_toggle5_tip), toggleStyle); // "Hide Min Science Slider""Hide the minimum science slider in the Here & Now window"
			if (toggle != ScienceChecklistAddon.Config.HideMinScienceSlider)
			{
				ScienceChecklistAddon.Config.HideMinScienceSlider = toggle;
				save = true;
			}


			toggle = GUILayout.Toggle(ScienceChecklistAddon.Config.VeryLowMinScience, new GUIContent(SettingWindow_toggle6, SettingWindow_toggle6_tip), toggleStyle); // "Low Min Science""Minimum science slider in the Here & Now window will go fom 0.0001 to 0.1"
			if (toggle != ScienceChecklistAddon.Config.VeryLowMinScience)
			{
				ScienceChecklistAddon.Config.VeryLowMinScience = toggle;
				save = true;
			}

			toggle = GUILayout.Toggle( ScienceChecklistAddon.Config.AllFilter, new GUIContent( SettingWindow_toggle7,  SettingWindow_toggle7_tip), toggleStyle ); // "Allow all filter""Adds a filter button showing all experiments, even on unexplored bodies using unavailable instruments.\nMight be considered cheating."
			if( toggle != ScienceChecklistAddon.Config.AllFilter )
			{
				ScienceChecklistAddon.Config.AllFilter = toggle;
				save = true;
			}

			toggle = GUILayout.Toggle( ScienceChecklistAddon.Config.FilterDifficultScience, new GUIContent( SettingWindow_toggle8,  SettingWindow_toggle8_tip), toggleStyle ); // "Filter difficult science""Hide a few experiments such as flying at stars and gas giants that are almost impossible.\n Also most EVA reports before upgrading Astronaut Complex."
			if( toggle != ScienceChecklistAddon.Config.FilterDifficultScience )
			{
				ScienceChecklistAddon.Config.FilterDifficultScience = toggle;
				save = true;
			}

			toggle = GUILayout.Toggle( ScienceChecklistAddon.Config.SelectedObjectWindow, new GUIContent( SettingWindow_toggle9, SettingWindow_toggle9_tip ), toggleStyle ); // "Selected Object Window""Show the Selected Object Window in the Tracking Station."
			if( toggle != ScienceChecklistAddon.Config.SelectedObjectWindow )
			{
				ScienceChecklistAddon.Config.SelectedObjectWindow = toggle;
				save = true;
			}

#if false
			if( BlizzysToolbarButton.IsAvailable )
			{
				toggle = GUILayout.Toggle( ScienceChecklistAddon.Config.UseBlizzysToolbar, new GUIContent( "Use blizzy78's toolbar", "Remove [x] Science button from stock toolbar and add to blizzy78 toolbar." ), toggleStyle );
				if( toggle != ScienceChecklistAddon.Config.UseBlizzysToolbar )
				{
					ScienceChecklistAddon.Config.UseBlizzysToolbar = toggle;
					save = true;
				}
			}
#endif
			GUILayout.Space(2);
			int selected = 0;
			if( !ScienceChecklistAddon.Config.RighClickMutesMusic )
				selected = 1;
			int new_selected = selected;
			GUILayout.Label( SettingWindow_RightClicktips, labelStyle ); // "Right click [x] icon"
			GUIContent[] Options = {
				new GUIContent( SettingWindow_MuteMusic, SettingWindow_MuteMusic_tip ), // "Mute music""Here & Now window gets its own icon"
				new GUIContent( SettingWindow_OpenHNWindow, SettingWindow_OpenHNWindow_tip ) // "Opens Here & Now window""Here & Now icon is hidden"
			};
			new_selected = GUILayout.SelectionGrid( selected, Options, 1, selectionStyle );
			if( new_selected != selected )
			{
				if( new_selected == 0 )
					ScienceChecklistAddon.Config.RighClickMutesMusic = true;
				else
					ScienceChecklistAddon.Config.RighClickMutesMusic = false;
				save = true;
			}

			if( ScienceChecklistAddon.Config.RighClickMutesMusic )
			{
				toggle = GUILayout.Toggle( ScienceChecklistAddon.Config.MusicStartsMuted, new GUIContent( SettingWindow_toggle10, SettingWindow_toggle10_tip ), toggleStyle ); // "Music starts muted""Title music is silenced upon load.  No need to right click"
				if( toggle != ScienceChecklistAddon.Config.MusicStartsMuted )
				{
					ScienceChecklistAddon.Config.MusicStartsMuted = toggle;
					save = true;
				}
			}

			GUILayout.Space(2);
			GUILayout.Label(new GUIContent( SettingWindow_AdjustUI, SettingWindow_AdjustUI_tip ), labelStyle ); // "Adjust UI size""Adjusts the the UI scaling."
			var slider = GUILayout.HorizontalSlider( ScienceChecklistAddon.Config.UiScale, 1, 2 );
			if( slider != ScienceChecklistAddon.Config.UiScale )
			{
				ScienceChecklistAddon.Config.UiScale = slider;
				save = true;
			}

			if( save )
				ScienceChecklistAddon.Config.Save( );

			GUILayout.EndVertical( );
			GUILayout.Space(10);
			GUI.Label( new Rect( 4, windowPos.height - 13, windowPos.width - 20, 12 ), "[x] Science! V" + version, versionStyle );
		}
	}
}
