/*
 * CalculatorAddin.cs: Allows a user to mark equations in the note's text and 
 * have the answer inserted in the text, or have them found and calculated 
 * automatically.
 *
 * Author:
 * Robert Nordan (rpvn@robpvn.net)
 *
 * Copyright (C) 2010 Robert Nordan, licensed under the GPL
*/

using System;
using System.Collections.Generic;
using Mono.Unix;
using Gtk;
using Tomboy;
using System.Globalization;
using cdrnet.Lib.MathLib.Complex;
using cdrnet.Lib.MathLib.Core;
using cdrnet.Lib.MathLib.Exceptions;
using cdrnet.Lib.MathLib.Literal;
using cdrnet.Lib.MathLib.Parsing;
using cdrnet.Lib.MathLib.Scalar;
// The cdrnet libs are contained in the external referenced library MathLib.dll
// (from the Math.NET project, Classic package )


namespace Tomboy.Calculator
{
	public class CalculatorAddin : NoteAddin
	{
		public const string CALCULATOR_AUTOMATIC_MODE = "/apps/tomboy/calcaddin/footer"; 
		//The footer name is prob. from some ridiculous copy-paste, must be preserved for backwards compat.
		public const string CALCULATOR_DECIMAL_COUNT = "/apps/tomboy/calcaddin/decimalcount";
		
		Gtk.ImageMenuItem item;
		NoteBuffer buffer;
		List<Gtk.TextMark> equation_starts;
		List<Gtk.TextMark> equation_ends;
		
		public override void Initialize ()
		{	                        
		}
		
		public override void Shutdown ()
		{
			if (item != null){
				item.Activated -= OnMenuItemActivated;
				item.Hide();
			}
			 Preferences.SettingChanged -= OnPrefsChanged;
		}
		
		public override void OnNoteOpened ()
		{
			this.buffer  = Note.Buffer;		
			setUp();
			Preferences.SettingChanged += OnPrefsChanged;
		}
		
		//Checks prefs and sets up system
		private void setUp()
		{
			bool auto;
			try {
				auto = (bool) Preferences.Get(CalculatorAddin.CALCULATOR_AUTOMATIC_MODE);
			} catch (Exception e) {
				Logger.Debug(e.Message);
				auto = false;				//Defaults to manual if no preference is set.
			}
			
			if (!auto) {
				//Clean up automatic listeners in case a switch has been made with the note open
				if (buffer != null) buffer.InsertText -= OnInsertText;
				
				//Sets up a menu item and listener
				item = new Gtk.ImageMenuItem (Catalog.GetString ("Calculate Answer"));
				item.Activated += OnMenuItemActivated;
				item.AddAccelerator ("activate", Window.AccelGroup,
					(uint) Gdk.Key.e, Gdk.ModifierType.ControlMask,
					Gtk.AccelFlags.Visible);
				item.Image = new Gtk.Image (Gtk.Stock.Execute, Gtk.IconSize.Menu);
				item.Show ();
				AddPluginMenuItem (item);
			} else {
				//Clean up manual listeners in case a switch has been made with the note open
				if (item != null){
					item.Activated -= OnMenuItemActivated;
					item.Hide();
				}
				//Add listener to check for braces being typed in
				buffer.InsertText += OnInsertText;
				Logger.Debug("CalcAddin: Listening for equation brackets");
				equation_ends = new List<TextMark>();
				equation_starts = new List<TextMark>();
				
				//TODO: Set up a watcher that notes when braces are deleted and removes the marks.
				
				//TODO: Set up a method to search through for existing brackets at startup
						
			}	
		}
		
		private void OnPrefsChanged(object sender, NotifyEventArgs args)
		{
			if (args.Key == CalculatorAddin.CALCULATOR_AUTOMATIC_MODE)
				setUp();
		}
		
