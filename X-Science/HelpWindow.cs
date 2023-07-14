using System;
using UnityEngine;
using KSP.Localization;


namespace ScienceChecklist
{
	class HelpWindow : Window<ScienceChecklistAddon>
	{
		private static string str_HelpWindowTitle = Localizer.Format("#XScience_HelpWindow_Title");
		private static string str_HelpWindowAuthInfo = Localizer.Format("#XScience_HelpWindow_AuthorInfo");
		private static string str_About = Localizer.Format("#XScience_HelpWindow_About");
		private static string str_About_Content = Localizer.Format("#XScience_HelpWindow_About_Content");
		private static string str_Buttons_Instruct = Localizer.Format("#XScience_HelpWindow_Buttons_Instruct");
		private static string str_Button1_desc = Localizer.Format("#XScience_HelpWindow_Button1");
		private static string str_Button2_desc = Localizer.Format("#XScience_HelpWindow_Button2");
		private static string str_Button3_desc = Localizer.Format("#XScience_HelpWindow_Button3");
		private static string str_Button4_desc = Localizer.Format("#XScience_HelpWindow_Button4");
		private static string str_TextFilter = Localizer.Format("#XScience_HelpWindow_TextFilter");
		private static string str_TextFiltertips = Localizer.Format("#XScience_HelpWindow_TextFilter_tips");
		private static string str_TextFiltertips_NOT = Localizer.Format("#XScience_HelpWindow_TextFilter_NOT");
		private static string str_TextFiltertips_OR = Localizer.Format("#XScience_HelpWindow_TextFilter_OR");
		private static string str_TextFiltertips_Hover = Localizer.Format("#XScience_HelpWindow_TextFilter_Hover");
		private static string str_TextFiltertips_Clear = Localizer.Format("#XScience_HelpWindow_TextFilter_Clear");
		private static string str_Settings_Instruct = Localizer.Format("#XScience_HelpWindow_Settings_Instruct");
		private static string str_Settings_Hide = Localizer.Format("#XScience_HelpWindow_Settings_Hide");
		private static string str_Settings_NotRecover = Localizer.Format("#XScience_HelpWindow_Settings_NotRecover");
		private static string str_Settings_debris = Localizer.Format("#XScience_HelpWindow_Settings_debris");
		private static string str_Settings_filterall = Localizer.Format("#XScience_HelpWindow_Settings_Filterall");
		private static string str_Settings_FilterDifficult = Localizer.Format("#XScience_HelpWindow_Settings_FilterDifficult");
		private static string str_Settings_usetoolar = Localizer.Format("#XScience_HelpWindow_Settings_usetoolar");
		private static string str_Settings_hideormute = Localizer.Format("#XScience_HelpWindow_Settings_hideormute");
		private static string str_Settings_mute = Localizer.Format("#XScience_HelpWindow_Settings_mute");
		private static string str_Settings_scaling = Localizer.Format("#XScience_HelpWindow_Settings_scaling");
		private static string str_HelpWindow_HereNowWindow = Localizer.Format("#XScience_HelpWindow_HereNowWindow");
		private static string str_HelpWindow_NowWindow = Localizer.Format("#XScience_HelpWindow_NowWindow");
		private static string str_HelpWindow_NowWindowtip = Localizer.Format("#XScience_HelpWindow_nowwindowtip");
		private static string str_HelpWindow_NowWindowtip2 = Localizer.Format("#XScience_HelpWindow_nowwindowtip2");
		private static string str_HelpWindow_NowWindowtip3 = Localizer.Format("#XScience_HelpWindow_nowwindowtip3");
		private static string str_HelpWindow_Didyouknow = Localizer.Format("#XScience_HelpWindow_Didyouknow");
		private static string str_HelpWindow_Knows1 = Localizer.Format("#XScience_HelpWindow_Knows1");
		private static string str_HelpWindow_Knows2 = Localizer.Format("#XScience_HelpWindow_Knows2");
		private static string str_HelpWindow_Knows3 = Localizer.Format("#XScience_HelpWindow_Knows3");
		private static string str_HelpWindow_Knows4 = Localizer.Format("#XScience_HelpWindow_Knows4");
		private static string str_HelpWindow_Knows5 = Localizer.Format("#XScience_HelpWindow_Knows5");
		private GUIStyle labelStyle;
		private GUIStyle sectionStyle;
		private Vector2 scrollPosition;
		private readonly ScienceChecklistAddon	_parent;



