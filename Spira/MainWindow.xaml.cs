using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
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
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnExtractFilesClick(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog {Filter = "FFX Image (*.iso)|*.iso"};
                if (dlg.ShowDialog() != true)
                    return;

                IsoInfo isoInfo = new IsoInfo(dlg.FileName);
                using (IsoFileCommander fileCommander = new IsoFileCommander(isoInfo))
                {
                    List<IsoTableEntry> entries = UiProgressWindow.Execute("Поиск файлов", fileCommander, () => fileCommander.GetEntries());
                    List<IsoTableEntryInfo> infos = UiProgressWindow.Execute("Анализ файлов", fileCommander, () => fileCommander.GetEntriesInfo(entries));
                    UiProgressWindow.Execute("Чтение дополнительной информации", fileCommander, () => fileCommander.ReadAdditionalEntriesInfo(infos));

                    Dictionary<int, string> knownFilePathes = isoInfo.GetKnownFilePathes();
                    foreach (string signature in EnumCache<FFXFileSignatures>.Names) Directory.CreateDirectory(@"W:\FFX\" + signature.ToUpperInvariant());
                    foreach (string path in knownFilePathes.Values) Directory.CreateDirectory(@"W:\FFX\" + Path.GetDirectoryName(path));

                    Directory.CreateDirectory(@"W:\FFX\Unknown");

                    foreach (IsoTableEntryInfo info in infos)
                        knownFilePathes.TryGetValue(info.Index, out info.RelativePath);

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
                if (info.CompressedSize < 1)
                    return;

                if (string.IsNullOrEmpty(info.RelativePath))
                {
                    string folder = EnumCache<FFXFileSignatures>.IsDefined(info.Signature) ? info.Signature.ToString().ToUpperInvariant() : "Unknown";
                    info.RelativePath = Path.Combine(folder, info.GetFileName());
                }

                fileCommander.ExtractFile(info, Path.Combine(@"W:\FFX", info.RelativePath));
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
                OpenFileDialog dlg = new OpenFileDialog {Filter = "Events (*.ev, *.ebp)|*.ev;*.ebp", Multiselect = true};
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