		//This one is used for manual calculating
		void OnMenuItemActivated (object sender, EventArgs args)
		{		
			Gtk.TextIter cursor;
			string equation_text;
				
			try {
				equation_text = buffer.Selection;
				cursor = buffer.GetIterAtMark (buffer.InsertMark);
				cursor.ForwardChars (equation_text.Length);				//Moving to insertion point.
				string toInsert = CalculateAnswer (equation_text);		//Where the calculation happens
				//Insert in text
				buffer.InsertInteractive (ref cursor, toInsert, true);			
				
			} catch (NullReferenceException e) {			//If nothing is selected.
				Console.WriteLine (e.Message);
				Console.WriteLine (e.ToString ());
				Gtk.MessageDialog unknownWarning = 
					new Gtk.MessageDialog (new Window ("Warning"), 
	                	DialogFlags.DestroyWithParent, 
	                    MessageType.Warning, 
	                    ButtonsType.Close, 
	                    Catalog.GetString ("You have not selected any text!"));
				
				unknownWarning.Run ();  
				unknownWarning.Destroy ();
				return;
			} catch (ParsingUnknownTokenException e)		//If passed something it doesn't understand.
			{
				Console.WriteLine (e.Message);
				Console.WriteLine (e.ToString ());
				Gtk.MessageDialog tokenWarning = 
					new Gtk.MessageDialog (new Window ("Warning"),
				    DialogFlags.DestroyWithParent,
				    MessageType.Warning, 
				    ButtonsType.Close, 
				    Catalog.GetString ("Not an understandable equation!"));
				
				tokenWarning.Run ();  
				tokenWarning.Destroy ();
				
			} catch (CalcNotConstantException e)			//If given letters.
			{
				Console.WriteLine (e.Message);
				Console.WriteLine (e.ToString ());
				Gtk.MessageDialog constantWarning = 
					new Gtk.MessageDialog (new Window ("Warning"), 
				    DialogFlags.DestroyWithParent, 
				    MessageType.Warning, 
				    ButtonsType.Close, 
				    Catalog.GetString ("Not an understandable equation!"));
				
				constantWarning.Run ();  
				constantWarning.Destroy ();
			} catch (Exception e)							//General failsafe to stop Tomboy from crashing.
			{
				Console.WriteLine (e.Message);
				Console.WriteLine (e.ToString ());
				Gtk.MessageDialog unknownWarning = 
					new Gtk.MessageDialog (new Window ("Warning"), 
				    DialogFlags.DestroyWithParent, 
				    MessageType.Warning, 
				    ButtonsType.Close, 
				    Catalog.GetString ("Not an understandable equation!"));
				
				unknownWarning.Run ();  
				unknownWarning.Destroy ();
			}
		}
		
		//For automatic calculating, discovery stage
	   	void OnInsertText (object sender, Gtk.InsertTextArgs args)
  	  	{
   	  	 //TODO: Make it possible to switch from brackets to something else? 
			Gtk.TextMark toInsert;
			Gtk.TextIter position;
			if (args.Text.Length == 1) {
				switch (args.Text) {
				case "[" :
					//Start of an equation identified	
					position = buffer.GetIterAtMark(buffer.InsertMark);
					toInsert = buffer.CreateMark(null, position, true);
					equation_starts.Add(toInsert);
					Logger.Debug("CalcAddin: Possible equation start identified at {0}", buffer.GetIterAtMark(toInsert).Offset);
					AutomaticCalculation();
					break;
				case "]" :
					//End of an equation identified
					position = buffer.GetIterAtMark(buffer.InsertMark);
					toInsert = buffer.CreateMark(null, position, true);					
					equation_ends.Add(toInsert);
					Logger.Debug("CalcAddin: Possible equation end identified at {0} ", buffer.GetIterAtMark(toInsert).Offset);
					AutomaticCalculation();
					break;
				default:
				break;
				} 
			}
   	 	}
		