		public HelpWindow( ScienceChecklistAddon Parent )
			: base(str_HelpWindowTitle, 500, Screen.height * 0.75f  / ScienceChecklistAddon.Config.UiScale ) // "[x] Science! Help"
		{
			_parent = Parent;
			UiScale = ScienceChecklistAddon.Config.UiScale;
			scrollPosition = Vector2.zero;
			ScienceChecklistAddon.Config.UiScaleChanged += OnUiScaleChange;
		}



		protected override void ConfigureStyles( )
		{
			base.ConfigureStyles();

			if( labelStyle == null )
			{
				labelStyle = new GUIStyle( _skin.label );
				labelStyle.wordWrap = true;
				labelStyle.fontStyle = FontStyle.Normal;
				labelStyle.normal.textColor = Color.white;
				labelStyle.stretchWidth = true;
				labelStyle.stretchHeight = false;
				labelStyle.margin.bottom -= wScale( 2 );
				labelStyle.padding.bottom -= wScale( 2 );
			}

			if( sectionStyle == null )
			{
				sectionStyle = new GUIStyle( labelStyle );
				sectionStyle.fontStyle = FontStyle.Bold;
			}
		}



		private void OnUiScaleChange( object sender, EventArgs e )
		{
			UiScale = ScienceChecklistAddon.Config.UiScale;
			labelStyle = null;
			sectionStyle = null;
			base.OnUiScaleChange( );
			ConfigureStyles( );
		}



