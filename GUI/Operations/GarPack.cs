using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        void PackMediaExec (object sender, ExecutedRoutedEventArgs e)
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
            var packMedia = new PackMedia();
            var destination = ViewModel.Path.First();

            if (ViewModel.IsArchive)
                destination = Path.GetDirectoryName (destination);
            if (!IsWritableDirectory (destination) && Directory.Exists (Settings.Default.appLastDestination))
                destination = Settings.Default.appLastDestination;

            packMedia.DestinationDir.Text = destination;
            packMedia.Owner = this;

            var result = packMedia.ShowDialog() ?? false;
            if (!result)
                return;
            if (!(packMedia.ImagePackFormat.SelectedItem is ImageFormat format))
            {
                Trace.WriteLine ("Format is not selected", "ConvertMediaExec");
                return;
            }
            try
            {
                destination = packMedia.DestinationDir.Text;
                Directory.SetCurrentDirectory (destination);
                var packer = new GarPackMedia(this)
                {
                    IgnoreErrors = packMedia.IgnoreErrors.IsChecked ?? false
                };
                packer.Pack(entries, format);
                Settings.Default.appLastDestination = destination;
            }
            catch (Exception x)
            {
                PopupError (x.Message, guiStrings.TextMediaConvertError);
            }
        }
    }

    internal class GarPackMedia : GarOperation
    {
        private IEnumerable<Entry> m_source;
        private ImageFormat     m_image_format;
        private List<Tuple<string,string>> m_failed = new List<Tuple<string,string>>();

        public bool IgnoreErrors { get; set; }
        public IEnumerable<Tuple<string,string>> FailedFiles => m_failed;

        public GarPackMedia(MainWindow parent) 
            : base(parent, guiStrings.TextMediaConvertError)
        {

        }

        public void Pack (IEnumerable<Entry> images, ImageFormat format)
        {
            m_main.StopWatchDirectoryChanges();
            m_source = images;
            m_image_format = format;
            m_progress_dialog = new ProgressDialog ()
            {
                WindowTitle = guiStrings.TextTitle,
                Text        = @"Packing image",
                Description = "",
                MinimizeBox = true,
            };
            m_progress_dialog.DoWork += PackWorker;
            m_progress_dialog.RunWorkerCompleted += OnPackComplete;
            m_progress_dialog.ShowDialog (m_main);
        }

        private void PackWorker (object sender, DoWorkEventArgs e)
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
                var progress = i++ * 100/total;
                m_progress_dialog.ReportProgress (progress, 
                    string.Format (guiStrings.MsgConvertingFile,
                    Path.GetFileName (filename)), null);
                try
                {
                    if (entry.Type == "image") PackImage(filename);
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

        private void PackImage (string filename)
        {
            string source_ext = Path.GetExtension (filename)?.TrimStart ('.').ToLowerInvariant();
            string target_name = Path.GetFileName (filename);
            string target_ext = m_image_format.Extensions.FirstOrDefault();
            target_name = Path.ChangeExtension (target_name, target_ext);

            using (var file = BinaryStream.FromFile (filename))
            {
                var src_format = ImageFormat.FindFormat (file);
                if (null == src_format)
                    return;
                if (src_format.Item1 == m_image_format && m_image_format.Extensions.Any (ext => ext == source_ext))
                    return;
                file.Position = 0;
                var image = src_format.Item1.Read (file, src_format.Item2);
                var output = CreateNewFile (target_name);
                try
                {
                    if (m_image_format.NeedInputToPack)
                    {
                        var inputFileName = Path.ChangeExtension(filename, "ctl");

                        if (!File.Exists(inputFileName))
                        {
                            return;
                        }

                        using (var inputFile = BinaryStream.FromFile(inputFileName))
                        {
                            m_image_format.Pack(output,inputFile,image);
                        }
                    }
                    else
                    {
                        m_image_format.Pack(output, null, image);
                    }

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

        private void OnPackComplete (object sender, RunWorkerCompletedEventArgs e)
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