		//For automatic calculating, execution stage
		void AutomaticCalculation()
		{
			//Checks through he two textmark lists to see if it finds a start 
			//and end bracketed equation, then sends it to be calculated,
			//removes the brackets from the text, prints answer and removes the textmarks from the list.
			
			//Find the matching pairs
			//Implemented sorting alghorithm because textmarks aren't defined as comparable
			//Horrible overengineering is fun!
			equation_starts = mergeSort(equation_starts);
			equation_ends = mergeSort(equation_ends);
			
			List<Gtk.TextMark> matches = new List<Gtk.TextMark>(); 	//We'll have matches on space n and n+1 in this list.
			
			foreach (Gtk.TextMark i in equation_starts ) {
				foreach (Gtk.TextMark j in equation_ends) {
					Logger.Debug("CalcAddin: Looking for possible equations between " 
					             + buffer.GetIterAtMark(i).Offset + " and " + buffer.GetIterAtMark(j).Offset);
					if (buffer.GetIterAtMark(j).Offset > buffer.GetIterAtMark(i).Offset) {
						matches.Add(i);
						matches.Add(j);
						Logger.Debug("CalcAddin: Found equation between " 
						             + buffer.GetIterAtMark(i).Offset + " and " + buffer.GetIterAtMark(j).Offset);
						break;
					}
				}
			}
			
			//Iterates through all the matches and tries to solve
			string equationraw;
			string equation;
			string answer;
			for (int i = 0; i< matches.Count -1 ; i = i + 2 ) {
				try {
					//At the moment it does not handle brackets within brackets very well.
					equationraw = buffer.GetText(buffer.GetIterAtMark(matches[i]), buffer.GetIterAtMark(matches[i+1]), false);
					equation = equationraw.Remove(equationraw.Length -1);
					answer = CalculateAnswer(equation);
					
					//Inserting into text and removing brackets
					TextIter pos_start = buffer.GetIterAtMark(matches[i]);
					TextIter pos_end = buffer.GetIterAtMark(matches[i+1]);
					pos_start.BackwardChar();
					buffer.DeleteInteractive(ref pos_start, ref pos_end, true);
					buffer.InsertInteractive (ref pos_start,  equation + answer, true);
					
					//Removing textmarks from lists.
					buffer.DeleteMark(matches[i]);
					buffer.DeleteMark(matches[i+1]);
					equation_starts.Remove(matches[i]);
					equation_starts.Remove(matches[i+1]);
					equation_ends.Remove(matches[i]);
					equation_ends.Remove(matches[i+1]);
					
				}catch (Exception ) {	//We just suppress all exceptions because they will occur from people bracekting non-equations
					Logger.Debug("CalcAddin: Caught an exception while trying to calculate automatically, supressing the exception.");
				}		
			}
		}	
		
