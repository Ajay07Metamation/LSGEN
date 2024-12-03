using FluentFTP;
using static System.Console;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;

if (args.Length == 0) { WriteLine ("No input!"); ReadKey (); return; }
RobotPostProcessor rpp = new (args[0]);
rpp.GenOutFiles ();
rpp.GenBendSub ();
WriteLine ("File generated successfully.");
rpp.TransferFiles ();

ReadKey ();

#region class RobotPostProcessor ----------------------------------------------------------------
partial class RobotPostProcessor {
   public RobotPostProcessor (string filePath) {
      mFilePath = filePath;
      mFileName = Path.GetFileNameWithoutExtension (filePath);
      mBendSubPoints = new ();
   }

   #region Methods ------------------------------------------------
   public void GenOutFiles () {
      string label = "";
      int pCount = 0;
      StringBuilder pointCollection = new ();
      List<string> labels = [];
      bool isBendPos = false;
      Regex pattern = MyRegex ();

      using StreamReader hrSR = new (Assembly.GetExecutingAssembly ().GetManifestResourceStream ("LSGen.HCData.HeaderHC.txt")!);
      using StreamReader refSR = new (Assembly.GetExecutingAssembly ().GetManifestResourceStream ("LSGen.HCData.RoboHC_WOSW.txt")!); // HardCode Reader
      using StreamWriter bendCoordSW = new ($"{mFileName}RamPoints.txt"); // Collection of ram offsets. To be sent to RA.
      using StreamReader rbcSR = new (mFilePath);
      var isFileExist = File.Exists ($"{mFileName}.LS");
      using StreamWriter lsSW = new ($"{mFileName}.LS"); // Main LS output

      // Write Header to LS file
      lsSW.WriteLine ($"/ PROG  {mFileName}");
      for (string? header = hrSR.ReadLine (); header != null; header = hrSR.ReadLine ()) {
         if (header == "") continue;
         if (header.StartsWith ("COMMENT") /*|| header.StartsWith ("CREATE")*/ || header.StartsWith ("MODIFIED")) {
            var currentTime = DateTime.Now;
            lsSW.WriteLine (header switch { // Still some clarification is needed in setting the time in the output.
               //var h when h.StartsWith ("CREATE") => $"CREATE\t\t= DATE {currentTime:yy-MM-dd} TIME {currentTime:HH:mm:ss};",
               var h when h.StartsWith ("COMMENT") => $"COMMENT\t\t= \"{currentTime:HH:mm MM/dd}\";",
               var h when h.StartsWith ("MODIFIED") => $"MODIFIED\t= DATE {currentTime:yy-MM-dd} TIME {currentTime:HH:mm:ss};",
               _ => ""
            });
            continue;
         }
         lsSW.WriteLine (header);
      }

      // Rbc to LS
      int lineCount = 1;
      for (int i = 0; ; i++) {
         string? crLine = rbcSR.ReadLine ();
         if (crLine == null) break;
         if (crLine == "" || i < 8) continue;
         if (crLine == "Bend 0") { isBendPos = true; pCount = 0; continue; }
         if (crLine.StartsWith ('(')) {
            if (rbcSR.Peek () != '(') {
               pCount++;
               label = pCount == 1 ? "Init" : crLine[1..^1];
               labels.Add (label);
               string? line = refSR.ReadLine ();
               if (!string.IsNullOrEmpty (line) && line!.StartsWith ("[1]")) line = refSR.ReadLine ();
               while (!string.IsNullOrEmpty (line) && !line.StartsWith ($"[{pCount + 1}]")) {
                  lsSW.WriteLine ($"{lineCount}: {line}");
                  line = refSR.ReadLine () ?? "";
                  lineCount++;
               }
            }
         } else if (crLine.StartsWith ("G01") && pattern.Match (crLine) is { Success: true } match) {
            string[] points = sJointsTags.Select (j => double.Parse (match.Groups[j].Value).ToString ("F2")).ToArray ();
            if (isBendPos) WriteToSB (mBendSubPoints, points, ++pCount);
            else {
               var motion = crLine.Contains ("Forward") ? 'J' : 'L';
               lsSW.WriteLine ($" {lineCount++}:  {motion} P[{pCount}: J_{label}] {(motion == 'J' ? "R[15] %" : "R[16] mm/sec")} FINE;");
               WriteToSB (pointCollection, points, pCount, labels);
            }
         }
         if (!crLine.StartsWith ("G01") && isBendPos) { bendCoordSW.WriteLine (crLine); }
      }
      lsSW.WriteLine ("/POS\n" + pointCollection + "\n/END");
   }

   // BendSub.LS
   public void GenBendSub () {
      var isFileExist = File.Exists ($"{mFileName}BendSub.LS");
      using (StreamWriter bendSubSW = new ($"{mFileName}BendSub.LS")) {
         using StreamReader bendSubHCSR = new (Assembly.GetExecutingAssembly ().GetManifestResourceStream ("LSGen.HCData.BendSubHC.txt")!);
         for (string? line; (line = bendSubHCSR.ReadLine ()) != null;) { // Header
            if (line.StartsWith ("COMMENT") || /*line.StartsWith ("CREATE") ||*/ line.StartsWith ("MODIFIED")) {
               var currentTime = DateTime.Now;
               bendSubSW.WriteLine (line switch { // Still some clarification is needed in setting the time in the output.
                  //var l when l.StartsWith ("CREATE") => $"CREATE\t= DATE {currentTime:yy-MM-dd} TIME {currentTime:HH:mm:ss};",
                  var l when l.StartsWith ("COMMENT") => $"COMMENT\t\t= \"{currentTime:HH:mm MM/dd}\";",
                  var l when l.StartsWith ("MODIFIED") => $"MODIFIED\t= DATE {currentTime:yy-MM-dd} TIME {currentTime:HH:mm:ss};",
                  _ => ""
               });
               continue;
            }
            bendSubSW.WriteLine (line);
         }
         bendSubSW.Write (mBendSubPoints.ToString ());
      }
   }

