using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Security.Principal;
using System.Threading;
using System.Windows.Threading;
using System.Runtime.CompilerServices;

namespace GarpValLite
{
    public partial class MainWindow : Window
    {
        string sClientPath = string.Empty;
        string sClientFolder = string.Empty;
        string sServerPath = string.Empty;
        string sMainDbDir = string.Empty;
        string settingsPath = AppDomain.CurrentDomain.BaseDirectory + "\\settings.xml";
        GarpSettings _garpSettings = new GarpSettings();
        
        Logg logg = new Logg();
        List<string> lstGarpServers = new List<string>();

        private void App_DispatcherUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            logg.write2logfile("*****************************************************");
            logg.write2logfile(e.Message);
            logg.write2logfile("Klientsökväg: " + sClientPath);
            logg.write2logfile("Serversökväg: " + sServerPath + @"\serv.exe");
            logg.write2logfile("MainDbDir: " + sMainDbDir);
            logg.write2logfile("*****************************************************");
        }
        
        private string getManualServerPath()
        {
            string sPath = string.Empty;

            CommonOpenFileDialog dlgServer = new CommonOpenFileDialog();
            dlgServer.Title = "Ange sökvägen för Garpservern";
            dlgServer.IsFolderPicker = false;
            CommonFileDialogResult ServerResult = dlgServer.ShowDialog();
            sPath = dlgServer.FileName;
            _garpSettings.serverPath = sPath;
            
            return sPath;
        }

        private string getClientPath(string sClient)
        {
            if (!File.Exists(sClientPath) || (sClient == ""))
            {
                if (Directory.Exists(Environment.GetEnvironmentVariable("ProgramFiles(x86)") + "\\Jeeves Information Systems AB"))
                    sClientPath = Environment.GetEnvironmentVariable("ProgramFiles(x86)") + "\\Jeeves Information Systems AB";
                else
                    if (Directory.Exists(Environment.GetEnvironmentVariable("ProgramFiles(x86)") + @"\Garp"))
                    sClientPath = Environment.GetEnvironmentVariable("ProgramFiles(x86)") + @"\Garp";
                else
                    sClientPath = @"C:\";
                CommonOpenFileDialog dlgClient = new CommonOpenFileDialog();
                dlgClient.Title = "Välj klient till server: " + sServerPath;
                dlgClient.Filters.Add(new CommonFileDialogFilter("Garpklient (Garp.exe)", "*.exe"));
                dlgClient.InitialDirectory = sClientPath;
                dlgClient.IsFolderPicker = false;

                CommonFileDialogResult clientResult = dlgClient.ShowDialog();
                sClientPath = dlgClient.FileName;
            }

            else
                sClientPath = sClient + @"\Garp" + lbVersions.SelectedValue + @"\Garp.exe";
            sClientFolder = sClientPath.Substring(0, sClientPath.IndexOf("Garp.exe"));
            return sClientPath;
        }

        private void mnuManualPaths_Click(object sender, RoutedEventArgs e)
        {
            sClientPath = getClientPath("");
            sServerPath = getManualServerPath(); ;
            StartGarp();
        }

        private bool checkIfAdmin()
        {
            bool isAdmin = false;
            if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                isAdmin = true;
            return isAdmin;
        }
        public MainWindow()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            InitializeComponent();
            logg.write2logfile("*****************************************************");
            logg.write2logfile("Kollar om GarpVal körs som administratör");

