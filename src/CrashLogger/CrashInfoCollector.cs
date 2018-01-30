using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollector.InProcDataCollector;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.InProcDataCollector;

namespace Microsoft.Vsts.CrashLogger
{
    [DataCollectorFriendlyName("VstsCrashInfoCollector")]
    [DataCollectorTypeUri("InProcDataCollector://VSTS/CrashInfoCollector/0.1")]
    public class CrashInfoCollector : InProcDataCollection
    {
        private string m_fileName;
        private static readonly XName s_logPathName = XName.Get("LogPath");

        public void Initialize(IDataCollectionSink dataCollectionSink)
        {
            // Do Nothing
        }

        /// <summary>
        /// The test session start.
        /// </summary>
        /// <param name="testSessionStartArgs">
        /// The test session start args.
        /// </param>
        public void TestSessionStart(TestSessionStartArgs testSessionStartArgs)
        {
            var configuration = testSessionStartArgs.Configuration;

            if (!string.IsNullOrWhiteSpace(configuration))
            {
                try
                {
                    var root = XDocument.Parse(configuration).Root;
                    var logPath = root?.Element(s_logPathName)?.Value;
                    if (logPath != null)
                    {
                        this.m_fileName = Path.Combine(logPath, "testevents.log");
                    }
                }
                catch (Exception)
                {
                    // If parsing failed, we will just return null.
                }
            }
        }

        /// <summary>
        /// The test case start.
        /// </summary>
        /// <param name="testCaseStartArgs">
        /// The test case start args.
        /// </param>
        public void TestCaseStart(TestCaseStartArgs testCaseStartArgs)
        {
            InvokeEvent(() =>
            {
                File.AppendAllLines(this.m_fileName,
                    new string[1] {"TestCaseStart:" + testCaseStartArgs.TestCase.FullyQualifiedName});
            });
        }

        /// <summary>
        /// The test case end.
        /// </summary>
        /// <param name="testCaseEndArgs">
        /// The test case end args.
        /// </param>
        public void TestCaseEnd(TestCaseEndArgs testCaseEndArgs)
        {
            InvokeEvent(() =>
            {
                File.AppendAllLines(this.m_fileName,
                    new string[1] {"TestCaseEnd:" + testCaseEndArgs.DataCollectionContext.TestCase.FullyQualifiedName});
            });
        }

        /// <summary>
        /// The test session end.
        /// </summary>
        /// <param name="testSessionEndArgs">
        /// The test session end args.
        /// </param>
        public void TestSessionEnd(TestSessionEndArgs testSessionEndArgs)
        {
            // Do Nothing
        }

        private void InvokeEvent(Action action)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
