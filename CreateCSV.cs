
using System.Text;
using System.Collections.Generic;
using CsvHelper;
using System.Globalization;
using ShellProgressBar;
using Microsoft.Extensions.Configuration;

namespace HandlerVisualizer
{
    internal class CreateCSV
    {
        internal static string CreateCSVHandlerSteps(List<HandlerFlowElements> flowElementsList)
        {

            var configRoot = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(path: "appsettings.json").Build();
            var hndvSettings = configRoot.GetSection("HndvSettings").Get<HndvSettings>();
            bool appendDatetime = hndvSettings.AppendDateTimeToFileName;

            var drawIOSettings = configRoot.GetSection("drawioSettings").Get<DrawioSettings>();
            bool colorNode = drawIOSettings.ColorNode;
            bool nodeRound = drawIOSettings.NodeRound;
            bool lineRound = drawIOSettings.LineRound;
            int nodespacing = drawIOSettings.Nodespacing;
            int levelspacing = drawIOSettings.Levelspacing;

            string nodeStyle = getNodeStyle(colorNode, nodeRound);
            string lineStyle = getLineStyle(lineRound);

            string layout = drawIOSettings.Layout;

            if(layout!= "horizontalflow" || layout!= "verticalFlow") { layout = "horizontalflow"; };

            List<string> customizationPointFirstStringList = hndvSettings.CustomizationPointFirstString;

            int maxExitPathCountOfSingleNode = flowElementsList.Select(x => x.maxCountOfRefPath).FirstOrDefault();

            List<string> csvFileResultList = new();

            var pboptions = new ProgressBarOptions
            {
                ProgressCharacter = '─',
                ProgressBarOnBottom = true
            };

            string handlerName = flowElementsList.Where(x=>!String.IsNullOrEmpty(x.HandlerName)).FirstOrDefault()?.HandlerName;

            string currentPath = Directory.GetCurrentDirectory();
            createCSVFolder(currentPath);
            string csvfilename;

            if (appendDatetime)
            {
                csvfilename = Path.Combine(currentPath, "csv", handlerName + "_" + DateTime.Now.ToString(@"yyyyMMdd-HHmmss") + ".csv");

            }
            else
            {
                csvfilename = Path.Combine(currentPath, "csv", handlerName + ".csv");

            }

            if (File.Exists(csvfilename))
            {
                csvfilename = Path.Combine(currentPath, "csv", handlerName + "_" + DateTime.Now.ToString(@"yyyyMMdd-HHmmss_fff") + ".csv");

            }

            using (var streamWriter = new StreamWriter(csvfilename, false, Encoding.Default))

            using (var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csv.WriteField("## Handler Flow");
                csv.NextRecord();
                csv.WriteComment(" label: %idLabel% %creatorName%<br>%desc%");
                csv.NextRecord();
                csv.WriteComment(nodeStyle);
                csv.NextRecord();
                csv.WriteComment(" namespace: csvimport-");
                csv.NextRecord();

                for (int i = 0; i < maxExitPathCountOfSingleNode; i++)
                {
                    csv.WriteComment(" connect: {\"from\":\"id\" , \"to\":\"refs"+i.ToString()+"\", \"invert\":true,\"tolabel\":\"toNodeExitlabel" + i.ToString() +"\" , \"style\":" + "\"" + lineStyle + "\"" + "}");
                    csv.NextRecord();
                }

                csv.WriteComment(" width: auto");
                csv.NextRecord();
                csv.WriteComment(" height: auto");
                csv.NextRecord();
                csv.WriteComment(" padding: 15");
                csv.NextRecord();
                csv.WriteComment(" ignore: id,shape,fill,stroke,refs");
                csv.NextRecord();
                csv.WriteComment(" nodespacing: " + nodespacing);
                csv.NextRecord();
                csv.WriteComment(" levelspacing: " + levelspacing);
                csv.NextRecord();
                csv.WriteComment(" edgespacing: 40");
                csv.NextRecord();
                csv.WriteComment(" layout: "+layout);
                csv.NextRecord();

                csv.WriteField("id");
                csv.WriteField("idLabel");
                csv.WriteField("creatorName");
                csv.WriteField("desc");

                csv.WriteField("fill");
                csv.WriteField("stroke");
                csv.WriteField("shape");

                for (int i = 0; i < maxExitPathCountOfSingleNode; i++)
                {
                    csv.WriteField("refs"+i.ToString());
                }

                for (int i = 0; i < maxExitPathCountOfSingleNode; i++)
                {
                    csv.WriteField("toNodeExitlabel" + i.ToString());
                }

                csv.NextRecord();

                foreach (var flowElementsList_i in flowElementsList)
                {

                    //CSSurveyPlayComment.csv
                    var debugId = flowElementsList_i.Id;

                    // id
                    csv.WriteField(flowElementsList_i.Id);
                    if (flowElementsList_i.Id == "-1")
                    {
                        csv.WriteField("");
                    }
                    else
                    {
                        csv.WriteField("("+flowElementsList_i.Id+")");
                    }

                    csv.WriteField(flowElementsList_i.CreatorName);
                    csv.WriteField(flowElementsList_i.Label);

                    string label = flowElementsList_i.Label;
                    bool IsStepCustomized = false;



                    if (customizationPointFirstStringList != null)
                    {
                        foreach (var customizationPointFirstStringList_i in customizationPointFirstStringList)
                        {
                            if (label.StartsWith(customizationPointFirstStringList_i) && !String.IsNullOrEmpty(customizationPointFirstStringList_i) && !String.IsNullOrEmpty(label))
                            {
                                IsStepCustomized = true;
                                break;
                            }
                        }

                    }

                    // shape
                    string[] shapeStyle = getShapeStyle(flowElementsList_i.CreatorName, flowElementsList_i.Type,flowElementsList_i.ExitPathCount,IsStepCustomized);
                    csv.WriteField(shapeStyle[0]); // fill
                    csv.WriteField(shapeStyle[1]); //stroke
                    csv.WriteField(shapeStyle[2]); //shape

                    for (int i = 0; i < maxExitPathCountOfSingleNode; i++)
                    {

                        if(flowElementsList_i.ExitPathsList.Any(x=>x.LabelIndex== i))
                        {
                            string targetRef = flowElementsList_i.ExitPathsList.Where(x => x.LabelIndex == i).Select(x => x.TargetRef).FirstOrDefault()?.ToString();
                            csv.WriteField(targetRef);
                        }
                        else
                        {
                            csv.WriteField("");
                        }
                        
                    }

                    for (int i = 0; i < maxExitPathCountOfSingleNode; i++)
                    {

                        if (flowElementsList_i.ExitPathsList.Any(x => x.LabelIndex == i))
                        {
                            string exitName = flowElementsList_i.ExitPathsList.Where(x => x.LabelIndex == i).Select(x => x.ExitName).FirstOrDefault()?.ToString();
                            string exitTo = flowElementsList_i.ExitPathsList.Where(x => x.LabelIndex == i).Select(x => x.TargetRef).FirstOrDefault()?.ToString();
                            string toLabel = exitName + "->" + exitTo;
                            csv.WriteField(toLabel);

                        }
                        else
                        {
                            csv.WriteField("");
                        }

                    }



                    csv.NextRecord();

                }

            }

            return csvfilename;
        }