		protected override void DrawWindowContents( int windowID )
		{
			scrollPosition = GUILayout.BeginScrollView( scrollPosition );
			GUILayout.BeginVertical( GUILayout.ExpandWidth( true ) );

			GUILayout.Label(str_HelpWindowAuthInfo, sectionStyle, GUILayout.ExpandWidth( true ) ); // "[x] Science! by Z-Key Aerospace and Bodrick."

			GUILayout.Space( wScale( 30 ) );
			GUILayout.Label(str_About, sectionStyle, GUILayout.ExpandWidth(true)); // "About"
			GUILayout.Label(str_About_Content, labelStyle, GUILayout.ExpandWidth( true ) ); // "[x] Science! creates a list of all possible science.  Use the list to find what is possible, to see what is left to accomplish, to decide where your Kerbals are going next."

			GUILayout.Space( wScale( 20 ) );
			GUILayout.Label( str_Buttons_Instruct, sectionStyle, GUILayout.ExpandWidth( true ) ); // "The four filter buttons at the bottom of the window are"
			GUILayout.Label( str_Button1_desc, labelStyle, GUILayout.ExpandWidth( true ) ); // "* Show experiments available right now – based on you current ship and its situation"
			GUILayout.Label( str_Button2_desc, labelStyle, GUILayout.ExpandWidth( true ) ); // "* Show experiments available on this vessel – based on your ship but including all known biomes",
			GUILayout.Label( str_Button3_desc, labelStyle, GUILayout.ExpandWidth( true ) ); // "* Show all unlocked experiments – based on instruments you have unlocked and celestial bodies you have visited."
			GUILayout.Label( str_Button4_desc, labelStyle, GUILayout.ExpandWidth( true ) ); // "* Show all experiments – shows everything.  You can hide this button"

			GUILayout.Space( wScale( 20 ) );
			GUILayout.Label( str_TextFilter, sectionStyle, GUILayout.ExpandWidth( true ) ); // "The text filter"
			GUILayout.Label( str_TextFiltertips, labelStyle, GUILayout.ExpandWidth( true ) ); //"To narrow your search, you may enter text into the filter eg \"kerbin’s shores\""
			GUILayout.Label( str_TextFiltertips_NOT, labelStyle, GUILayout.ExpandWidth( true ) ); // "Use – to mean NOT eg \"mun space -near\""
			GUILayout.Label( str_TextFiltertips_OR, labelStyle, GUILayout.ExpandWidth( true ) ); // "Use | to mean OR eg \"mun|minmus space\""
			GUILayout.Label( str_TextFiltertips_Hover, labelStyle, GUILayout.ExpandWidth( true ) ); // "Hover the mouse over the \"123/456 completed\" text.  A pop-up will show more infromation."
			GUILayout.Label( str_TextFiltertips_Clear, labelStyle, GUILayout.ExpandWidth( true ) ); // "Press the X button to clear your text filter."

			GUILayout.Space( wScale( 20 ) );
			GUILayout.Label( str_Settings_Instruct, sectionStyle, GUILayout.ExpandWidth( true ) ); // "The settings are"
			GUILayout.Label( str_Settings_Hide, labelStyle, GUILayout.ExpandWidth( true ) ); // "* Hide complete experiments – Any science with a full green bar is hidden.  It just makes it easier to see what is left to do."
			GUILayout.Label( str_Settings_NotRecover, labelStyle, GUILayout.ExpandWidth( true ) ); // "* Complete without recovery – Consider science in your spaceships as if it has been recovered.  You still need to recover to get the points.  It just makes it easier to see what is left to do."
			GUILayout.Label( str_Settings_debris, labelStyle, GUILayout.ExpandWidth( true ) ); // "* Check debris – Science that survived a crash will be visible.  You may still be able to recover it."
			GUILayout.Label( str_Settings_filterall, labelStyle, GUILayout.ExpandWidth( true ) ); // "* Allow all filter – The \"All\" filter button shows science on planets you have never visited using instruments you have not invented yet.  Some people may consider it overpowered.  If you feel like a cheat, turn it off."
			GUILayout.Label( str_Settings_FilterDifficult, labelStyle, GUILayout.ExpandWidth( true ) ); // "* Filter difficult science – Hide science that is practically impossible.  Flying at stars, that kinda thing."
			GUILayout.Label( str_Settings_usetoolar, labelStyle, GUILayout.ExpandWidth( true ) ); // "* Use blizzy78's toolbar – If you have blizzy78’s toolbar installed then place the [x] Science! button on that instead of the stock \"Launcher\" toolbar."
			GUILayout.Label( str_Settings_hideormute, labelStyle, GUILayout.ExpandWidth( true ) ); // "* Right click [x] icon – Choose to open the Here and Now window by right clicking.  Hides the second window.  Otherwise mute music."
			GUILayout.Label(str_Settings_mute, labelStyle, GUILayout.ExpandWidth( true ) ); // "* Music starts muted – Music is muted on load."
			GUILayout.Label( str_Settings_scaling, labelStyle, GUILayout.ExpandWidth( true ) ); // "* Adjust UI Size – Change the scaling of the UI."

			GUILayout.Space( wScale( 20 ) );
			GUILayout.Label( str_HelpWindow_HereNowWindow, sectionStyle, GUILayout.ExpandWidth( true ) ); // "Here and Now Window"
			GUILayout.Label( str_HelpWindow_NowWindow, labelStyle, GUILayout.ExpandWidth( true ) ); // "The Here and Now Window will stop time-warp, display an alert message and play a noise when you enter a new situation.  To prevent this, close the window."
			GUILayout.Label( str_HelpWindow_NowWindowtip, labelStyle, GUILayout.ExpandWidth( true ) ); // "The Here and Now Window will show all outstanding experiments for the current situation that are possible with the current ship."
			GUILayout.Label( str_HelpWindow_NowWindowtip2, labelStyle, GUILayout.ExpandWidth( true ) ); // "To run an experiment, click the button.  If the button is greyed-out then you may need to reset the experiment or recover or transmit the science."
			GUILayout.Label( str_HelpWindow_NowWindowtip3, labelStyle, GUILayout.ExpandWidth( true ) ); // "To perform an EVA report or surface sample, first EVA your Kerbal.  The window will react, allowing those buttons to be clicked."

			GUILayout.Space( wScale( 20 ) );
			GUILayout.Label( str_HelpWindow_Didyouknow, sectionStyle, GUILayout.ExpandWidth( true ) ); // "Did you know? (includes spoilers)"
			GUILayout.Label( str_HelpWindow_Knows1, labelStyle, GUILayout.ExpandWidth( true ) ); // "* In the VAB editor you can use the filter \"Show experiments available on this vessel\" to see what your vessel could collect before you launch it."
			GUILayout.Label( str_HelpWindow_Knows2, labelStyle, GUILayout.ExpandWidth( true ) ); // "* Does the filter \"mun space high\" show mun’s highlands?  – use \"mun space –near\" instead."
			GUILayout.Label( str_HelpWindow_Knows3, labelStyle, GUILayout.ExpandWidth( true ) ); // "* Need more science?  Go to Minmus.  It’s a little harder to get to but your fuel will last longer.  A single mission can collect thousands of science points before you have to come back."
			GUILayout.Label( str_HelpWindow_Knows4, labelStyle, GUILayout.ExpandWidth( true ) ); // "* Generally moons are easier - it is more efficient to collect science from the surface of Ike or Gilly than from Duna or Eve.  That said - beware Tylo, it's big and you can't aerobrake."
			GUILayout.Label( str_HelpWindow_Knows5, labelStyle, GUILayout.ExpandWidth( true ) ); // "* Most of Kerbin’s biomes include both splashed and landed situations.  Landed at Kerbin’s water?  First build an aircraft carrier."

			GUILayout.EndVertical( );
			GUILayout.EndScrollView( );

			GUILayout.Space( wScale( 8 ) );
		}
	}
}
