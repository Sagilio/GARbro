using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GameRes;
using GARbro.GUI.Properties;
using GARbro.GUI.Strings;

namespace GARbro.GUI
{

    public partial class MainWindow : Window
    {
        /// <summary>
        /// Convert selected images to another format.
        /// </summary>
        void ExportMediaExec (object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewModel.IsArchive)
                return;
            var source = from entry in CurrentDirectory.SelectedItems.Cast<EntryViewModel>()
                         where entry.Type == "image"
                         select entry.Source;
            var entries = source as Entry[] ?? source.ToArray();
            if (!entries.Any())
            {
                PopupError (guiStrings.MsgNoMediaFiles, guiStrings.TextMediaConvertError);
                return;
            }
            var export_dialog = new ExportMedia();

            string destination = ViewModel.Path.First();
            if (ViewModel.IsArchive)
                destination = Path.GetDirectoryName (destination);
            if (!IsWritableDirectory(destination) && Directory.Exists(Settings.Default.appLastDestination))
                destination = Settings.Default.appLastDestination;
            export_dialog.DestinationDir.Text = destination;

            export_dialog.Owner = this;
            var result = export_dialog.ShowDialog() ?? false;
            if (!result)
                return;
            var format = export_dialog.ImageExportionFormat.SelectedItem as ImageFormat;
            if (null == format)
            {
                Trace.WriteLine ("Format is not selected", "ConvertMediaExec");
                return;
            }
            try
            {
                destination = export_dialog.DestinationDir.Text;
                Directory.SetCurrentDirectory (destination);
                var exporter = new GarExportMedia(this);
                exporter.IgnoreErrors = export_dialog.IgnoreErrors.IsChecked ?? false;
                exporter.Export(entries, format);
                Settings.Default.appLastDestination = destination;
            }
            catch (Exception X)
            {
                PopupError (X.Message, guiStrings.TextMediaConvertError);
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool GetVolumeInformation (string rootName, string volumeName, uint volumeNameSize,
                                                 IntPtr serialNumber, IntPtr maxComponentLength, 
                                                 out uint flags, string fs, uint fs_size);

        bool IsWritableDirectory (string path)
        {
            var root = Path.GetPathRoot (path);
            if (null == root)
                return false;
            uint flags;
            if (!GetVolumeInformation (root, null, 0, IntPtr.Zero, IntPtr.Zero, out flags, null, 0))
                return false;
            return (flags & 0x00080000) == 0; // FILE_READ_ONLY_VOLUME
        }
    }

    internal class GarExportMedia : GarOperation
    {
        private IEnumerable<Entry> m_source;
        private ImageFormat     m_image_format;
        private List<Tuple<string,string>> m_failed = new List<Tuple<string,string>>();

        public bool IgnoreErrors { get; set; }
        public IEnumerable<Tuple<string,string>> FailedFiles { get { return m_failed; } }

        public GarExportMedia (MainWindow parent) 
            : base (parent, guiStrings.TextMediaConvertError)
        {
        }

        public void Export (IEnumerable<Entry> images, ImageFormat format)
        {
            m_main.StopWatchDirectoryChanges();
            m_source = images;
            m_image_format = format;
            m_progress_dialog = new ProgressDialog ()
            {
                WindowTitle = guiStrings.TextTitle,
                Text        = @"Exporting image",
                Description = "",
                MinimizeBox = true,
            };
            m_progress_dialog.DoWork += ExportWorker;
            m_progress_dialog.RunWorkerCompleted += OnExportComplete;
            m_progress_dialog.ShowDialog (m_main);
        }

        private void ExportWorker (object sender, DoWorkEventArgs e)
        {
            m_pending_error = null;
            int total = m_source.Count();
            int i = 0;
            foreach (var entry in m_source)
            {
                if (m_progress_dialog.CancellationPending)
                {
                    m_pending_error = new OperationCanceledException();
                    break;
                }
                var filename = entry.Name;
                int progress = i++ * 100/total;
                m_progress_dialog.ReportProgress (progress, string.Format (guiStrings.MsgConvertingFile,
                    Path.GetFileName (filename)), null);
                try
                {
                    if ("image" == entry.Type)
                        ExportImage(filename);
                }
                catch (SkipExistingFileException)
                {
                    continue;
                }
                catch (OperationCanceledException x)
                {
                    m_pending_error = x;
                    break;
                }
                catch (Exception x)
                {
                    if (!IgnoreErrors)
                    {
                        var error_text = string.Format (guiStrings.TextErrorConverting, entry.Name, x.Message);
                        var result = ShowErrorDialog (error_text);
                        if (!result.Continue)
                            break;
                        IgnoreErrors = result.IgnoreErrors;
                    }
                    m_failed.Add (Tuple.Create (Path.GetFileName (filename), x.Message));
                }
            }
        }

        void ExportImage (string filename)
        {
            string source_ext = Path.GetExtension (filename)?.TrimStart ('.').ToLowerInvariant();
            string target_name = Path.GetFileName (filename);
            string target_ext = m_image_format.Extensions.FirstOrDefault();
            target_name = Path.ChangeExtension (target_name, target_ext);
            var target_ctl_name = Path.ChangeExtension(target_name, "ctl");
            using (var file = BinaryStream.FromFile (filename))
            {
                var src_format = ImageFormat.FindFormat (file,imageFormat => imageFormat.CanExportAndPack);
                if (null == src_format)
                    return;
                if (src_format.Item1 == m_image_format && m_image_format.Extensions.Any (ext => ext == source_ext))
                    return;
                file.Position = 0;

                var output_ctl = CreateNewFile(target_ctl_name);
                var image = src_format.Item1.ReadAndExport(file,src_format.Item2,output_ctl);
                var output = CreateNewFile (target_name);

                try
                {
                    m_image_format.Write (output, image);
                }
                catch // delete destination file on conversion failure
                {
                    // FIXME if user chooses to overwrite file, and conversion results in error,
                    // then original file will be lost.
                    output.Dispose();
                    File.Delete (target_name ?? 
                                 throw new InvalidOperationException());
                    throw;
                }
                output.Dispose();
            }
        }

        private void OnExportComplete (object sender, RunWorkerCompletedEventArgs e)
        {
            m_main.ResumeWatchDirectoryChanges();
            m_progress_dialog.Dispose();
            if (null != m_pending_error)
            {
                if (m_pending_error is OperationCanceledException)
                    m_main.SetStatusText (m_pending_error.Message);
                else
                    m_main.PopupError (m_pending_error.Message, guiStrings.TextMediaConvertError);
            }
            m_main.Activate();
            m_main.ListViewFocus();
            m_main.RefreshView();
        }
    }


}