        // Create CSV folder if it does not exists
        private static void createCSVFolder(string currentPath)
        {
            try
            {
                if (!Directory.Exists(Path.Combine(currentPath, "CSV")))
                    Directory.CreateDirectory(Path.Combine(currentPath, "CSV"));

            }
            catch (Exception)
            {
                ColorConsole.WriteError("Failed to create CSV folder.Check file access permission.");
                Environment.Exit(1);
            }

        }

        // Whether node shape is colored and rounded
        private static string getNodeStyle(bool colorNode, bool nodeRound)
        {
            string style = " style: html=1;shape=%shape%;";
            if (colorNode)
            {
                style = style + "fillColor=%fill%;strokeColor=%stroke%;";

            }
            else
            {
                style = style + "fillColor=#ffffff;strokeColor=#000000;";

            }

            if (nodeRound)
            {
                style = style + "rounded=1;";

            }

            return style;
        
        }

        // Whether line is rounded
        private static string getLineStyle(bool lineRound)
        {
            string lineStyle=null;

            if (lineRound)
            {
                lineStyle = lineStyle + "edgeStyle=orthogonalEdgeStyle;orthogonalLoop=1;jettySize=auto;curved = 0; endArrow = blockThin; endFill = 1;";

            }
            else
            {
                lineStyle = lineStyle + "edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;curved = 0; endArrow = blockThin; endFill = 1;";

            }

            return lineStyle;

        }

        // Set shape and color according to Type value
        private static string[] getShapeStyle(string creatorName,string nodeType,int exitPathCount,bool customization)
        {
            string[] shapeArray = new string[3];

            if (creatorName == "Condition" || exitPathCount>1)
            {
                if(customization)
                {
                    shapeArray[0] = "#ff0000"; //red
                    shapeArray[1] = "#ffffff";
                }
                else
                {
                    shapeArray[0] = "#e1d5e7"; //purple
                    shapeArray[1] = "#9673a6";

                }
                shapeArray[2] = "rhombus";

                return shapeArray;

            }

            if (nodeType == "Subroutine")
            {
                if (customization)
                {
                    shapeArray[0] = "#ff0000"; //red
                    shapeArray[1] = "#ffffff";
                }
                else
                {
                    shapeArray[0] = "#d5e8d4"; //green
                    shapeArray[1] = "#82b366";

                }
                shapeArray[2] = "rectangle";


                return shapeArray;
            }

            if (nodeType == "Initiator")
            {
                shapeArray[0] = "#f8cecc"; //red
                shapeArray[1] = "#b85450";
                shapeArray[2] = "ellipse";

                return shapeArray;
            }


            if (nodeType == "description")
            {
                shapeArray[0] = "#f8cecc"; //red
                shapeArray[1] = "#b85450";
                shapeArray[2] = "rectangle";

                return shapeArray;
            }


            shapeArray[0] = "#dae8fc"; //blue
            shapeArray[1] = "#6c8ebf";
            shapeArray[2] = "rectangle";

            return shapeArray;
        }

    }

}