            if (!checkIfAdmin())
            {
                logg.write2logfile("*****************************************************");
                logg.write2logfile("Garpval måste köras som administratör. Garpval avslutas. Högerklicka och välj Kör som administratör");
                logg.write2logfile("*****************************************************");
                MessageBox.Show("Garpval måste köras som administratör. Garpval avslutas. Högerklicka och välj Kör som administratör");
                Application.Current.Shutdown();
            }
            logg.write2logfile("*****************************************************");
            logg.write2logfile("Garpval körs som administratör. ");
            logg.write2logfile("*****************************************************");
        }

        private void logInvalidSettings()
        {
            lbVersions.Items.Add("Se över inställningarna");
            logg.write2logfile("*****************************************************");
            logg.write2logfile("Ogiltiga inställningar");
            logg.write2logfile("Klientsökväg: " + sClientPath + @"\Garp.exe");
            logg.write2logfile("Serversökväg: " + sServerPath + @"\serv.exe");
            logg.write2logfile("MainDbDirCtree: " + _garpSettings.dataPathCtree);
            logg.write2logfile("MainDbDirCtree: " + _garpSettings.dataPathRocksDB);
            logg.write2logfile("*****************************************************");
        }

        private void updateVersion()
        {
            logg.write2logfile("*****************************************************");
            logg.write2logfile("Läser Garpversioner från " + sServerPath);
            logg.write2logfile("*****************************************************");
            if (_garpSettings.manualPaths == true)
            {
                lbVersions.Items.Add(_garpSettings.serverPath);
                return;
            }
                
            string sFileVersion = string.Empty;
            string sPath = sServerPath;
            if (sPath == "")
            {
                logInvalidSettings();
                lbVersions.Items.Clear();
                lbVersions.Items.Add("Se över inställningarna");
                return;
            }

            int nrOfServerPaths = 0;
            //localGarp = (bool)Properties.Settings.Default.localgarp;
            IEnumerable<string> path = new List<string>();
            try
            {
                path = Directory.EnumerateDirectories(sPath);
            }

            catch (Exception ex)
            {
                //Skriv felmeddelande till loggfil
                logg.write2logfile(ex.Message);
            }
            if (lbVersions.Items.Count > 0)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    lbVersions.Items.Clear();
                    lstGarpServers.Clear();
                }));
            }

            nrOfServerPaths = path.Count();
            if (nrOfServerPaths == 0)
            {
                try
                {
                    path = Directory.EnumerateDirectories(getManualServerPath());
                }
                catch (Exception ex)
                {
                    //Skriv felmeddelande till loggfil
                    logg.write2logfile(ex.Message);
                }

            }

            foreach (string sGarpServerPath in path)
            {
                int iPosGarpServerVersion = -10;
                if (File.Exists(sGarpServerPath + "\\serv.exe"))
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        iPosGarpServerVersion = sGarpServerPath.IndexOf("4.");
                        if (iPosGarpServerVersion == -1)
                            iPosGarpServerVersion = sGarpServerPath.IndexOf("3.");
                        if (iPosGarpServerVersion == -1)
                            iPosGarpServerVersion = 0;
                        if (iPosGarpServerVersion == 0)
                            lbVersions.Items.Add("Okänd version");
                        else
                        {
                            sFileVersion = sGarpServerPath.Substring(iPosGarpServerVersion);
                            if (sFileVersion.IndexOf("\\") > -1)
                                sFileVersion = sFileVersion.Substring(0, sFileVersion.IndexOf("\\"));
                            if (sFileVersion == "4.4.14")
                                lbVersions.Items.Add(sFileVersion + " funkar ej tyvärr!!");
                            else
                                lbVersions.Items.Add(sFileVersion);
                        }
                        lstGarpServers.Add(sGarpServerPath + "\\serv.exe");
                    }));
                }

                else
                {
                    IEnumerable<string> pathSubfolder = new List<string>();
                    pathSubfolder = Directory.EnumerateDirectories(sGarpServerPath);
                    foreach (string sGarpServerSubFolderPath in pathSubfolder)
                    {
                        if (File.Exists(sGarpServerSubFolderPath + "\\serv.exe"))
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                iPosGarpServerVersion = sGarpServerSubFolderPath.IndexOf("4.");
                                if (iPosGarpServerVersion == -1)
                                    iPosGarpServerVersion = sGarpServerSubFolderPath.IndexOf("3.");
                                if (iPosGarpServerVersion == -1)
                                    iPosGarpServerVersion = 0;
                                if (iPosGarpServerVersion == 0)
                                    lbVersions.Items.Add("Okänd version");
                                else
                                {
                                    sFileVersion = sGarpServerSubFolderPath.Substring(iPosGarpServerVersion);
                                    if (sFileVersion.IndexOf("\\") > -1)
                                        sFileVersion = sFileVersion.Substring(0, sFileVersion.IndexOf("\\"));
                                    lbVersions.Items.Add(sFileVersion);
                                }
                                lstGarpServers.Add(sGarpServerSubFolderPath + "\\serv.exe");

                            }));

                        }
                    }
                }
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (lbVersions.Items.Count == 0)
                {
                    logInvalidSettings();
                    lbVersions.Items.Clear();
                    lbVersions.Items.Add("Se över inställningarna");
                }
                lbVersions.SelectedIndex = 0;
            }));
        }

        public void loadFolders()
        {

            Thread threadGetVersion = new Thread(updateVersion);
            threadGetVersion.Start();
            if (lbVersions.Items.Count > 0)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    lbVersions.Items.Clear();
                    lstGarpServers.Clear();
                }));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            logg.write2logfile("*****************************************************");
            logg.write2logfile("Laddar XML-komponent så inställningsfilen kan läsas...");
            logg.write2logfile("*****************************************************");

            if (!File.Exists(settingsPath))
            {
                logg.write2logfile("*****************************************************");
                logg.write2logfile("Inställningsfil saknas! Skapar ny");
                logg.write2logfile("*****************************************************");

                //garpXML.writeNewFile(settingsPath);
                _garpSettings.writeNewFile(settingsPath);
            }
            logg.write2logfile("*****************************************************");
            logg.write2logfile("XML-komponent laddad. Laddar inställningsfil");
            logg.write2logfile("*****************************************************");


            _garpSettings = _garpSettings.readSettings(settingsPath);
            logg.write2logfile("*****************************************************");
            logg.write2logfile("Inställningar inlästa!");
            logg.write2logfile("*****************************************************");
            //sServerPath = Properties.Settings.Default.serverfolder;
            sServerPath = _garpSettings.serverPath;
            sClientPath = _garpSettings.clientPath;
            loadFolders();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            sClientFolder = sClientPath + @"\Garp" + lbVersions.SelectedValue;
            //sClientPath = sClientPath + @"\Garp" + lbVersions.SelectedValue + @"\Garp.exe";
            sClientPath = sClientFolder + @"\Garp.exe";
            sServerPath = Path.GetFullPath(lstGarpServers[lbVersions.SelectedIndex]);
            StartGarp();
        }

        private void UpdateRegistry(string sServerPath, string sMainDbDir, bool bLinux)
        {
            // First write the server key
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microcraft\\ServerVAL");
            key.SetValue("MainDbDir", sMainDbDir);
            key.SetValue("Path", sServerPath);
            key.SetValue("PortNumber", "2222");
            if (bLinux)
                key.SetValue("NumDataFormat", "Z");
            else
                key.SetValue("NumDataFormat", "W");
            key.Close();

            // write the client key
            key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microcraft\\GarpVAL");
            key.SetValue("ComType", "mcLocal");
            key.SetValue("ServerKey", "VAL");
            key.Close();
        }

        private void lbVersions_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            StartGarp();
        }

        public bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        private void StartGarp()
        {

            sServerPath = Path.GetFullPath(sServerPath);
            //string sourceData = string.Empty;
            string sFileVersion = string.Empty;

            bool bLinux = false;
            if (lbVersions.Items.Count == 0)
                return;
            
            try
            {
                FileVersionInfo FileVersion = FileVersionInfo.GetVersionInfo(sServerPath);
                sFileVersion = FileVersion.FileVersion.Replace(".", "");
            }
            catch (Exception ex)
            {
                sFileVersion = Path.GetFileName(Path.GetDirectoryName(sServerPath));
                sFileVersion = sFileVersion.Replace(".", "");
                //Skriv felmeddelande till loggfil
                logg.write2logfile(ex.Message);
            }
            Int32 iServerVersion = Int32.Parse(sFileVersion);
            if ((sFileVersion.Substring(1, 1) == "4") || (iServerVersion >= 433001))
            {
                sMainDbDir = _garpSettings.dataPathRocksDB;
            }

            else
            {
                sMainDbDir = _garpSettings.dataPathCtree;
                if (MessageBox.Show("Är databasen i linuxformat?", "Linuxformat", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
                    bLinux = true;
            }

            Directory.CreateDirectory(sMainDbDir);
            bool emptyFolder = IsDirectoryEmpty(sMainDbDir);
            if (emptyFolder)
                MessageBox.Show("Datamappen verkar tom. Se över inställningarna i filen settings.xml");

            // Update registry
            UpdateRegistry(sServerPath, sMainDbDir, bLinux);

            //Kopiera dtbas.lev till klientmappen för säkerhets skull...
            if (!File.Exists(sClientFolder + "\\DTBAS.LEV"))
                File.Copy(AppDomain.CurrentDomain.BaseDirectory + "\\DTBAS.LEV", sClientFolder + "\\DTBAS.LEV");

            try
            {
                ProcessStartInfo proc = new ProcessStartInfo(sClientPath, "/k VAL")
                {
                    UseShellExecute = false,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Maximized,
                    FileName = sClientPath,
                    CreateNoWindow = false
                };
                proc.WorkingDirectory = Path.GetDirectoryName(sClientFolder);
                Process.Start(proc);
            }
            catch (Exception ex)
            { 
                logg.write2logfile(ex.Message); 
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow wndSettings = new SettingsWindow();
            wndSettings.PropMainWindow = this;
            wndSettings.Show();
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            //_garpSettings.manualPaths = false;
        }
    }
}
