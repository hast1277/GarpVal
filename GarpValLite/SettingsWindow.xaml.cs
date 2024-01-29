using System;
using System.IO;
using System.Net.Security;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;


namespace GarpValLite
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        bool bUpdateCtreePath = false;
        bool bUpdateRocksDBPath = false;
        GarpSettings _garpSettings;
        private MainWindow wnd;
        public MainWindow PropMainWindow
        {
            set { wnd = value; }
        }
        public SettingsWindow()
        {
            InitializeComponent();
            _garpSettings = new GarpSettings();
            _garpSettings = _garpSettings.readSettings(AppDomain.CurrentDomain.BaseDirectory + "\\settings.xml");

            txtMainFolderClient.Text = _garpSettings.clientPath;
            txtMainFolderServer.Text = _garpSettings.serverPath;
            txtMainDbDirCtree.Text = _garpSettings.dataPathCtree;
            txtMainDbDirRocksDB.Text = _garpSettings.dataPathRocksDB;
            bUpdateCtreePath = false; 
            bUpdateRocksDBPath = false;
            rbNetwork.IsChecked = true;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            wnd.loadFolders();
            this.Close();
        }

        private void BtnMainFolderClient_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dlgClient = new CommonOpenFileDialog();
            dlgClient.Filters.Add(new CommonFileDialogFilter("Garpklient (Garp.exe)", "*.exe"));
            dlgClient.InitialDirectory = @"\\jvsborfs02\Common\program\garp3\versioner\Client";
            dlgClient.IsFolderPicker = false;
            dlgClient.Title = "Ange sökväg till klienten (Garp.exe)";
            CommonFileDialogResult clientResult = dlgClient.ShowDialog();
            txtMainFolderClient.Text = dlgClient.FileName;
        }
        private void BtnMainFolderServer_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dlgServer = new CommonOpenFileDialog();
            dlgServer.Title = "Ange sökväg till servern (serv.exe)";
            if (Directory.Exists(Environment.GetEnvironmentVariable("ProgramFiles(x86)") + @"\Jeeves Information Systems AB"))
                dlgServer.InitialDirectory = Environment.GetEnvironmentVariable("ProgramFiles(x86)") + @"\Jeeves Information Systems AB";

            else if (Directory.Exists(Environment.GetEnvironmentVariable("ProgramFiles(x86)") + @"\Garp"))
                dlgServer.InitialDirectory = Environment.GetEnvironmentVariable("ProgramFiles(x86)") + @"Garp";

            else
                dlgServer.InitialDirectory = @"C:\";

            dlgServer.IsFolderPicker = true;
            dlgServer.Title = "Välj Garpserver";

            CommonFileDialogResult ServerResult = dlgServer.ShowDialog();
            txtMainFolderServer.Text = dlgServer.FileName;

        }

        private void BtnMainDbDirCtree_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dlgMainDbDir = new CommonOpenFileDialog();
            if (Directory.Exists(@"C:\Data"))
                dlgMainDbDir.InitialDirectory = @"C:\Data";
            else if (Directory.Exists(@"C:\Garp\Data\Ctree"))
                dlgMainDbDir.InitialDirectory = @"C:\Garp\Data\Ctree";
            else
                dlgMainDbDir.InitialDirectory = @"C:\";
            dlgMainDbDir.IsFolderPicker = true;

            CommonFileDialogResult ctreeResult = dlgMainDbDir.ShowDialog();
            txtMainDbDirCtree.Text = dlgMainDbDir.FileName;
        }

        private void BtnMainDbDirRocksDB_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dlgMainDbDir = new CommonOpenFileDialog();
            if (Directory.Exists(@"C:\Data"))
                dlgMainDbDir.InitialDirectory = @"C:\Data";
            else if (Directory.Exists(@"C:\Garp\Data\RocksDB"))
                dlgMainDbDir.InitialDirectory = @"C:\Garp\Data\RocksDB";
            else
                dlgMainDbDir.InitialDirectory = @"C:\";
            dlgMainDbDir.IsFolderPicker = true;

            CommonFileDialogResult rocksDBResult = dlgMainDbDir.ShowDialog();
            txtMainDbDirRocksDB.Text = dlgMainDbDir.FileName;
        }

        private void SaveSettings()
        {
            if (bUpdateCtreePath)
                _garpSettings.dataPathCtree = txtMainDbDirCtree.Text;
            if (bUpdateRocksDBPath)
                _garpSettings.dataPathRocksDB = txtMainDbDirRocksDB.Text;
            if(bUpdateRocksDBPath == true | bUpdateCtreePath == true)
                MessageBox.Show("Inställning sparad!", "GarpVal", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            _garpSettings.updateSettings(AppDomain.CurrentDomain.BaseDirectory + "\\settings.xml", _garpSettings);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            bUpdateCtreePath = false;
            bUpdateRocksDBPath = false;
        }

        private void RbLocal_Click(object sender, RoutedEventArgs e)
        {
            btnMainFolderClient.IsEnabled = true;
            txtMainFolderClient.IsEnabled = true;
            btnMainFolderServer.IsEnabled = true;
            txtMainFolderServer.IsEnabled = true;
            _garpSettings.manualPaths = true;
        }

        private void RbNetwork_Checked(object sender, RoutedEventArgs e)
        {
            txtMainFolderClient.Text = @"\\jvsborfs02\Common\program\garp3\versioner\Client";
            txtMainFolderServer.Text = @"\\jvsborfs02\Common\program\garp3\versioner\Server";
            btnMainFolderClient.IsEnabled = false;
            txtMainFolderClient.IsEnabled = false;
            btnMainFolderServer.IsEnabled = false;
            txtMainFolderServer.IsEnabled = false;
            _garpSettings.manualPaths = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }

        private void txtMainDbDirCtree_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            bUpdateCtreePath = true;
        }

        private void txtMainDbDirRocksDB_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            bUpdateRocksDBPath = true;
        }
    }
}
