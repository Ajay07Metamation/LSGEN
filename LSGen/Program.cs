using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

//if (args.Length == 0) return;
//Console.WriteLine (args[0]);
//Console.ReadKey ();

string label = "";
int pCount = 0;
StringBuilder pointCollection = new ();
StringBuilder bendSubPoints = new ();
List<string> labels = new ();
bool isBendPos = false;
Regex pattern = new (@"G01 J1\s(?<J1>[-+]?\d*\.?\d+) J2\s(?<J2>[-+]?\d*\.?\d+) J3\s(?<J3>[-+]?\d*\.?\d+) J4\s(?<J4>[-+]?\d*\.?\d+) J5\s(?<J5>[-+]?\d*\.?\d+) J6\s(?<J6>[-+]?\d*\.?\d+).*", RegexOptions.Compiled);
var wswSr = new StreamReader (Assembly.GetExecutingAssembly ().GetManifestResourceStream ($"LSGen.HCData.RoboHC_WSW.txt")!);
var woswSr = new StreamReader (Assembly.GetExecutingAssembly ().GetManifestResourceStream ($"LSGen.HCData.RoboHC_WOSW.txt")!);
StreamWriter bendCoordSW = new ("part230BendCoord.txt");

using (StreamWriter sw = new ("part230.LS")) {
   using (StreamReader sr = new ("C:\\Users\\nehrujiaj\\Downloads\\BEND_WOSW.rbc")) {
      // Writing header part of the LS file.
      string curLine = woswSr.ReadLine () ?? "";
      while (curLine != "" && curLine.StartsWith ('[')) {
         for (; ; ) {
            var header = woswSr.ReadLine () ?? "";
            if (header.StartsWith ('[')) break;
            sw.WriteLine (header);
         }
         break;
      }

      int c = 0;
      for (int i = 0; ; i++) {
         string crLine = sr.ReadLine ();
         if (crLine == null) break;
         if (crLine == "") continue;
         switch (i) {
            case < 8: break;
            default:
               if (crLine == "Bend 0") { isBendPos = true; pCount = 0; break; }
               if (crLine.StartsWith ('(')) {
                  label = crLine.Remove (0, 1).Remove (crLine.Length - 2);
                  labels.Add (label);
                  pCount++;
                  for (; ; ) {
                     // Writes the default code of LS file.
                     var nxtLine = woswSr.ReadLine () ?? "";
                     if (nxtLine.StartsWith ('[')) break;
                     if (nxtLine == "") continue;
                     sw.WriteLine (nxtLine);
                     c++;
                  }
                  sw.WriteLine ($"  {++c}: J P[{pCount}: J_{label}] 100% FINE");
               }
               if (crLine.StartsWith ("G01")) {
                  Match match = pattern.Match (crLine);
                  if (match.Success) {
                     // Collects points if pattern matches.
                     double[] points = {    double.Parse(match.Groups["J1"].Value),
                                            double.Parse(match.Groups["J2"].Value),
                                            double.Parse(match.Groups["J3"].Value),
                                            double.Parse(match.Groups["J4"].Value),
                                            double.Parse(match.Groups["J5"].Value),
                                            double.Parse(match.Groups["J6"].Value)
                        };
                     if (!isBendPos) {
                        // Collects the points for LS file
                        pointCollection.Append ($"P[{pCount}:{label}]{{\nGP1:\nUF : {(pCount < 9 ? 1 : pCount < 20 ? 2 : 3)}, UT : 2\n" +
                                                $"J1 = {points[0]} deg, J2 = {points[1]} deg, J3 = {points[2]} deg,\n" +
                                                $"J4 = {points[3]} deg, J5 = {points[4]} deg, J6 = {points[5]} deg\n}};\n");
                     } else {
                        // Collects the points for BendSub LS file
                        bendSubPoints.Append ($"P[{++pCount}]{{\nGP1:\nUF : {(pCount < 9 ? 1 : pCount < 20 ? 2 : 3)}, UT : 2\n" +
                                             $"J1 = {points[0]} deg, J2 = {points[1]} deg, J3 = {points[2]} deg,\n" +
                                             $"J4 = {points[3]} deg, J5 = {points[4]} deg, J6 = {points[5]} deg\n}};\n");
                     }

                  }
               } else if (isBendPos) bendCoordSW.WriteLine (crLine);
               break;
         }
      }
      sw.WriteLine (pointCollection);
   }
}
bendCoordSW.Close ();

// BendSub.LS
StreamReader bendSubSR = new (Assembly.GetExecutingAssembly ().GetManifestResourceStream ($"LSGen.HCData.BendSubHC.txt")!);
StreamWriter bendSubSW = new ("BendSub.LS");
for (; ; ) {
   var line = bendSubSR.ReadLine ();
   if (line == null) break;
   bendSubSW.WriteLine (line);
}
bendSubSR.Close ();
bendSubSW.WriteLine (bendSubPoints);
bendSubSW.Close ();