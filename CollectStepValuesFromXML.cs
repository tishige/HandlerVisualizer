using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using ShellProgressBar;


namespace HandlerVisualizer
{
    class CollectStepValuesFromXML
    {
        internal static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        internal static List<HandlerFlowElements> CollectSteps(string xmlFileName,bool showStepParameters)
        {

            XElement xmllist = XElement.Load(xmlFileName);

            IEnumerable<XElement> allSteps = xmllist.Descendants("Step");

            List<HandlerFlowElements> handlerFlowElementsList = new List<HandlerFlowElements>();

            // Load description
            XmlTextReader reader = null;
            reader = new XmlTextReader(xmlFileName);
            XElement items = XElement.Load(reader);
            string description = items.Attribute("description").ToString();
            description= description.Replace("&#xD;&#xA;", "<br>").Replace("\"", "").Replace("\\", "_").Replace("&#x9","").Replace("description=", "");

            // Set Handler Info as ID -1
            HandlerFlowElements handlerInfo = new HandlerFlowElements();
            handlerInfo.Id = "-1";
            handlerInfo.HandlerName = xmllist.Attribute("name").Value;
            handlerInfo.Label = description;
            handlerInfo.CreatorName = xmllist.Attribute("name").Value;
            handlerInfo.Type = "description";
            handlerFlowElementsList.Add(handlerInfo);

            List<RefAndLabel> refAndLabelList = new List<RefAndLabel>();

            //int maxCountOfExitPath = 0;
            int maxCountOfRefPath = 0;

            foreach (var allSteps_i in allSteps)
            {
                HandlerFlowElements handlerFlowElements= new HandlerFlowElements();

                var stepId = allSteps_i.Attribute("id").Value;

                handlerFlowElements.Id= allSteps_i.Attribute("id").Value;
                handlerFlowElements.Type= allSteps_i.Attribute("type").Value;

                string creatorName = allSteps_i.Attribute("creatorName").Value;
                handlerFlowElements.CreatorName = creatorName;

                handlerFlowElements.Label= allSteps_i.Attribute("label").Value;
                handlerFlowElements.Isinitiator = allSteps_i.Attribute("isInitiator") != null ? true : false;
                handlerFlowElements.TopPos = Int32.Parse(allSteps_i.Attribute("top").Value);
                handlerFlowElements.LeftPos = Int32.Parse(allSteps_i.Attribute("left").Value);

                var parameters = allSteps_i.Descendants("Parameter");

                List<StepParameters> stepParametersList = new List<StepParameters>();

                string parameterInputStrings = "";
                string parameterOutputStrings = "";


                if (parameters!=null && showStepParameters)
                {
                    foreach (var parameter_i in parameters)
                    {
                        StepParameters stepParameters = new StepParameters();
                        stepParameters.Id = handlerFlowElements.Id;
                        stepParameters.ParameterLabelName = parameter_i.Attribute("label") != null ? parameter_i.Attribute("label").Value : null;
                        if (String.IsNullOrEmpty(stepParameters.ParameterLabelName))
                        {
                            stepParameters.ParameterLabelName = parameter_i.Attribute("name") != null ? parameter_i.Attribute("name").Value : null;
                        }
                        
                        stepParameters.ParameterValue = parameter_i.Attribute("value") != null ? parameter_i.Attribute("value").Value : null;
                        stepParameters.ParameterIsInput = parameter_i.Attribute("isInput") != null ? parameter_i.Attribute("isInput").Value : "false";
                        stepParametersList.Add(stepParameters);

                    }


                    if(stepParametersList.Count > 0)
                    {
                         var inputParams = stepParametersList.Select(x=> new { x.ParameterLabelName,x.ParameterValue,x.ParameterIsInput}).GroupBy(x=>x.ParameterIsInput).ToList();
                        foreach (var inputParams_i in inputParams)
                        {
                            if (inputParams_i.Key=="True")
                            {
                                parameterInputStrings = "<b>Inputs<br></b>";
                                foreach (var inputParams_j in inputParams_i)
                                {
                                    Logger.Info("hit");
                                    parameterInputStrings = parameterInputStrings + inputParams_j.ParameterLabelName+" : "+inputParams_j.ParameterValue+"<br>";
                                }


                            }
                            else if(inputParams_i.Key == "False")
                            {
                                parameterOutputStrings = "<b>Outputs<br></b>";
                                foreach (var inputParams_j in inputParams_i)
                                {
                                    Logger.Info("hit");
                                    parameterOutputStrings = parameterOutputStrings + inputParams_j.ParameterLabelName + " : " + inputParams_j.ParameterValue + "<br>";
                                }
                            }
                            else
                            {
                                parameterOutputStrings = "<b>Parameters<br></b>";


                                if (inputParams_i.FirstOrDefault().ParameterLabelName == "LHS")
                                {
                                    parameterOutputStrings = parameterOutputStrings + inputParams_i.FirstOrDefault().ParameterValue + "=" + inputParams_i.LastOrDefault().ParameterValue;
                                }
                                else
                                {

                                    foreach (var inputParams_j in inputParams_i)
                                    {
                                        Logger.Info("hit");
                                        parameterOutputStrings = parameterOutputStrings + inputParams_j.ParameterLabelName + " : " + inputParams_j.ParameterValue + "<br>";
                                    }

                                }

                            }

                        }


                    }


                }

                if(parameterInputStrings.Length > 0||parameterOutputStrings.Length>0)
                {
                    handlerFlowElements.Label = allSteps_i.Attribute("label").Value + "<br><br>" + parameterInputStrings + parameterOutputStrings;
                }


                List<ExitPath> exitPathList = new List<ExitPath>();

                var exitPathXML = allSteps_i.Descendants("ExitPath");
                foreach (var exitPathXML_i in exitPathXML)
                {
                    ExitPath exitPath = new ExitPath();
                    
                    exitPath.Label= exitPathXML_i.Attribute("label").Value;
                    exitPath.TargetStepId= (string)(exitPathXML_i.Attribute("targetStepID") != null ? exitPathXML_i.Attribute("targetStepID") : null);
                    exitPathList.Add(exitPath);
                }

                for (int i = 0; i < exitPathList.Count(); i++)
                {
                    RefAndLabel refAndLabels = new RefAndLabel();
                    refAndLabels.LabelIndex = i;
                    refAndLabels.TargetRef = exitPathList[i].TargetStepId;
                    refAndLabels.ParentId = stepId;
                    refAndLabels.ExitName = exitPathList[i].Label;
                    refAndLabelList.Add(refAndLabels); // Flat data list toNodeExitLabel0 toNodeExitLabelN

                }

                handlerFlowElements.ExitPaths = exitPathList;
                handlerFlowElements.ExitPathCount=exitPathList.Count;
                handlerFlowElementsList.Add(handlerFlowElements);

                if (exitPathList.Count >= maxCountOfRefPath)
                {
                    maxCountOfRefPath = exitPathList.Count;
                    handlerFlowElements.maxCountOfRefPath = maxCountOfRefPath;
                }

            }

            int groupedListCount = 0;

            do
            {
                var groupByLabelIndex = refAndLabelList.GroupBy(x => x.LabelIndex).ToList();
                List<IGrouping<string, RefAndLabel>> groupedList = new List<IGrouping<string, RefAndLabel>>();
                //Repeat per column
                foreach (var groupByLabelIndex_i in groupByLabelIndex)
                {
                    var duplicateTargetRefs = groupByLabelIndex_i.GroupBy(x => x.TargetRef).Where(g => g.Count() > 1).ToList();
                    //groupedList key:duplicatedSteps value:steps gathered to this key step
                    groupedList.AddRange(duplicateTargetRefs);
                }

                groupedListCount = groupedList.Count;
                // key:null means disconnect steps or no next Step
                foreach (var groupedList_i in groupedList)
                {
                    for (int i = 1; i < groupedList_i.Count(); i++)
                    {
                        int idx = groupedList_i.ElementAt(1).LabelIndex;
                        idx++;
                        groupedList_i.ElementAt(i).LabelIndex = idx;

                    }

                }

                // Repeat no duplicated targetRef in a column
            } while (groupedListCount != 0); 

            // Update LabelIndex if it duplicated in a step
            var duplicateParentInRefList = refAndLabelList.GroupBy(x => x.ParentId).Where(g => g.Count() > 1).ToList();
            foreach (var duplicateParentInRefList_i in duplicateParentInRefList)
            {
                var duplicateLabel = duplicateParentInRefList_i.GroupBy(x => x.LabelIndex).Where(g => g.Count() > 1).ToList();
                if(duplicateLabel.Count() > 0)
                {

                    int duplicateKeyStart = duplicateLabel.FirstOrDefault().Key;

                    string duplicateKeyStartFirstStepExitName= duplicateLabel.FirstOrDefault().FirstOrDefault().ExitName;
                    int duplicateKeyStartFirstStepLabelIndex = duplicateLabel.FirstOrDefault().FirstOrDefault().LabelIndex;
                    string duplicateKeyStartFirstStepParentId = duplicateLabel.FirstOrDefault().FirstOrDefault().ParentId;
                    string duplicateKeyStartFirstStepTargetRef = duplicateLabel.FirstOrDefault().FirstOrDefault().TargetRef;
                    var duplicateParentInRefList_iList= duplicateParentInRefList_i.ToList();
                    int duplicateKeyStartFirstStepPos = duplicateParentInRefList_iList.FindIndex(x => x.ExitName == duplicateKeyStartFirstStepExitName && x.LabelIndex == duplicateKeyStartFirstStepLabelIndex && x.ParentId == duplicateKeyStartFirstStepParentId && x.TargetRef == duplicateKeyStartFirstStepTargetRef);

                    for (int i = duplicateKeyStartFirstStepPos; i < duplicateParentInRefList_i.Count(); i++)
                    {
                        duplicateParentInRefList_i.ElementAt(i).LabelIndex=duplicateKeyStart;
                        duplicateKeyStart++;
                        if (duplicateKeyStart > maxCountOfRefPath)
                        {
                            maxCountOfRefPath=duplicateKeyStart;
                        }

                    }

                }

            }

            foreach (var handlerFlowElementsList_i in handlerFlowElementsList)
            {
                foreach (var refAndLabelList_i in refAndLabelList)
                {
                    if(refAndLabelList_i.ParentId==handlerFlowElementsList_i.Id)
                    {
                        handlerFlowElementsList_i.ExitPathsList.Add( new HandlerVisualizer.RefAndLabel { ParentId=refAndLabelList_i.ParentId,LabelIndex=refAndLabelList_i.LabelIndex,ExitName=refAndLabelList_i.ExitName,TargetRef=refAndLabelList_i.TargetRef});
                        handlerFlowElementsList_i.maxCountOfRefPath = maxCountOfRefPath;

                    }

                }

            }

            return (handlerFlowElementsList.OrderByDescending(x=>x.Isinitiator).ThenBy(x=>x.TopPos).ThenBy(x=>x.LeftPos).ToList());

        }

    }


}