		//This method takes a string with an equation and returns the answer as a string and 
		//and is used for both automatic and manual calculating
		private string CalculateAnswer (string equation_text) 
		{	
			// Wash newlines out of the text first
			for (int a = 0; a < equation_text.Length; a++) {
				if (equation_text[a] == '\n' || equation_text[a] == '\r') {
					equation_text = equation_text.Remove (a, 1);
				}
			}
			
			// Have to wash currency signs out of the text first, assumes only one currency is used!
			string money_sign = "";
			for (int a = 0; a < equation_text.Length; a++) {
				if (equation_text[a] == '£' || equation_text[a] == '$' || 
				    equation_text[a] == '¥' || equation_text[a] == '€' ||
				    equation_text[a] == '¢' || equation_text[a] == '¤') {
					money_sign = equation_text[a].ToString ();
					//Console.WriteLine ("CalcAddin: Removed money sign: -" + money_sign + "-");
					equation_text = equation_text.Remove (a, 1);
					equation_text =  equation_text.Insert (a, " ");		//So we don't mess up the for loop.					
				}
			}
			
			int comma = 0; //0 = no decimal, 1 = comma, 2 = dot
			//Also, we have to make commas useable! (Espes. with the MS runtime bug)
			if (equation_text.Contains(",")) {
				equation_text = equation_text.Replace(',','.');
				comma = 1;
			}else if (equation_text.Contains(".")) {
				comma = 2;
			}
//			for (int b = 0; b < equation_text.Length; b++) {
//				if (equation_text[b] == ',') {
//					//Console.WriteLine ("CalcAddin: Changed comma to punctuation mark");
//					equation_text = equation_text.Remove (b, 1);
//					equation_text =  equation_text.Insert (b, ".");	
//					comma = 1;
//				}
//				else if (equation_text[b] == '.') {
//					comma = 2;
//				}
//			}
			
			Logger.Debug ("CalcAddin: Equation to calculate: " + equation_text);
			
			Parser translator = new Parser ();					//Math.Net classes
			translator.Provider = new InfixTokenizer ();
			double ans;
	
			//We have to switch the thread over to the invariant culture to avoid the decimal bug in cdrnets parser
			CultureInfo cultureBackup = (CultureInfo) System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			
			// Assumes the user inputs a scalar equation, you don't use Tomboy for heavy computing!

			IScalarExpression equation = (IScalarExpression) translator.Parse (equation_text);	// Root of the decimal bug!
//			Logger.Debug ("CalcAddin: About to calculate");
			ans = equation.Calculate ();
//			Logger.Debug ("CalcAddin: Finished calculating");
			
			//Switching back to the systems current culture so we don't mess up Tomboy
			System.Threading.Thread.CurrentThread.CurrentCulture = cultureBackup;
			
			//Gets the decimal count. TODO: Should be moved into setup for grester efficency. (Wait for a rewrite)
			int decimal_count;
			try {
				decimal_count = (int) Preferences.Get (CalculatorAddin.CALCULATOR_DECIMAL_COUNT);
			} catch (Exception) {
				Logger.Debug("CalcAddin: Couldn't find a preference for decimal count.");
				decimal_count = 3;				//Defaults to 3 if no preference is set.
			}
			
			double round_ans = System.Math.Round (ans, decimal_count);
			string toInsert = " = " + round_ans.ToString(cultureBackup) + money_sign + " ";
			
				Logger.Debug ("CalcAddin: Comma is: " + comma);
			//(Re)insert commas where that is desired, or change commas to punct. if one uses a different mark then the current culture
			if (comma == 1) {
				toInsert = toInsert.Replace('.', ',');
			} else if (comma == 2) {
				toInsert = toInsert.Replace(',', '.');
			}
			Logger.Debug ("CalcAddin: Answer to post: " + toInsert);
			return toInsert;
		}	
		
		//Implementation of sorting method for textmarks
		private List<TextMark> mergeSort(List<TextMark> toSort)
		{
			if (toSort.Count <= 1) {
				return toSort;
			}
			List<TextMark> left = new List<TextMark>();
			List<TextMark> right = new List<TextMark>();
			
			int middle = toSort.Count/2;
			
			for (int i = 0; i<middle; i++) {
				left.Add(toSort[i]);
			}
			for (int i = middle; i<toSort.Count; i++) {
				right.Add(toSort[i]);
			}
			
			left = mergeSort(left);
			right = mergeSort(right);
			
			if (buffer.GetIterAtMark(left[left.Count -1]).Offset > buffer.GetIterAtMark(right[0]).Offset) {
				return merge(left, right);
			} else
			{
				left.AddRange(right);
				return left;
			}
		}
		
		//Implementation of sorting method for textmarks
		private List<TextMark> merge(List<TextMark> left, List<TextMark> right)
		{
			List<TextMark> result = new List<TextMark>();
			
			while (left.Count > 0 && right.Count > 0) 
			{
				if (buffer.GetIterAtMark(left[0]).Offset <= buffer.GetIterAtMark(right[0]).Offset) {
					result.Add(left[0]);
					left.RemoveAt(0);
				}else 
				{
					result.Add(right[0]);
					right.RemoveAt(0);
				}
			}
			if (left.Count > 0) {
				result.AddRange(left);
			}else
			{
				result.AddRange(right);
			}
			return result;
		}		
	}
}