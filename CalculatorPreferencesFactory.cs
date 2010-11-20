/*
* CalculatorPreferencesFactory.cs: Creates a widget that will be used in
* the addin's preferences dialog.
* 
* Author:
* Robert Nordan (rpvn@robpvn.net)
*
* Copyright (C) 2010 Robert Nordan, licensed under the GPL
*/

using System;
using Tomboy;

namespace Tomboy.Calculator {
	public class CalculatorPreferencesFactory : AddinPreferenceFactory {
		public override Gtk.Widget CreatePreferenceWidget ()
		{
			return new CalculatorPreferences ();
		}
	}
}