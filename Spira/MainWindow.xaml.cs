using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Spira.Images;
using Spira.ISO;
using Spira.Core;
using Spira.Text;
using Spira.UI;
using Path = System.IO.Path;

namespace Spira
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnExtractFilesClick(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog {Filter = "*.iso"};
                if (dlg.ShowDialog() != true)
                    return;

                IsoFileInfo isoInfo = new IsoFileInfo(dlg.FileName);
                using (IsoFileCommander fileCommander = new IsoFileCommander(isoInfo))
                {
                    List<IsoTableEntry> entries = UiProgressWindow.Execute("Поиск файлов", fileCommander, () => fileCommander.GetEntries());
                    List<IsoTableEntryInfo> infos = UiProgressWindow.Execute("Анализ файлов", fileCommander, () => fileCommander.GetEntriesInfo(entries));
                    UiProgressWindow.Execute("Чтение дополнительной информации", fileCommander, () => fileCommander.ReadAdditionalEntriesInfo(infos));

                    foreach (string signature in EnumCache<FFXFileSignatures>.Names)
                        Directory.CreateDirectory(@"D:\Temp\FFX\" + signature.ToUpperInvariant());

                    Directory.CreateDirectory(@"D:\Temp\FFX\Unknown");
                    Directory.CreateDirectory(@"D:\Temp\FFX\Unknown\1B0");

                    UiProgressWindow.Execute("Распаковка файлов", (total, incr) => ExtractFiles(fileCommander, infos, total, incr));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ExtractFiles(IsoFileCommander fileCommander, List<IsoTableEntryInfo> infos, Action<long> total, Action<long> incr)
        {
            total.NullSafeInvoke(infos.Count);
            Parallel.ForEach(infos, info => SafeExtractInfo(fileCommander, info, incr));
        }

        private void SafeExtractInfo(IsoFileCommander fileCommander, IsoTableEntryInfo info, Action<long> progressIncr)
        {
            try
            {
                if (info.CompressedSize < 1 || !EnumCache<FFXFileSignatures>.IsDefined(info.Signature))
                    return;

                string outputPath = Path.Combine(@"D:\Temp\FFX", info.Signature.ToString().ToUpperInvariant(), info.GetFileName());
                fileCommander.ExtractFile(info, outputPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to extract file '{0}'", info.GetFileName());
            }
            finally
            {
                progressIncr(1);
            }
        }

        private void OnConvertEvClick(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog {Filter = "*.ev", Multiselect = true};
                if (dlg.ShowDialog() != true)
                    return;

                foreach (string filePath in dlg.FileNames)
                {
                    using (FileStream input = File.OpenRead(filePath))
                    {
                        input.Position = 0x123F8;
                        FFXTextEncoding encoding = new FFXTextEncoding(FFXTextEncodingCodepage.Create());
                        using (StreamReader sr = new StreamReader(input, encoding))
                        {
                            string str = sr.ReadToEnd();
                            Console.WriteLine(str);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void OnConvertFtcxClick(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog {Filter = "FFX Picture (*.ftcx)|*.ftcx", Multiselect = true};
                if (dlg.ShowDialog() != true)
                    return;

                foreach (string filePath in dlg.FileNames)
                {
                    using (FileStream input = File.OpenRead(filePath))
                    {
                        FtcxFileReader fileReader = new FtcxFileReader(input);
                        FtcxFileHeader header = fileReader.ReadHeader();
                        int width = header.BlockSize * 2;
                        short height = header.BlockCount;
                        fileReader.SkipUnknownSubHeader();
                        using (SafeHGlobalHandle image = fileReader.ReadImage())
                        using (UnmanagedMemoryStream imageInput = image.OpenStream(FileAccess.Read))
                        using (FileStream output = File.Create(Path.ChangeExtension(filePath, String.Format(".{0}x{1}.raw", width, height))))
                            imageInput.CopyTo(output);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}