using System.Linq;
using System.Windows;
using System.Windows.Input;
using GameRes;
using GARbro.GUI.Strings;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace GARbro.GUI
{
    /// <summary>
    /// PackMedia.xaml 的交互逻辑
    /// </summary>
    public partial class PackMedia : Window
    {
        public PackMedia()
        {
            InitializeComponent();
            ImagePackFormat.ItemsSource = FormatCatalog.Instance.ImageFormats.Where (f => f.CanExportAndPack);
        }

        private void BrowseExec (object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog
            {
                Title = guiStrings.TextChooseDestDir,
                IsFolderPicker = true,
                InitialDirectory = DestinationDir.Text,

                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true,
            };
            if (dlg.ShowDialog (this) == CommonFileDialogResult.Ok)
                DestinationDir.Text = dlg.FileName;
        }

        public void CanExecuteAlways (object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ConvertButton_Click (object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void acb_OnEnterKeyDown (object sender, KeyEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
