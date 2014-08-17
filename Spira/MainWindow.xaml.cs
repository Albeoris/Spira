using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Spira.Battle;
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
                    
                    isoInfo.FillKnownFileInformation(infos);
                    
                    UiProgressWindow.Execute("Чтение дополнительной информации", fileCommander, () => fileCommander.ReadAdditionalEntriesInfo(infos));
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

                fileCommander.ExtractFile(info, Path.Combine(@"W:\FFX", info.GetRelativePath()));
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

        private void OnRenameClick(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog {Filter = "Scenes (*.scn, *.bin)|*.scn;*.bin", Multiselect = true};
                if (dlg.ShowDialog() != true)
                    return;

                ConcurrentDictionary<int, Tuple<int, string>> dic2 = new ConcurrentDictionary<int, Tuple<int, string>>();
                
                Parallel.ForEach(dlg.FileNames, filePath =>
                {
                    using (FileStream input = File.OpenRead(filePath))
                    {
                        BattleBinFileReader er = new BattleBinFileReader(input);
                        er.ReadHeader();
                        er.ReadDescriptorHeader();
                        
                        IsoTableEntryInfo info = IsoTableEntryInfo.TryParse(filePath);
                        string fileName = er.ReadFileName();
                        dic2.TryAdd(info.Index, Tuple.Create(info.DefectiveIndex, fileName));

                        Log.Message(dic2[info.Index].Item2);
                    }
                });

                using (FileStream fs = File.Create(@"W:\\Indices.bin"))
                {
                    using (Stream input = File.OpenRead(@"C:\Git\C#\Spira\Spira.ISO\SLPS_250.88\AdditionalFileInformation.bin"))
                        input.CopyTo(fs);
                    
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        foreach (KeyValuePair<int, Tuple<int, string>> pair in dic2)
                        {
                            bw.Write(pair.Key);
                            bw.Write(pair.Value.Item1);
                            bw.Write((int)(IsoAdditionalInfo.PS2KnownName | IsoAdditionalInfo.PS2KnownSignature));
                            bw.Write((int)FFXFileSignatures.Scn);
                            bw.Write(pair.Value.Item2);
                        }
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