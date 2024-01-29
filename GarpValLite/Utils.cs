using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml.Linq;
using System.Linq.Expressions;

namespace GarpValLite

{
    class GarpSettings
    {
        public string clientPath = string.Empty;
        public string dataPathCtree = string.Empty;
        public string dataPathRocksDB = string.Empty;
        public string serverPath = string.Empty;
        public bool manualPaths = false;

        public void writeNewFile(string path)
        {
            XDocument settingsXML;
            //string comment = <? xml version = "1.0" encoding = "utf-8" ?>
            string comment = "<? xml version = \"1.0\" encoding = \"utf-8\"?>";
            settingsXML = new XDocument(
                new XComment(comment),
                new XElement("Path",
                    new XElement("Client", @"n:\program\garp3\versioner\Client"),
                    new XElement("Server", @"N:\program\garp3\versioner\Server"),
                    new XElement("MainDbDirCtree", @"C:\garp\data\ctree\000"),
                    new XElement("MainDbDirRocksDB", @"C:\garp\data\RocksDB\000")
                )
            );
            settingsXML.Save(path);
        }

        public GarpSettings readSettings(string path)
        {
            XDocument xmlSettings = XDocument.Load(path);
            this.clientPath = xmlSettings.Root.Element("Client").Value.ToString();
            this.dataPathCtree = xmlSettings.Root.Element("MainDbDirCtree").Value;
            this.dataPathRocksDB = xmlSettings.Root.Element("MainDbDirRocksDB").Value;
            this.serverPath = xmlSettings.Root.Element("Server").Value;
            try
            {
                this.manualPaths = xmlSettings.Root.Element("ManualPaths").Value == "true";   
            }
            catch (Exception ex)
            {
                this.manualPaths = false;
                //https://stackoverflow.com/questions/7931650/adding-elements-to-an-xml-file-in-c-sharp
                XElement rootElement = xmlSettings.Root.Element("Path");
                rootElement.Add(new XElement("ManualPaths", this.manualPaths.ToString()));
            }
            return this;
        }

        public void updateSettings(string path, GarpSettings settings)
        {
            XDocument xmlGarpSettings = XDocument.Load(path);
            if (settings.clientPath != "")
                xmlGarpSettings.Root.Element("Client").Value = settings.clientPath;
            if (settings.dataPathCtree != "")
                xmlGarpSettings.Root.Element("MainDbDirCtree").Value = settings.dataPathCtree;
            if (settings.dataPathRocksDB != "")
                xmlGarpSettings.Root.Element("MainDbDirRocksDB").Value = settings.dataPathRocksDB;
            if (settings.serverPath != "")
                xmlGarpSettings.Root.Element("Server").Value = settings.serverPath;
            xmlGarpSettings.Root.Element("ManualPaths").Value = settings.manualPaths.ToString();
            xmlGarpSettings.Save(path);
        }

    }

    class Logg
    {
        string sLogPath = System.IO.Path.GetTempPath() + "garpval.log";
        public void write2logfile(string sLogtext)
        {
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(sLogPath, true))
            {
                file.WriteLine(DateTime.Now.ToString("yyyyMMdd HH:mm:ss ") + sLogtext);
            }
        }

    }
}
