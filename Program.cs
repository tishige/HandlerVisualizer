
using System.Text;
using CommandLine;
using System.Text.RegularExpressions;
using ShellProgressBar;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using HandlerVisualizer;
using System.Xml.Linq;

namespace HandlerVisualizer
{
    internal class Program
    {

        static void Main(string[] args)
        {

            bool convertToVisio = false;
            bool convertToPng = false;
            bool showStepParameters = false;
            try
            {
                var configRoot = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(path: "appsettings.json").Build();
                convertToVisio = configRoot.GetSection("drawioSettings").Get<DrawioSettings>().ConvertToVisio;
                convertToPng = configRoot.GetSection("drawioSettings").Get<DrawioSettings>().ConvertToPng;
                showStepParameters = configRoot.GetSection("HndvSettings").Get<HndvSettings>().ShowStepParameters;

            }
            catch (Exception)
            {
                ColorConsole.WriteError($"The configuration file 'appsettings.json' was not found in this directory.");
                PrintUsage();

            }


            var parseResult = Parser.Default.ParseArguments<Options>(args);
            Options opt = new();

            FileInfo[] fileInfo = null;
            List<string> xmlFiles = new List<string>();
            switch (parseResult.Tag)
            {
                case ParserResultType.Parsed:
                    var parsed = parseResult as Parsed<Options>;
                    opt = parsed.Value;

                    if (convertToVisio) opt.visio = true;
                    if (convertToPng) opt.png = true;

                    if (opt.Filename == null && opt.handlers==false)
                    {
                        ColorConsole.WriteError("Incorrect command line argument.");
                        PrintUsage();

                    }

                    if (opt.drawio || opt.visio || opt.png)
                    {
                        Process[] processes = Process.GetProcessesByName("draw.io");
                        if (processes.Length > 0)
                        {
                            ColorConsole.WriteError($"draw.io is running. Close draw.io first.");
                            Environment.Exit(1);
                        }

                    }

                    if (opt.Filename != null)
                    {
                        if (!File.Exists(opt.Filename))
                        {
                            ColorConsole.WriteError($"{opt.Filename} does not exist.  Check if the file exists.");
                            PrintUsage();
                        }

                        if (Path.GetExtension(opt.Filename) == ".xml" || Path.GetExtension(opt.Filename) == ".XML")
                        {
                            xmlFiles.Add(opt.Filename);
                            break;
                        }

                    }

                    if (opt.handlers)
                    {
                        try
                        {
                            DirectoryInfo di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "Handlers"));
                            fileInfo = di.GetFiles("*.xml", SearchOption.AllDirectories);

                            if (fileInfo.Length == 0)
                            {
                                ColorConsole.WriteError($"No xml files were found in Handler folder. Run idu.exe on your IC server first to export XML files of handlers.");
                                PrintUsage();
                            }
                            else
                            {
                                xmlFiles = fileInfo.Select(x => x.FullName).ToList();

                            }

                        }
                        catch (Exception)
                        {
                            ColorConsole.WriteError($"No Handler folder.Create Handler folder and Copy handler XML files from your IC server.");
                            PrintUsage();
                        }

                        break;

                    }


                    PrintUsage();
                    break;

                case ParserResultType.NotParsed:

                    PrintUsage();
                    break;

            }

            List<string> csvFileResultList = new List<string>();

            foreach (var xmlfiles_i in xmlFiles)
            {
                ColorConsole.WriteLine($"Analyzing Handler XML file {xmlfiles_i}", ConsoleColor.Yellow);
                List<HandlerFlowElements> flowElementsList = CollectStepValuesFromXML.CollectSteps(xmlfiles_i,showStepParameters);
                
                string csvFilename = CreateCSV.CreateCSVHandlerSteps(flowElementsList);

                csvFileResultList.Add(csvFilename); 

            }

            ColorConsole.WriteLine($"Create CSV files done.", ConsoleColor.Yellow);


            if (opt.drawio || opt.visio || opt.png)
            {
                DrawFlow.DrawFlowFromCSV(csvFileResultList, opt.visio, opt.png);

            }


            Console.WriteLine();
            ColorConsole.WriteLine("Completed!", ConsoleColor.Yellow);
            Environment.Exit(0);
        }


        private static void PrintUsage()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();

            sb.AppendLine("Usage:");
            sb.AppendLine("  HandlerVisualizer.exe Exported Handler XML File");

            sb.AppendLine();
            sb.AppendLine("Options:");
            sb.AppendLine(@"  -d --drawio    Call .\drawio\draw.io.exe for CallFlowVisualizer after creating CSV files.");
            sb.AppendLine(@"  -v --visio     Convert to visio file after creating drawio files");
            sb.AppendLine(@"  -n --png       Convert to png file after creating drawio files");
            sb.AppendLine(@"  -h --handlers  Load all xml files in Handlers folder.");

            sb.AppendLine(@"  --help         Show this screen.");
            sb.AppendLine(@"  --version      Show version.");

            sb.AppendLine();
            sb.AppendLine("Examples:");
            sb.AppendLine("  HandlerVisualizer.exe SystemIVR.xml");
            sb.AppendLine();

            Console.Out.Write(sb.ToString());
            Environment.Exit(1);
        }



    }


}




