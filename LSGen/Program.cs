using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

//Console.WriteLine (args.Length);
//Console.ReadKey ();
//if (args.Length == 0) return;
RobotPostProcessor rpp = new ("C:\\Users\\nehrujiaj\\Downloads\\BEND.rbc");
rpp.CreateOutFiles ();
Console.WriteLine ("File generated successfully :)");
Console.ReadKey ();

class RobotPostProcessor {
   public RobotPostProcessor (string filePath) { mFilePath = filePath; mFileName = Path.GetFileNameWithoutExtension (filePath); }
   public void CreateOutFiles () {
      string label = "";
      int pCount = 0;
      StringBuilder pointCollection = new (), bendSubPoints = new ();
      List<string> labels = new ();
      bool isBendPos = false;
      Regex pattern = new (@"G01 J1\s(?<J1>[-+]?\d*\.?\d+) J2\s(?<J2>[-+]?\d*\.?\d+) J3\s(?<J3>[-+]?\d*\.?\d+) J4\s(?<J4>[-+]?\d*\.?\d+) J5\s(?<J5>[-+]?\d*\.?\d+) J6\s(?<J6>[-+]?\d*\.?\d+).*", RegexOptions.Compiled);

      using StreamReader refSr = new (Assembly.GetExecutingAssembly ().GetManifestResourceStream ("LSGen.HCData.RoboHC_WOSW.txt")!);
      using StreamWriter bendCoordSW = new ($"{mFileName}BendCoord.txt");
      using StreamWriter sw = new ($"{mFileName}.LS");
      using StreamReader sr = new (mFilePath);

      // Write Header to LS file
      sw.WriteLine ($"/ PROG  {mFileName}");
      for (string? header = refSr.ReadLine (); header != null && !header.StartsWith ("[1]"); header = refSr.ReadLine ()) {
         if (header == "" || header.StartsWith ("[0]")) continue;
         if (header.StartsWith ("COMMENT") || header.StartsWith ("CREATE") || header.StartsWith ("MODIFIED")) {
            var isFileExist = File.Exists (mFilePath);
            var currentTime = DateTime.Now;
            var updatedHeader = header switch {
               var h when h.StartsWith ("COMMENT") => $"COMMENT\t\t= \"{currentTime:HH:mm MM/dd}\";",
               var h when h.StartsWith ("MODIFIED") => $"MODIFIED\t= DATE {currentTime:yy-MM-dd} TIME {currentTime:HH:mm:ss};",
               _ => null
            };

            if (!string.IsNullOrEmpty (updatedHeader)) { sw.WriteLine (updatedHeader); continue; }
         }
         sw.WriteLine (header);
      }

      // Write to LS and BendSub LS files
      int c = 1;
      for (int i = 0; ; i++) {
         string? crLine = sr.ReadLine ();
         if (crLine == null) break;
         if (crLine == "" || i < 8) continue;
         if (crLine == "Bend 0") { isBendPos = true; pCount = 0; continue; }
         if (crLine.StartsWith ("(")) {
            pCount++;
            if (pCount == 1) label = "Init";
            else label = crLine[1..^1];
            labels.Add (label);
            string? line = refSr.ReadLine ();
            while (!string.IsNullOrEmpty (line) && !line!.StartsWith ($"[{pCount + 1}]")) {
               sw.WriteLine (line);
               line = refSr.ReadLine () ?? "";
               c++;
            }
         } else if (crLine.StartsWith ("G01") && pattern.Match (crLine) is { Success: true } match) {
            string[] points = new[] { "J1", "J2", "J3", "J4", "J5", "J6" }
                                    .Select (j => double.Parse (match.Groups[j].Value).ToString ("F2"))
                                    .ToArray ();
            if (isBendPos) bendSubPoints.Append ($"P[{++pCount}]{{\nGP1:\nUF : {(pCount < 9 ? 1 : pCount < 20 ? 2 : 3)}, UT : 2,\n" +
                          $"J1 = {points[0]} deg, J2 = {points[1]} deg, J3 = {points[2]} deg,\n" +
                          $"J4 = {points[3]} deg, J5 = {points[4]} deg, J6 = {points[5]} deg\n}};\n");
            else {
               var motion = crLine.Contains ("Forward") ? 'J' : 'L';
               sw.WriteLine ($" {c++}: {motion} P[{pCount}: J_{label}] 100% FINE;");
               pointCollection.Append ($"P[{pCount}:\"{labels[pCount - 1]}\"]{{\nGP1:\nUF : {(pCount < 9 ? 1 : pCount < 20 ? 2 : 3)}, UT : 2,\n" +
                         $"J1 = {points[0]} deg, J2 = {points[1]} deg, J3 = {points[2]} deg,\n" +
                         $"J4 = {points[3]} deg, J5 = {points[4]} deg, J6 = {points[5]} deg\n}};\n");
            }
         }
         if (!crLine.StartsWith ("G01") && isBendPos) { bendCoordSW.WriteLine (crLine); }
      }
      sw.WriteLine ("/POS\n" + pointCollection + "\n/END");

      using (StreamWriter bendSubSW = new ($"{mFileName}BendSub.LS")) {
         using StreamReader bendSubSR = new (Assembly.GetExecutingAssembly ().GetManifestResourceStream ("LSGen.HCData.BendSubHC.txt")!);
         for (string? line; (line = bendSubSR.ReadLine ()) != null;) {
            if (line.StartsWith ("COMMENT") || line.StartsWith ("CREATE") || line.StartsWith ("MODIFIED")) {
               var isFileExist = File.Exists (mFilePath);
               var currentTime = DateTime.Now;
               var updatedLine = line switch {
                  var l when l.StartsWith ("COMMENT") => $"COMMENT\t\t= \"{currentTime:HH:mm MM/dd}\";",
                  //var h when h.StartsWith ("CREATE") && !isFileExist => $"CREATE\t\t= DATE {currentTime:yy-MM-dd} TIME {currentTime:HH:mm:ss};",
                  var l when l.StartsWith ("MODIFIED") => $"MODIFIED\t= DATE {currentTime:yy-MM-dd} TIME {currentTime:HH:mm:ss};",
                  _ => null
               };

               if (!string.IsNullOrEmpty (updatedLine)) { bendSubSW.WriteLine (updatedLine); continue; }
            }
            bendSubSW.WriteLine (line);
         }
         bendSubSW.Write (bendSubPoints);
      }
   }

   string mFileName, mFilePath;
}