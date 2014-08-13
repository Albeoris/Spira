using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Spira.ISO;
using Spira.Core;
using Spira.Text;
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

        private async void OnClick1(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = (Button)sender;
                int i2 = 0;
                IsoFileInfo isoInfo = new IsoFileInfo(@"S:\Downloads\final_fantasy_x_international.iso");
                using (IsoFileCommander fileCommander = new IsoFileCommander(isoInfo))
                {
                    List<IsoTableEntry> entries = fileCommander.GetEntries();
                    for (int i = 0; i < entries.Count - 1; i++)
                    {
                        IsoTableEntry entry = entries[i];
                        //if ((entry.Flags & IsoTableEntryFlags.Dummy) == IsoTableEntryFlags.Dummy)
                        //    continue;

                        string ext = "";
                        if ((entry.Flags & IsoTableEntryFlags.Compressed) == IsoTableEntryFlags.Compressed)
                            ext += ".cmp";
                        if ((entry.Flags & IsoTableEntryFlags.Dummy) == IsoTableEntryFlags.Dummy)
                            ext += ".dmp";
                        if (string.IsNullOrEmpty(ext))
                            ext = ".raw";

                        long fileSize = (entries[i + 1].Sector - entry.Sector) * isoInfo.SectorSize - entry.Left;
                        if (fileSize < 1)
                            continue;

                        string fileName = String.Format("File{0:D5}_{1:D4}", i, i2++) + ext;


                        button.Content = fileName;
                        //if (i2 == 1336)
                        //{
                            using (Stream input = fileCommander.OpenStream(entry.Sector * isoInfo.SectorSize, fileSize))
                            using (Stream output = File.Create(@"D:\Temp\FFX\" + fileName))
                            {
                                if ((entry.Flags & IsoTableEntryFlags.Compressed) == IsoTableEntryFlags.Compressed)
                                {
                                    BinaryReader br = input.GetBinaryReader();
                                    byte compressed = br.ReadByte();
                                    int uncompressedSize = br.ReadInt32();
                             
                                    LZSStream decompressStream = new LZSStream(input, output);
                                    decompressStream.Decompress(uncompressedSize);
                                }
                                else
                                    await input.CopyToAsync(output);
                            }
                        //}
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private async void OnClick2(object sender, RoutedEventArgs e)
        {
            try
            {
                Directory.CreateDirectory(@"D:\Temp\FFX\Unknown1");
                Directory.CreateDirectory(@"D:\Temp\FFX\Unknown2");
                Directory.CreateDirectory(@"D:\Temp\FFX\Unknown3");
                Directory.CreateDirectory(@"D:\Temp\FFX\Unknown4");
                Directory.CreateDirectory(@"D:\Temp\FFX\Unknown5");
                Directory.CreateDirectory(@"D:\Temp\FFX\Unknown6");
                Directory.CreateDirectory(@"D:\Temp\FFX\Unknown7");
                Directory.CreateDirectory(@"D:\Temp\FFX\Unknown8");
                Directory.CreateDirectory(@"D:\Temp\FFX\Unknown9");
                Directory.CreateDirectory(@"D:\Temp\FFX\Unknown10");
                Directory.CreateDirectory(@"D:\Temp\FFX\Unknown11");
                Directory.CreateDirectory(@"D:\Temp\FFX\Unknown12");
                Directory.CreateDirectory(@"D:\Temp\FFX\Unknown13");
                Directory.CreateDirectory(@"D:\Temp\FFX\Empty");
                Directory.CreateDirectory(@"D:\Temp\FFX\Small");
                Directory.CreateDirectory(@"D:\Temp\FFX\Map1");
                Directory.CreateDirectory(@"D:\Temp\FFX\NotMap1");
                Directory.CreateDirectory(@"D:\Temp\FFX\NotBGM");
                Directory.CreateDirectory(@"D:\Temp\FFX\Scenes");
                Directory.CreateDirectory(@"D:\Temp\FFX\Scenes2");
                Directory.CreateDirectory(@"D:\Temp\FFX\FTCX");
                Directory.CreateDirectory(@"D:\Temp\FFX\_FTCX");
                Directory.CreateDirectory(@"D:\Temp\FFX\EV01");
                Directory.CreateDirectory(@"D:\Temp\FFX\_EV01");
                Directory.CreateDirectory(@"D:\Temp\FFX\8ES");

                Parallel.ForEach(Directory.EnumerateFiles(@"D:\Temp\FFX\"), (file) =>
                {
                    int size = 0;
                    byte[] header = new byte[0x40];

                    using (Stream input = File.OpenRead(file))
                        size = input.Read(header, 0, 0x40);

                    TryWriteTag(size, header, file);

                    bool b = TryMoveToEmpty(size, header, file)
                        ||TryMoveToSmall(size, header, file)
                            || TryMoveMap1(size, header, file)
                            || TryMoveNotMap1(size, header, file)
                            || TryMoveNotBGM(size, header, file)
                            || TryMoveScene(size, header, file)
                            || TryMoveScene2(size, header, file)
                            || TryMoveFTCX(size, header, file)
                            || TryMove_FTCX(size, header, file)
                            || TryMoveEV01(size, header, file)
                            || TryMove_EV01(size, header, file)
                            || TryMove8ES(size, header, file)
                             || TryMoveToUnknown1(size, header, file)
                             || TryMoveToUnknown2(size, header, file)
                             || TryMoveToUnknown3(size, header, file)
                             || TryMoveToUnknown4(size, header, file)
                             || TryMoveToUnknown5(size, header, file)
                             || TryMoveUnknown6(size, header, file)
                             || TryMoveUnknown7(size, header, file)
                             ||TryMoveToUnknown8(size, header, file)
                             || TryMoveToUnknown9(size, header, file)
                             || TryMoveToUnknown10(size, header, file)
                             || TryMoveToUnknown11(size, header, file)
                             || TryMoveToUnknown12(size, header, file)
                             || TryMoveToUnknown13(size, header, file);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private async void OnClick3(object sender, RoutedEventArgs e)
        {
            try
            {
                new Thread(() =>
                {
                    int count1 = 0;
                    using (StreamWriter sw = File.CreateText(@"D:\Temp\log.txt"))
                        Parallel.ForEach(Directory.EnumerateFiles(@"D:\Temp\FFX", "*", SearchOption.AllDirectories), (file) =>
                        {
                            ((Button)sender).Dispatcher.Invoke(() => ((Button)sender).Content = count1++);

                            ConcurrentDictionary<long, List<byte>> dic = new ConcurrentDictionary<long, List<byte>>();
                            List<long> keys = new List<long>();

                            long fileSize = new FileInfo(file).Length;
                            if (fileSize < 100 || fileSize > 20 * 1024 * 1024)
                                return;

                            int count = 0;
                            int length = 0;
                            using (Stream input = File.OpenRead(file))
                            {
                                while (!input.IsEndOfStream())
                                {
                                    int b = input.ReadByte();

                                    if (keys.Count > 0)
                                    {
                                        for (int i = keys.Count - 1; i >= 0; i--)
                                        {
                                            List<byte> list = dic[keys[i]];
                                            if (TryAdd(list, (byte)b))
                                                list.Add((byte)b);
                                            else
                                            {
                                                dic.Remove(keys[i]);
                                                keys.RemoveAt(i);

                                                if (list.Count > 13)
                                                {
                                                    length += list.Count;
                                                    count++;
                                                }

                                                if (count > 5)
                                                    break;
                                            }
                                        }
                                    }

                                    if (b != 0x13)
                                        continue;

                                    keys.Add(input.Position);
                                    dic.TryAdd(input.Position, new List<byte>() {0x13});
                                }
                            }

                            if (count > 3)
                                lock (sw)
                                {
                                    sw.WriteLine("{0}: {1} - {2}", count, length, file);
                                    sw.Flush();
                                }
                        }
                            );
                }).Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private bool TryAdd(List<byte> list, byte b)
        {
            if (list.Count == 1)
                return (b >= 0x30 && b <= 0x37);

            if (list.Count == 2)
                return b == 0x03;

            return b >= 0x3A && b <= 0x89;
        }

        private static readonly Encoding _enc = Encoding.GetEncoding(1251);

        private void TryWriteTag(int size, byte[] header, string file)
        {
            if (size < 10)
                return;

            int i = 0;
            char[] chars = _enc.GetChars(header, 6, 4);
            for (i = 0; i < chars.Length; i++)
            {
                if (chars[i] < 'A' || chars[i] > 'Z')
                    break;
            }

            if (i >= 1)
                Console.WriteLine("{0} - {1}", new string(chars, 0, Math.Min(i, chars.Length)), file);
        }

        private bool TryMoveToEmpty(int size, byte[] header, string file)
        {
            if (size == 0)
            {
                File.Move(file, Path.Combine(@"D:\Temp\FFX\Empty", Path.GetFileName(file)));
                return true;
            }
            return false;
        }

        private bool TryMoveToSmall(int size, byte[] header, string file)
        {
            if (size < 0x40)
            {
                File.Move(file, Path.Combine(@"D:\Temp\FFX\Small", Path.GetFileName(file)));
                return true;
            }
            return false;
        }

        private bool TryMoveScene(int size, byte[] header, string file)
        {
            if (size < 0x40)
                return false;

            if (header[0] == 0x8 && header[4] == 0x30)
            {
                for (int i = 1; i < 3; i++)
                {
                    if (header[i] != header[i + 4] || header[i] != 0)
                        return false;
                }
                File.Move(file, Path.Combine(@"D:\Temp\FFX\Scenes", Path.GetFileName(file) + ".scene"));
                return true;
            }
            return false;
        }

        private bool TryMoveScene2(int size, byte[] header, string file)
        {
            if (size < 0x40)
                return false;

            if (header[0] == 0x3 && header[4] == 0x10)
            {
                for (int i = 1; i < 3; i++)
                {
                    if (header[i] != header[i + 4] || header[i] != 0)
                        return false;
                }
                File.Move(file, Path.Combine(@"D:\Temp\FFX\Scenes2", Path.GetFileName(file) + ".scene2"));
                return true;
            }
            return false;
        }

        private bool TryMoveMap1(int size, byte[] header, string file)
        {
            if (size < 4)
                return false;

            if (header[0] == 0x4D && header[1] == 0x41 && header[2] == 0x50 && header[3] == 0x31)
            {
                File.Move(file, Path.Combine(@"D:\Temp\FFX\Map1", Path.GetFileName(file) + ".map1"));
                return true;
            }
            return false;
        }

        private bool TryMoveNotMap1(int size, byte[] header, string file)
        {
            if (size < 10)
                return false;

            if (header[6] == 0x4D && header[7] == 0x41 && header[8] == 0x50 && header[9] == 0x31)
            {
                File.Move(file, Path.Combine(@"D:\Temp\FFX\NotMap1", Path.GetFileName(file) + ".notmap1"));
                return true;
            }
            return false;
        }

        private bool TryMoveFTCX(int size, byte[] header, string file)
        {
            if (size < 10)
                return false;

            if (header[6] == 0x46 && header[7] == 0x54 && header[8] == 0x43 && header[9] == 0x58)
            {
                File.Move(file, Path.Combine(@"D:\Temp\FFX\FTCX", Path.GetFileName(file) + ".ftcx"));
                return true;
            }
            return false;
        }

        private bool TryMove_FTCX(int size, byte[] header, string file)
        {
            if (size < 10)
                return false;

            if (header[0] == 0x46 && header[1] == 0x54 && header[2] == 0x43 && header[3] == 0x58)
            {
                File.Move(file, Path.Combine(@"D:\Temp\FFX\_FTCX", Path.GetFileName(file) + "._ftcx"));
                return true;
            }
            return false;
        }

        private bool TryMoveEV01(int size, byte[] header, string file)
        {
            if (size < 10)
                return false;

            if (header[6] == 0x45 && header[7] == 0x56 && header[8] == 0x30 && header[9] == 0x31)
            {
                File.Move(file, Path.Combine(@"D:\Temp\FFX\EV01", Path.GetFileName(file) + ".ev01"));
                return true;
            }
            return false;
        }
        
        private bool TryMove_EV01(int size, byte[] header, string file)
        {
            if (size < 10)
                return false;

            if (header[0] == 0x45 && header[1] == 0x56 && header[2] == 0x30 && header[3] == 0x31)
            {
                File.Move(file, Path.Combine(@"D:\Temp\FFX\_EV01", Path.GetFileName(file) + "._ev01"));
                return true;
            }
            return false;
        }

        private bool TryMove8ES(int size, byte[] header, string file)
        {
            if (size < 0x40)
                return false;

            if (_enc.GetString(header).Contains("8ES"))
            {
                File.Move(file, Path.Combine(@"D:\Temp\FFX\8ES", Path.GetFileName(file) + ".8es"));
                return true;
            }
            return false;
        }

        private bool TryMoveNotBGM(int size, byte[] header, string file)
        {
            if (size < 10)
                return false;

            if (header[6] == 0x42 && header[7] == 0x47 && header[8] == 0x4D)
            {
                File.Move(file, Path.Combine(@"D:\Temp\FFX\NotBGM", Path.GetFileName(file) + ".notbgm"));
                return true;
            }
            return false;
        }

        private bool TryMoveUnknown6(int size, byte[] header, string file)
        {
            if (size < 10)
                return false;

            if (header[8] == 0xB0 && header[9] == 0x01)
                {
                File.Move(file, Path.Combine(@"D:\Temp\FFX\Unknown6", Path.GetFileName(file) + ".unk6"));
                return true;
            }
            return false;
        }

        private bool TryMoveUnknown7(int size, byte[] header, string file)
        {
            if (size < 16)
                return false;

            if (header[9] == 0xC0 && header[10] == 0x07)
                {
                File.Move(file, Path.Combine(@"D:\Temp\FFX\Unknown7", Path.GetFileName(file) + ".unk7"));
                return true;
            }
            return false;
        }

        private bool TryMoveToUnknown1(int size, byte[] header, string file)
        {
            if (size < 16)
                return false;

            if (header[14] == 0 && header[13] == 0 && header[12] == 0x1A && header[11] == 0x28 && header[10] == 0x03)
            {
                File.Move(file, Path.Combine(@"D:\Temp\FFX\Unknown1", Path.GetFileName(file)));
                return true;
            }
            return false;
        }

        private bool TryMoveToUnknown2(int size, byte[] header, string file)
        {
            if (size < 16)
                return false;

            int index = 0;
            if (header[index++] == 0x56 && header[index++] == 0x53)
            {
                while (++index < 12)
                    if (header[index] != 0)
                        return false;

                File.Move(file, Path.Combine(@"D:\Temp\FFX\Unknown2", Path.GetFileName(file)));
                return true;
            }

            return false;
        }

        private bool TryMoveToUnknown3(int size, byte[] header, string file)
        {
            if (size < 32)
                return false;

            for (int i = 0; i < 4; i++)
                if (header[i] != 0)
                    return false;

            for (int i = 16; i < 32; i++)
                if (header[i] != 0xFF)
                    return false;

            File.Move(file, Path.Combine(@"D:\Temp\FFX\Unknown3", Path.GetFileName(file)));
            return true;
        }

        private bool TryMoveToUnknown4(int size, byte[] header, string file)
        {
            if (size < 0x40)
                return false;

            int count = 0;
            for (int i = 2; i < 0x40; i += 2)
            {
                if (header[i] == 0xB0 && header[i + 1] == 0x01)
                    count++;
            }

            if (count < 8)
                return false;

            File.Move(file, Path.Combine(@"D:\Temp\FFX\Unknown4", Path.GetFileName(file)));
            return true;
        }

        private bool TryMoveToUnknown12(int size, byte[] header, string file)
        {
            if (size < 0x40)
                return false;

            if (header[5] == 0x01 && header[6] == 0x01 && header[7] == 0xb0 && header[8] == 0x09)
            {
                File.Move(file, Path.Combine(@"D:\Temp\FFX\Unknown12", Path.GetFileName(file)));
                return true;
            }
            return false;
        }

        private bool TryMoveToUnknown13(int size, byte[] header, string file)
        {
            if (size < 0x40)
                return false;

            return false;
        }

        private bool TryMoveToUnknown9(int size, byte[] header, string file)
        {
            if (size < 0x40)
                return false;

            if (header[6] == 0x00 && header[7] == 0x01 && header[8] == 0x0B && header[9] == 0x80 && header[10] == 0x03)
            {
                File.Move(file, Path.Combine(@"D:\Temp\FFX\Unknown9", Path.GetFileName(file)));
                return true;
            }

            return false;
        }

        private bool TryMoveToUnknown10(int size, byte[] header, string file)
        {
            if (size < 0x40)
                return false;

            if (header[6] == 0x00 && header[7] == 0x01 && header[8] == 0x01 && header[9] == 0xA0)
            {
                File.Move(file, Path.Combine(@"D:\Temp\FFX\Unknown10", Path.GetFileName(file)));
                return true;
            }

            return false;
        }

        private bool TryMoveToUnknown11(int size, byte[] header, string file)
        {
            if (size < 0x40)
                return false;

            if (header[4] != 1 || header[0xc] == 0)
                return false;

            for (int i = 0; i < 4; i++)
                if (header[i] != 0)
                    return false;

            for (int i = 5; i < 0xC; i++)
                if (header[i] != 0)
                    return false;

            File.Move(file, Path.Combine(@"D:\Temp\FFX\Unknown11", Path.GetFileName(file)));
            return true;
        }

        private bool TryMoveToUnknown5(int size, byte[] header, string file)
        {
            if (size < 0x40)
                return false;
            
            int i = 0;
            for (; i < 0x40; i++)
            {
                if (header[i] != 0)
                {
                    if (header[i] != 0x0B)
                        return false;
                    break;
                }
            }

            if (header[++i] != 0)
                return false;

            for (; i < 0x40; i++)
            {
                if (header[i] != 0)
                {
                    if (header[i] != 0x02)
                        return false;
                    
                    for (int n = 1; n < 5; n++)
                        if (header[i + n] != 0)
                            return false;

                    break;
                }
            }

            File.Move(file, Path.Combine(@"D:\Temp\FFX\Unknown5", Path.GetFileName(file)));
            return true;
        }

        private bool TryMoveToUnknown8(int size, byte[] header, string file)
        {
            if (size < 0x40)
                return false;

            byte b = 0;
            int i = 0;
            for (; i < 0x40; i++)
            {
                if (header[i] != 0)
                {
                    b = header[i];
                    break;
                }
            }
            
            i++;
            
            for (int n = 0; n < 4; n++, i++)
            if (header[i] != 0)
                return false;

            for (; i < 0x40; i++)
            {
                if (header[i] != 0)
                {
                    if (header[i] != b)
                        return false;
                    
                    for (int n = 1; n < 3; n++)
                        if (header[i + n] != 0)
                            return false;

                    break;
                }
            }

            File.Move(file, Path.Combine(@"D:\Temp\FFX\Unknown8", Path.GetFileName(file)));
            return true;
        }

        private void OnClick4(object sender, RoutedEventArgs e)
        {
            try
            {
                using (FileStream input = File.OpenRead(@"D:\Temp\FFX\_EV01\File00850_0342.cmp._ev01"))
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
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
