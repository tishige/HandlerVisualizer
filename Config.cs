using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandlerVisualizer
{

    public class HndvSettings
    {
        public bool AppendDateTimeToFileName { get; set; }
        public List<string> CustomizationPointFirstString { get; set; }
        public bool ShowStepParameters { get; set; }

    }

    public class DrawioSettings
    {
        public bool ColorNode { get; set; }
        public bool NodeRound { get; set; }
        public bool LineRound { get; set; }
        public int Nodespacing { get; set; }
        public int Levelspacing { get; set; }
        public bool ReplaceSpecialCharacter { get; set; }
        public int MaxRetryCount { get; set; }
        public bool ConvertToVisio { get; set; }
        public bool ConvertToPng { get; set; }
        public string Layout { get; set; }

    }

}
