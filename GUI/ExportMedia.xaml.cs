using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GameRes;
using GARbro.GUI.Strings;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace GARbro.GUI
{
    /// <summary>
    /// ExportMedia.xaml 的交互逻辑
    /// </summary>
    public partial class ExportMedia : Window
    {
        public ExportMedia()
        {
            InitializeComponent();
            ImageExportionFormat.ItemsSource = FormatCatalog.Instance.ImageFormats.Where (f => f.CanWrite);
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
