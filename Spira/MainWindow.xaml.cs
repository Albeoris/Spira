using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

                IsoInfo isoInfo = new IsoInfo(dlg.FileName);
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

        private void OnCalcPS2Hash(object sender, RoutedEventArgs e)
        {
            try
            {
                new Thread(() =>
                {
                    Parallel.ForEach(Directory.GetFiles(@"W:\FFX", "*", SearchOption.AllDirectories), filePath =>
                    {
                        string name = Path.GetFileNameWithoutExtension(filePath);
                        if (name.Length > 30)
                            return;

                        using (FileStream input = File.OpenRead(filePath))
                        {
                            SHA256Managed sha = new SHA256Managed();
                            byte[] hash = sha.ComputeHash(input);
                            name += '_' + BitConverter.ToString(hash).Replace("-", String.Empty).ToLower();
                        }

                        File.Move(filePath, Path.Combine(Path.GetDirectoryName(filePath), name + Path.GetExtension(filePath)));
                    });

                    MessageBox.Show("All done!");
                }).Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void OnCalcPS3Hash(object sender, RoutedEventArgs e)
        {
            try
            {
                new Thread(() =>
                {
                    Parallel.ForEach(Directory.GetFiles(@"W:\FFX_PS3", "*", SearchOption.AllDirectories), filePath =>
                    {
                        string infoPath = filePath + ".fileinfo";
                        using (FileStream input = File.OpenRead(filePath))
                        {
                            SHA256Managed sha = new SHA256Managed();
                            byte[] hash = sha.ComputeHash(input);
                            using (FileStream output = File.Create(infoPath))
                            using (BinaryWriter bw = new BinaryWriter(output))
                            {
                                bw.Write(filePath);
                                bw.Write(input.Length);
                                bw.Write(BitConverter.ToString(hash).Replace("-", String.Empty).ToLower());
                            }
                        }
                    });

                    MessageBox.Show("All done!");
                }).Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void OnPreBuildIndices(object sender, RoutedEventArgs e)
        {
            try
            {
                ConcurrentDictionary<string, IsoTableEntryInfo> ps2Infos = new ConcurrentDictionary<string, IsoTableEntryInfo>();
                ConcurrentDictionary<string, string> invalidHash = new ConcurrentDictionary<string, string>();

                new Thread(() =>
                {
                    Parallel.ForEach(Directory.GetFiles(@"W:\FFX", "*", SearchOption.AllDirectories), filePath =>
                    {
                        IsoTableEntryInfo info = IsoTableEntryInfo.TryParse(filePath);
                        if (info == null)
                        {
                            Log.Error(filePath);
                            return;
                        }

                        if (!ps2Infos.TryAdd(info.Sha256Hash, info))
                            invalidHash.TryAdd(info.Sha256Hash, info.Sha256Hash);
                    });

                    ConcurrentDictionary<string, Tuple<string, long>> ps3Infos = new ConcurrentDictionary<string, Tuple<string, long>>();
                    Parallel.ForEach(Directory.GetFiles(@"W:\FFX_PS3", "*.fileinfo", SearchOption.AllDirectories), filePath =>
                    {
                        using (FileStream input = File.OpenRead(filePath))
                        using (BinaryReader br = new BinaryReader(input))
                        {
                            string path = br.ReadString();
                            long length = br.ReadInt64();
                            if (length == 0)
                                return;

                            string hash = br.ReadString();
                            if (!ps3Infos.TryAdd(hash, Tuple.Create(path, length)))
                                invalidHash.TryAdd(hash, hash);
                        }
                    });

                    foreach (string hash in invalidHash.Keys)
                    {
                        ps2Infos.Remove(hash);
                        ps3Infos.Remove(hash);
                    }
                    using (FileStream output = File.Create(@"W:\PS2.txt"))
                    using (BinaryWriter bw = new BinaryWriter(output))
                    {
                        foreach (IsoTableEntryInfo info in ps2Infos.Values.OrderBy(v => v.Index))
                            info.Write(bw);
                    }

                    using (FileStream output = File.Create(@"W:\PS3.txt"))
                    using (BinaryWriter bw = new BinaryWriter(output))
                    {
                        foreach (KeyValuePair<string, Tuple<string, long>> pair in ps3Infos)
                        {
                            bw.Write(pair.Value.Item1);
                            bw.Write(pair.Value.Item2);
                            bw.Write(pair.Key);
                        }
                    }

                    ConcurrentBag<IsoTableEntryInfo> bag = new ConcurrentBag<IsoTableEntryInfo>();
                    foreach (KeyValuePair<string, IsoTableEntryInfo> ps2 in ps2Infos)
                    {
                        Tuple<string, long> ps3;
                        if (!ps3Infos.TryGetValue(ps2.Key, out ps3))
                            continue;

                        if (ps2.Value.UncompressedSize != ps3.Item2)
                            continue;

                        ps2.Value.TruePath = ps3.Item1;
                        bag.Add(ps2.Value);
                    }

                    using (FileStream output = File.Create(@"W:\PS23.txt"))
                    using (BinaryWriter bw = new BinaryWriter(output))
                    {
                        foreach (IsoTableEntryInfo info in bag)
                        {
                            bw.Write(info.Index);
                            bw.Write(info.DefectiveIndex);
                            bw.Write(info.TruePath.Substring(28));
                        }
                    }
                    MessageBox.Show("Alldone");
                }).Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}