using CommandLine;

namespace HandlerVisualizer
{
    /// <summary>
    /// CommandLine arguments
    /// </summary>
    internal class Options
    {
        // Call drawio for CallFlowVisualizer
        [Option('d', "drawio", Required = false, HelpText = "Call drawio.exe for CallFlowVisualizer")]
        public bool drawio { get; set; } = false;

        // Call drawio.exe for CallFlowVisualizer and Convert to VISIO format
        [Option('v', "visio", Required = false, HelpText = "Call drawio.exe for CallFlowVisualizer and Convert to VISIO format")]
        public bool visio { get; set; } = false;

        // Call drawio.exe for CallFlowVisualizer and Convert to png format
        [Option('n', "png", Required = false, HelpText = "Call drawio.exe for CallFlowVisualizer and Convert to png format")]
        public bool png { get; set; } = false;

        // Call drawio.exe for CallFlowVisualizer and Convert to VISIO format
        [Option('h', "handlers", Required = false, HelpText = "Read all xml files in Handlers folder")]
        public bool handlers { get; set; } = false;

        // Set file name at arg[0]
        [Value(0,Hidden =true)]
        public string Filename { get; set; } = null!;

    }

}

