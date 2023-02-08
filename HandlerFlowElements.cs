using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandlerVisualizer
{
    internal class HandlerFlowElements
    {
        internal string Id { get; set; } = null!;
        internal string Type { get; set; } = null!;
        internal string Label { get; set; } = null!;
        internal string CreatorName { get; set; } = null!;
        internal string Parameter { get; set; } = null!;
        internal List<ExitPath> ExitPaths { get; set; }= new List<ExitPath>();
        internal List<RefAndLabel> ExitPathsList { get; set; } = new List<RefAndLabel>();
        internal int ExitPathCount { get; set; }
        internal int maxCountOfRefPath { get; set; } = 0;
        internal int TopPos { get; set; } = 0;
        internal int LeftPos { get; set; } = 0;
        internal bool Isinitiator { get; set; }=false;
        internal string HandlerName { get; set; } = null;

    }

    internal class RefAndLabel
    {
        public int LabelIndex { get; set; } = 0;
        public string ParentId { get; set; } = null;
        public string ExitName { get; set; } = null;
        public string TargetRef { get; set; } = null;

    }

    internal class StepParameters
    {
        internal string Id { get; set; } = null!;
        public string ParameterLabelName { get; set; }
        public string ParameterValue { get; set; }
        public string ParameterIsInput { get; set; }
    }

    internal class ExitPath
    {
        public string Label { get; set; } = null!;
        public string TargetStepId { get; set; } = null;
    }

}
