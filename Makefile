CSOBJECTS = CalculatorAddin.cs CalculatorPreferences.cs CalculatorPreferencesFactory.cs

CSC = gmcs

all: ${CSOBJECTS}
	$(CSC) ${CSOBJECTS} -debug -out:CalculatorAddin.dll -target:library -pkg:tomboy-addins -r:Mono.Posix,MathLib.dll -resource:Calculator.addin.xml

install: ${CSOBJECTS}
	$(CSC) ${CSOBJECTS} -out:/usr/lib/tomboy/addins/CalculatorAddin.dll -target:library -pkg:tomboy-addins -r:Mono.Posix,MathLib.dll -resource:Calculator.addin.xml
