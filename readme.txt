Calculator Addin for Tomboy Notes - http://www.robpvn.net/calcaddin

1) Installing

In order to install the add-in, simply place CalculatorAddin.dll and MathLib.dll in your Tomboy plugins folder.
(CalculatorAddin.dll is the actual add-in, MathLib.dll is the Math.NET redistributable library it depends on for
mathematical operations.) Then go to Preferences -> Add-ins and enable the add-in named Calculator.

Your Tomboy plugins folder varies depending on your platform, see http://live.gnome.org/Tomboy/Directories.

2) Using

The add-in will default to handling equations in manual mode, while automatic mode can be turned on via the preferences dialog.
Manual mode will cause the add-in to behave exactly as in previous versions, automatic mode is more fancy but less stable. Improved
automatic functionality will probably be a focus of future releases.

In automatic mode:
Simply surround your equation with brackets like so: [2+2] 
The answer (and a = sign) will then be inserted into the text directly behind the selected equation, with the brackets removed.
At the moment automatic mode is not very robust, but will work fine for straight-forward use. Brackets within brackets, misplaced
brackets and so on will give unpredictable results. Using the LaTex add-in at same time will probably give very strange results 
since LaTex uses a lot of brackets!

In manual mode:
Simply write an equation in the text, select it and choose Tools -> Calculate Answer or use the shortcut: Ctrl+E. 
The answer (and a = sign) will then be inserted into the text directly behind the selected equation. 

The add-in uses the Math.NET Classic library for calculations, so it has a pretty robust set of features. It can 
handle parantheses, decimals (x.x or x,x), additions (+), subtractions (-), multiplication (*), division (/),  
roots (sqrt()) and powers (x^3), logarithms (ln(x), exp(x)), sinus/cosinus/tangent (sin/cos/tan(x)), and probably 
some other stuff too. It is 
only set up for simple scalar expressions, so no variables (letters, that is), matrices 
and other heavy stuff! 
Also, the answer will also be rounded down to three decimals. If you want to do the kind of 
stuff that requires more advanced tools, you shouldn't be doing it in a Tomboy note... If you want to use the most 
common currency signs anywhere in the equation, it can recognize this and assume you want the same sign after the answer.

3) Changelog

Version 0.40: Added preference dialog to select how many decimals the answer should be rounded to. (Minimum 1, maximum 12)
Version 0.35: Added culture handling to eliminate the bug relating to commas or punctuation marks as decimal signs.
	      Both will now be used correctly regardless of runtime and regional settings.
Version 0.30: Implemented automatic identification and solving of equations, with preferences for turning on or off.
	      Added ability to handle equations that span over several lines. (Thanks to Matt Harrison)
Version 0.20: Added ability to handle commas as decimal separators.
Version 0.15: Added ability to handle currency signs. (Thanks to Roumano)
Version 0.10: Original release.


4) Known bugs

- When using automatic mode and starting Tomboy from the command line (on Linux), you will recieve a large number of 
warnings from Glib everytime a calculation is completed about the buffer having changed since the creation of the textiter 
and so on. As far as I can see, it happens because both the add-in and Tomboy itself access the buffer when these changes 
are made, and Glib reacts badly to it because the buffer is not designed to be used by two objects simultaneously 
(this is somewhat akin to threading). 

However, this has noe discernible effect on the actual functionality of Tomboy and the add-inn, it merely creates unsightly
output to the command line.

5) Acknowledgements

Thank you to the Tomboy team for making such a great tutorial on creating add-ins and for answering my questions!
Thank you to everyone who contributes to Tomboy and Open Source in general, and thank you to my girlfriend
for bearing with me while I do my computer stuff...

