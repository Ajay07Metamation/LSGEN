using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

if (args.Length == 0) { Console.WriteLine ("No input!"); Console.ReadKey (); return; }
var file = args[0];
var exists = File.Exists (file);
RobotPostProcessor rpp = new (file);
rpp.GenOutFiles ();
rpp.GenBendSub ();
Console.WriteLine ("File generated successfully.");
Console.ReadKey ();

partial class RobotPostProcessor {
   public RobotPostProcessor (string filePath) { mFilePath = filePath; mFileName = Path.GetFileNameWithoutExtension (filePath); mBendSubPoints = new (); }
   public void GenOutFiles () {
      string label = "";
      int pCount = 0;
      StringBuilder pointCollection = new ();
      List<string> labels = new ();
      bool isBendPos = false;
      Regex pattern = MyRegex ();

      using StreamReader refSr = new (Assembly.GetExecutingAssembly ().GetManifestResourceStream ("LSGen.HCData.RoboHC_WOSW.txt")!);
      using StreamWriter bendCoordSW = new ($"{mFileName}BendCoord.txt");
      using StreamWriter lsSW = new ($"{mFileName}.LS");
      using StreamReader rbcSR = new (mFilePath);

      // Write Header to LS file
      lsSW.WriteLine ($"/ PROG  {mFileName}");
      for (string? header = refSr.ReadLine (); header != null && !header.StartsWith ("[1]"); header = refSr.ReadLine ()) {
         if (header == "" || header.StartsWith ("[0]")) continue;
         if (header.StartsWith ("COMMENT") || header.StartsWith ("CREATE") || header.StartsWith ("MODIFIED")) {
            var isFileExist = File.Exists (mFilePath);
            var currentTime = DateTime.Now;
            lsSW.WriteLine (header switch {
               var h when h.StartsWith ("COMMENT") => $"COMMENT\t\t= \"{currentTime:HH:mm MM/dd}\";",
               var h when h.StartsWith ("MODIFIED") => $"MODIFIED\t= DATE {currentTime:yy-MM-dd} TIME {currentTime:HH:mm:ss};",
               _ => ""
            });
         }
         lsSW.WriteLine (header);
      }

      // Write to LS and BendSub LS files
      int lineCount = 1;
      for (int i = 0; ; i++) {
         string? crLine = rbcSR.ReadLine ();
         if (crLine == null) break;
         if (crLine == "" || i < 8) continue;
         if (crLine == "Bend 0") { isBendPos = true; pCount = 0; continue; }
         if (crLine.StartsWith ("(")) {
            pCount++;
            label = pCount == 1 ? "Init" : crLine[1..^1];
            labels.Add (label);
            string? line = refSr.ReadLine ();
            while (!string.IsNullOrEmpty (line) && !line.StartsWith ($"[{pCount + 1}]")) {
               lsSW.WriteLine ($"{lineCount}: {line}");
               line = refSr.ReadLine () ?? "";
               lineCount++;
            }
         } else if (crLine.StartsWith ("G01") && pattern.Match (crLine) is { Success: true } match) {
            string[] points = new[] { "J1", "J2", "J3", "J4", "J5", "J6" }
                                    .Select (j => double.Parse (match.Groups[j].Value).ToString ("F2")).ToArray ();
            if (isBendPos) WriteToSB (mBendSubPoints, points, ++pCount);
            else {
               var motion = crLine.Contains ("Forward") ? 'J' : 'L';
               lsSW.WriteLine ($" {lineCount++}:  {motion} P[{pCount}: J_{label}] 100% FINE;");
               WriteToSB (pointCollection, points, pCount, labels);
            }
         }
         if (!crLine.StartsWith ("G01") && isBendPos) { bendCoordSW.WriteLine (crLine); }
      }
      lsSW.WriteLine ("/POS\n" + pointCollection + "\n/END");
   }

   public void GenBendSub () {
      using (StreamWriter bendSubSW = new ($"{mFileName}BendSub.LS")) {
         using StreamReader bendSubSR = new (Assembly.GetExecutingAssembly ().GetManifestResourceStream ("LSGen.HCData.BendSubHC.txt")!);
         for (string? line; (line = bendSubSR.ReadLine ()) != null;) {
            if (line.StartsWith ("COMMENT") || line.StartsWith ("CREATE") || line.StartsWith ("MODIFIED")) {
               var isFileExist = File.Exists (mFilePath);
               var currentTime = DateTime.Now;
               bendSubSW.WriteLine (line switch {
                  var l when l.StartsWith ("CREATE") => isFileExist ? "" : $"CREATE\t= DATE {currentTime:yy-MM-dd} TIME {currentTime:HH:mm:ss};",
                  var l when l.StartsWith ("COMMENT") => $"COMMENT\t\t= \"{currentTime:HH:mm MM/dd}\";",
                  var l when l.StartsWith ("MODIFIED") => $"MODIFIED\t= DATE {currentTime:yy-MM-dd} TIME {currentTime:HH:mm:ss};",
                  _ => ""
               });
            }
            bendSubSW.WriteLine (line);
         }
         bendSubSW.Write (mBendSubPoints.ToString ());
      }
   }

   void WriteToSB (StringBuilder sb, string[] jPositions, int pCount, List<string>? labels = null) {
      sb.Append ($"P[{pCount}:{(labels != null ? $"{labels[pCount - 1]}" : "")}]{{\nGP1:\n" +
                 $"UF : {(pCount < 9 ? 1 : pCount < 20 ? 2 : 3)}, UT : 2,\n" +
                 $"J1 = {jPositions[0]} deg, J2 = {jPositions[1]} deg, J3 = {jPositions[2]} deg,\n" +
                 $"J4 = {jPositions[3]} deg, J5 = {jPositions[4]} deg, J6 = {jPositions[5]} deg\n}};\n");
   }

   string mFileName, mFilePath;
   StringBuilder mBendSubPoints;

   [GeneratedRegex (@"G01 J1\s(?<J1>[-+]?\d*\.?\d+) J2\s(?<J2>[-+]?\d*\.?\d+) J3\s(?<J3>[-+]?\d*\.?\d+) J4\s(?<J4>[-+]?\d*\.?\d+) J5\s(?<J5>[-+]?\d*\.?\d+) J6\s(?<J6>[-+]?\d*\.?\d+).*", RegexOptions.Compiled)]
   private static partial Regex MyRegex ();
}


// Code used for removing serial numbers in hard code. Attaching here for any further requiremments.

/*using System.Reflection;

using StreamReader sR = new (Assembly.GetExecutingAssembly ().GetManifestResourceStream ("LSGen.HCData.RoboHC_WSW.txt")!);
using StreamWriter sW = new ("HardCodeWOSW.txt");
for (int i = 1; ; i++) {
   var line = sR.ReadLine ();
   if (line == null) break;
   if (line.Trim () == "[1]") i = 0;
   if (line.Trim ().StartsWith ($"{i}:")) {
      var idx = line.IndexOf (':') + 1;
      var s = line.Substring (idx);
      if (s != null && s.Count () > 1) line = s;
   }
   sW.WriteLine (line);
}*/