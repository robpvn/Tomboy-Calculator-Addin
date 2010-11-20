/*
 * CalculatorPreferences.cs: Allows a user to choose between automatic and manual 
 * execution of calculations. Defaults to manual.
 *
 * Author:
 * Robert Nordan (rpvn@robpvn.net)
 *
 * Copyright (C) 2010 Robert Nordan, licensed under the GPL
*/

using System;
using Mono.Unix;
using Gtk;
using Tomboy;

namespace Tomboy.Calculator
{
		
	public class CalculatorPreferences : Gtk.VBox
	{
		
		Gtk.RadioButton auto_radio;
		Gtk.RadioButton alternate_radio;
		
		public CalculatorPreferences()
		{
			// TextLabel
			Gtk.Label label = new Gtk.Label (Catalog.GetString (
				"Choose calculation method:"));
			label.Wrap = true;
			label.Xalign = 0;
			PackStart (label);

			// Radio buttons
			auto_radio = new Gtk.RadioButton (Catalog.GetString (
				"Automatic"));
			PackStart (auto_radio);
			
			alternate_radio = new Gtk.RadioButton (auto_radio, Catalog.GetString("Manual"));
			PackStart(alternate_radio);
			
			//Check if the preferences have been set earlier and adjust buttons
			if (Preferences.Get(CalculatorAddin.CALCULATOR_AUTOMATIC_MODE) == null) {
				alternate_radio.Active = true;;
			}else if((bool) Preferences.Get(CalculatorAddin.CALCULATOR_AUTOMATIC_MODE)) {
				auto_radio.Active = true;	
			}else{
				alternate_radio.Active = true;
			}
			
			auto_radio.Toggled += OnSelectedRadioToggled;
		}
		
		//Sets the preference once the button is toggled
		private void OnSelectedRadioToggled(object sender, EventArgs args)
		{			
			if (auto_radio.Active) {
				Preferences.Set (CalculatorAddin.CALCULATOR_AUTOMATIC_MODE,
					true);
				Logger.Debug("CalcAddin: turning automatic mode on");
			}else
			{
				Preferences.Set (CalculatorAddin.CALCULATOR_AUTOMATIC_MODE,
					false);
				Logger.Debug("CalcAddin: turning automatic mode off");
			}
		}
	}
}