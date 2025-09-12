using Microsoft.VisualStudio.TestWindow.Extensibility;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ZigVS.Test
{
    [Export(typeof(IRunSettingsService))]
    [Name("ZigVsRunSettings")]
    //[Order(Before = "Default")]
    internal sealed class TestRunSettingsProvider : IRunSettingsService
    {
        public string Name => "ZigVsRunSettings";

        public IXPathNavigable AddRunSettings(
            IXPathNavigable inputRunSettingDocument,
            IRunSettingsConfigurationInfo configurationInfo,
            ILogger log)
        {
            try
            {
                // 1) Load incoming into XmlDocument (editable)
                var doc = new XmlDocument { PreserveWhitespace = true };

                if (inputRunSettingDocument != null)
                {
                    var nav = inputRunSettingDocument.CreateNavigator();
                    var xml = nav?.OuterXml;
                    if (!string.IsNullOrWhiteSpace(xml))
                    {
                        doc.LoadXml(xml);
                    }
                }

                // Ensure <RunSettings> root exists
                if (doc.DocumentElement is null || doc.DocumentElement.Name != "RunSettings")
                {
                    doc.RemoveAll();
                    doc.AppendChild(doc.CreateElement("RunSettings"));
                }

                var root = doc.DocumentElement!;

                // 2) Upsert your section/value
                var zig = root.SelectSingleNode("ZigVs") as XmlElement
                          ?? (XmlElement)root.AppendChild(doc.CreateElement("ZigVs"));

                var toolNode = zig.SelectSingleNode("ToolPath") as XmlElement
                               ?? (XmlElement)zig.AppendChild(doc.CreateElement("ToolPath"));

                // Get your cached/expanded value (avoid UI-thread-only calls here)
                var l_generalOptions = Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () => {
                    return await GeneralOptions.GetLiveInstanceAsync();
                });

                var toolPath = l_generalOptions.ToolPathExpanded;
                toolNode.InnerText = toolPath; // auto-escapes

                log?.Log(MessageLevel.Informational, $"ZigVs injected ToolPath='{toolPath}'");

                // 3) Return the *XmlDocument* (implements IXPathNavigable and is editable)
                return doc;
            }
            catch (Exception ex)
            {
                log?.Log(MessageLevel.Error, "ZigVs AddRunSettings failed: " + ex);
                return inputRunSettingDocument; // graceful fallback
            }
        }
    }
}