   // Writes the positions to the required string builder. 
   void WriteToSB (StringBuilder sb, string[] jPositions, int pCount, List<string>? labels = null) {
      sb.Append ($"P[{pCount}:{(labels != null ? $"\"{labels[pCount - 1]}\"" : "")}]{{\nGP1:\n" +
                 $"UF : {(pCount < 9 ? 1 : pCount < 20 ? 2 : 3)}, UT : 2,\n" +
                 $"J1 = {jPositions[0]} deg, J2 = {jPositions[1]} deg, J3 = {jPositions[2]} deg,\n" +
                 $"J4 = {jPositions[3]} deg, J5 = {jPositions[4]} deg, J6 = {jPositions[5]} deg\n}};\n");
   }
   #endregion

   public void TransferFiles () {
      WriteLine ("Attempting to transfer files");
      string[] files = { $"{mFileName}.LS", $"{mFileName}BendSub.LS" };
      using (var ftpClient = new FtpClient ("192.168.1.175")) {
         try {
            WriteLine ("Attempting to connect to FTP server...");
            //using special commands
            WriteLine (ftpClient.Execute ("ftp -i 192.168.1.175").ErrorMessage);
            WriteLine (ftpClient.Execute ("username ").ErrorMessage);
            WriteLine (ftpClient.Execute ("password ").ErrorMessage);

            // using inbuilt ftpclient commands
            //ftpClient.Connect ();
            if (ftpClient.IsConnected) {
               //WriteLine ("Connected successfully.");
               foreach (var file in files) {
                  // using special commands
                  WriteLine (ftpClient.Execute ("cd md:").ErrorMessage);
                  WriteLine (ftpClient.Execute ($"mput {file}").ErrorMessage);

                  // using inbuilt ftpclient commands
                  //FtpStatus status = ftpClient.UploadFile (file, $"/{file}");
                  //WriteLine ($"Attempting to upload {file} file");
                  //string msg = status switch {
                  //   FtpStatus.Success => $"{file} uploaded successfully.",
                  //   FtpStatus.Skipped => $"{file} was skipped (file might already exist).",
                  //   FtpStatus.Failed => $"{file} upload failed.",
                  //   _ => ""
                  //};
                  //WriteLine (msg);
                  Thread.Sleep (500);
               }
            }
         } catch (Exception ex) { WriteLine ($"An error occurred: {ex.Message}"); } finally {
            if (ftpClient.IsConnected) {
               WriteLine ("Successfully transferred");
               //using special commands
               ftpClient.Execute ("Bye");

               // using inbuilt ftpclient commands
               //ftpClient.Disconnect ();
               //WriteLine ("Disconnected from FTP server.");
            }
         }
      }
   }

   public void TransferFiles1 () {
      WriteLine ("Attempting to transfer files");
      string[] files = { $"{mFileName}.LS", $"{mFileName}BendSub.LS" };
      string ftpAddress = "192.168.1.175";
      string username = ""; // Empty username
      string password = ""; // Empty password

      using (var ftpClient = new FtpClient (ftpAddress)) {
         try {
            WriteLine ("Attempting to connect to FTP server...");
            // Set credentials
            ftpClient.Credentials = new NetworkCredential (username, password);
            // Connect to the server
            ftpClient.Connect ();
            if (ftpClient.IsConnected) {
               WriteLine ("Connected successfully.");
               // Change directory to "mdb:"
               ftpClient.SetWorkingDirectory ("mdb:");
               foreach (var file in files) {
                  WriteLine ($"Uploading {file}...");
                  ftpClient.UploadFile (file, $"mdb:/{Path.GetFileName (file)}");
                  WriteLine ($"{file} uploaded successfully.");
                  Thread.Sleep (500);
               }
            }
         } catch (Exception ex) {
            WriteLine ($"An error occurred: {ex.Message}");
         } finally {
            if (ftpClient.IsConnected) {
               ftpClient.Disconnect ();
               WriteLine ("Disconnected from FTP server.");
            }
         }
      }
   }


   #region Attributes ---------------------------------------------
   [GeneratedRegex (@"G01 J1\s(?<J1>[-+]?\d*\.?\d+) J2\s(?<J2>[-+]?\d*\.?\d+) J3\s(?<J3>[-+]?\d*\.?\d+) J4\s(?<J4>[-+]?\d*\.?\d+) J5\s(?<J5>[-+]?\d*\.?\d+) J6\s(?<J6>[-+]?\d*\.?\d+).*", RegexOptions.Compiled)]
   private static partial Regex MyRegex ();
   #endregion

   string mFileName, mFilePath;
   StringBuilder mBendSubPoints;
   static readonly string[] sJointsTags = ["J1", "J2", "J3", "J4", "J5", "J6"];
}
#endregion


// Code used for removing serial numbers in hard code. Attaching here for any further requiremments.

/*using System.Reflection;

using StreamReader sr = new (Assembly.GetExecutingAssembly ().GetManifestResourceStream ("LSGen.HCData.RoboHC_WOSW.txt")!);
using StreamWriter sw = new ("HardCodeWOSW.txt");
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