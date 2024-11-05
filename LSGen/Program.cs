// See https://aka.ms/new-console-template for more information
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

//if (args.Length == 0) return;
//Console.WriteLine (args[0]);
//Console.ReadKey ();

bool isRequired = false;
string label;
int pCount = 0;
List<List<double>> iniPositions = new ();
List<double> pos = new ();
bool pointRead = false;
string point = "";
List<string> labels = new ();
bool isBendPos = false;
List<List<double>> bendPosistions = new ();
List<double> bendPos = new ();

StreamWriter bendCoordSW = new ("part230BendCoord.txt");

using (StreamWriter sw = new StreamWriter ("part230.LS")) {


   using (StreamReader sr = new StreamReader ("C:\\Users\\rajakumarra\\Downloads\\part230.rbc")) {
      for (int i = 1; ; i++) {
         string line = sr.ReadLine ();
         if (line == null) break;
         if (line == "") continue;
         switch (i) {
            case < 9: break;
            default:
               if (line == "Bend 0") { isBendPos = true; break; }
               pos = new (); bendPos = new ();
               if (isBendPos) {
                  if (line[0] == 'G') {
                     for (int k = 0; k < line.Length; k++) {
                        if (line[k] == 'J' && Char.IsDigit (line[k + 1])) { pointRead = true; k += 3; }
                        if (line[k] == ' ') { pointRead = false; if (Double.TryParse (point, out double parsedPt)) bendPos.Add (parsedPt); point = ""; }
                        if (pointRead) point += line[k];
                     }
                     if (bendPos.Count != 0) bendPosistions.Add (bendPos);
                  } else
                     bendCoordSW.WriteLine (line);
                  continue;
               }


               if (line[0] == '(') {
                  isRequired = true; // True to collect points from next line
                  pCount++;
                  label = line.Remove (0, 1).Remove (line.Length - 2);
                  labels.Add (label);
                  sw.WriteLine ($"J P[{pCount}: J_{label}] 100% FINE");
               } else {
                  if (isRequired) {
                     for (int k = 0; k < line.Length; k++) {
                        var ch = line[k];
                        if (line[k] == 'J' && Char.IsDigit (line[k + 1])) { pointRead = true; k += 3; }
                        if (line[k] == ' ') { pointRead = false; if (Double.TryParse (point, out double parsedPt)) pos.Add (parsedPt); point = ""; }
                        if (pointRead) point += line[k];
                     }
                  }
                  if (pos.Count != 0) { isRequired = false; iniPositions.Add (pos); }
               }
               break;
         }



      }
   }

   sw.WriteLine ("\n");
   for (int c = 0; c < labels.Count; c++) { // labels iteration
      var printPos = iniPositions[c];
      sw.WriteLine ($"P[{c + 1}:\"J_{labels[c]}\"]"); // Need to insert a curly bracket
      sw.WriteLine (" GP1:");
      sw.Write (c + 1 <= 8 ? "  UF = 1," : c + 1 >= 17 ? "  UF = 3," : "  UF = 2,");
      sw.WriteLine (" UT = 2"); // Input for UF and UT
      for (int p = 1; p <= 6; p++)  // Iteration for printing 6 joints
         sw.Write (p != 3 ? $"  J{p} = {printPos[p - 1]}  deg," : $"  J{p} = {printPos[p - 1]}  deg\n");
      sw.WriteLine ();
      sw.WriteLine ("};");
   }
}

using (StreamWriter bendPosSW = new ("part230BendPos.txt")) {
   for (int c = 0; c < bendPosistions.Count; c++) {
      var printBendPos = bendPosistions[c];
      for (int p = 1; p <= 6; p++)
         bendPosSW.Write ($"{printBendPos[p - 1]} ");
      bendPosSW.WriteLine ();
   }
}

bendCoordSW.Close ();