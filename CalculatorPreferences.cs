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
			Gtk.Label method_label = new Gtk.Label (Catalog.GetString (
				"Choose calculation method:"));
			method_label.Wrap = true;
			method_label.Xalign = 0;
			PackStart (method_label);

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
						
			//Decimal settings
			
			
			Gtk.Label decimal_label = new Gtk.Label (Catalog.GetString (
				"Choose number of decimals:"));
			decimal_label.Wrap = true;
			decimal_label.Xalign = 0;
			PackStart (decimal_label);
			
			Gtk.SpinButton decimal_spinner = new Gtk.SpinButton (1, 12, 1);
			
			int decimal_count;
			try {
				decimal_count = (int) Preferences.Get (CalculatorAddin.CALCULATOR_DECIMAL_COUNT);
			} catch (Exception) {
				Logger.Debug("CalcAddin: Couldn't find a preference for decimal count.");
				decimal_count = 3;				//Defaults to 3 if no preference is set.
			}
			
			
			decimal_spinner.Value = decimal_count <= 12 ? decimal_count : 3;
			
			PackStart (decimal_spinner);
			decimal_spinner.Show();
			
			decimal_spinner.ValueChanged += OnDecimalValueChanged;
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
		
		//Sets the preference once the button is toggled
		private void OnDecimalValueChanged(object sender, EventArgs args)
		{
			Gtk.SpinButton spinner = sender as SpinButton;
			Preferences.Set (CalculatorAddin.CALCULATOR_DECIMAL_COUNT, spinner.ValueAsInt);
			Logger.Debug("CalcAddin: Changing decimal count preference");
		}
	}
}