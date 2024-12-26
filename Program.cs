using System;
using System.Drawing;
using System.IO;
using NAudio.Wave;

namespace EZ
{
    public static class AssetManager
    {
        public const string BitmapAsset_Template_ResourceName = "EZAssetManager.BitmapAsset_Template.h";
        public const string AudioAsset_Template_ResourceName = "EZAssetManager.AudioAsset_Template.h";
        [STAThread]
        public static void Main(string[] args)
        {
            File.WriteAllText("D:\\ImportantData\\Coding\\EpsilonTheatrics\\PowerCues\\PowerDMX\\ETC_WinUsb_INF.txt", BytesToHex(File.ReadAllBytes("D:\\ImportantData\\Coding\\EpsilonTheatrics\\PowerCues\\PowerDMX\\ETC_WinUsb.inf")));
            File.WriteAllText("D:\\ImportantData\\Coding\\EpsilonTheatrics\\PowerCues\\PowerDMX\\ETC_WinUsb_CAT.txt", BytesToHex(File.ReadAllBytes("D:\\ImportantData\\Coding\\EpsilonTheatrics\\PowerCues\\PowerDMX\\ETC_WinUsb.cat")));
        }
        public static void ImportAudio(string inputFilePath, string outputFilePath)
        {
            string name = Path.GetFileNameWithoutExtension(inputFilePath);
            string path = inputFilePath;

            AudioFileReader audioFileReader = new AudioFileReader(inputFilePath);

            WaveFormat waveFormat = WaveFormat.CreateCustomFormat(WaveFormatEncoding.Pcm, 44100, 2, 44100 * 2 * (16 / 8), 2 * (16 / 8), 16);
            MediaFoundationResampler resampler = new MediaFoundationResampler(audioFileReader, waveFormat);
            resampler.ResamplerQuality = 60;

            MemoryStream memoryStream = new MemoryStream();

            int bytesRead;
            byte[] bufferBytes = new byte[waveFormat.AverageBytesPerSecond];
            while ((bytesRead = resampler.Read(bufferBytes, 0, bufferBytes.Length)) > 0)
            {
                memoryStream.Write(bufferBytes, 0, bytesRead);
            }

            string length = memoryStream.Length.ToString();
            string buffer = BytesToHex(memoryStream.ToArray());

            resampler.Dispose();
            audioFileReader.Dispose();

            string output = LoadTemplate(AudioAsset_Template_ResourceName);

            output = output.Replace("#NAME#", name);
            output = output.Replace("#PATH#", path);
            output = output.Replace("#LENGTH#", length);
            output = output.Replace("#BUFFER#", buffer);

            File.WriteAllText(outputFilePath, output);
        }
        public static void ImportBitmap(string inputFilePath, string outputFilePath)
        {
            string name = Path.GetFileNameWithoutExtension(inputFilePath);
            string path = inputFilePath;

            Bitmap bitmap = new Bitmap(inputFilePath);
            string width = bitmap.Width.ToString();
            string height = bitmap.Height.ToString();

            byte[] bufferBytes = new byte[bitmap.Width * bitmap.Height * 4];
            int offset = 0;
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    bufferBytes[offset] = pixel.B; offset++;
                    bufferBytes[offset] = pixel.G; offset++;
                    bufferBytes[offset] = pixel.R; offset++;
                    bufferBytes[offset] = pixel.A; offset++;
                }
            }

            string buffer = BytesToHex(bufferBytes);

            bitmap.Dispose();

            string output = LoadTemplate(BitmapAsset_Template_ResourceName);

            output = output.Replace("#NAME#", name);
            output = output.Replace("#PATH#", path);
            output = output.Replace("#WIDTH#", width);
            output = output.Replace("#HEIGHT#", height);
            output = output.Replace("#BUFFER#", buffer);

            File.WriteAllText(outputFilePath, output);
        }
        public static string LoadTemplate(string embeddedResourceName)
        {
            Stream stream = typeof(EZ.AssetManager).Assembly.GetManifestResourceStream(embeddedResourceName);
            StreamReader reader = new StreamReader(stream);

            string output = reader.ReadToEnd();

            reader.Dispose();
            stream.Dispose();

            return output;
        }
        public static string BytesToHex(byte[] data)
        {
            char[] charset = new char[16] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

            char[] output = new char["{ ".Length + (data.Length * "0x00".Length) + ((data.Length - 1) * ", ".Length) + " }".Length];

            int index = 0;

            output[index] = '{'; index++;
            output[index] = ' '; index++;

            output[index] = '0'; index++;
            output[index] = 'x'; index++;
            output[index] = charset[data[0] >> 4]; index++;
            output[index] = charset[data[0] & 0x0F]; index++;

            for (int i = 1; i < data.Length; i++)
            {
                output[index] = ','; index++;
                output[index] = ' '; index++;

                output[index] = '0'; index++;
                output[index] = 'x'; index++;
                output[index] = charset[data[i] >> 4]; index++;
                output[index] = charset[data[i] & 0x0F]; index++;
            }

            output[index] = ' '; index++;
            output[index] = '}';

            return new string(output);
        }
    }
}