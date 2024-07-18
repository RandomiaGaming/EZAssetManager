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
            ImportAudio("D:\\Coding\\C++\\MysteryMemeware2\\Song.wav", "D:\\Coding\\C++\\MysteryMemeware2\\Code\\MysterySong.h");
        }
        public static void ImportAudio(string audioClipFilePath, string outputHeaderFilePath)
        {
            AudioFileReader audioFileReader = new AudioFileReader(audioClipFilePath);
            byte[] bufferBytes = new byte[audioFileReader.Length];
            audioFileReader.Read(bufferBytes, 0, bufferBytes.Length);

            string name = Path.GetFileNameWithoutExtension(audioClipFilePath);
            string path = audioClipFilePath;
            string optionalHeaders = "";
            switch ((AudioAsset_FormatTags)audioFileReader.WaveFormat.Encoding)
            {
                case AudioAsset_FormatTags.MPEG_RAW_AAC:
                case AudioAsset_FormatTags.WAVE_FORMAT_MPEG_LOAS:
                case AudioAsset_FormatTags.NOKIA_MPEG_ADTS_AAC:
                case AudioAsset_FormatTags.NOKIA_MPEG_RAW_AAC:
                case AudioAsset_FormatTags.VODAFONE_MPEG_ADTS_AAC:
                case AudioAsset_FormatTags.VODAFONE_MPEG_RAW_AAC:
                    optionalHeaders = "#include <mwCodec.h>\r\n";
                    break;
                case AudioAsset_FormatTags.MPEG_HEAAC:
                    throw new Exception("Audio assets in the MPEG HEAAC format are not supported (yet).");
            }
            string formatTag = Enum.GetName(typeof(AudioAsset_FormatTags), (AudioAsset_FormatTags)audioFileReader.WaveFormat.Encoding);
            string channelCount = audioFileReader.WaveFormat.Channels.ToString();
            string sampleRate = audioFileReader.WaveFormat.SampleRate.ToString();
            string averageBytesPerSecond = audioFileReader.WaveFormat.AverageBytesPerSecond.ToString();
            string blockAlign = audioFileReader.WaveFormat.BlockAlign.ToString();
            string bitsPerSample = audioFileReader.WaveFormat.BitsPerSample.ToString();
            string extraSize = audioFileReader.WaveFormat.ExtraSize.ToString();
            string buffer = BytesToHex(bufferBytes);

            audioFileReader.Dispose();

            Stream stream = typeof(EZ.AssetManager).Assembly.GetManifestResourceStream(AudioAsset_Template_ResourceName);
            StreamReader reader = new StreamReader(stream);
            string output = reader.ReadToEnd();
            reader.Dispose();
            stream.Dispose();

            output = output.Replace("#NAME#", name);
            output = output.Replace("#PATH#", path);
            output = output.Replace("#OPTIONAL_HEADERS#", optionalHeaders);
            output = output.Replace("#FORMAT_TAG#", formatTag);
            output = output.Replace("#CHANNEL_COUNT#", channelCount);
            output = output.Replace("#SAMPLE_RATE#", sampleRate);
            output = output.Replace("#AVERAGE_BYTES_PER_SECOND#", averageBytesPerSecond);
            output = output.Replace("#BLOCK_ALIGN#", blockAlign);
            output = output.Replace("#BITS_PER_SAMPLE#", bitsPerSample);
            output = output.Replace("#EXTRA_SIZE#", extraSize);
            output = output.Replace("#BUFFER#", buffer);

            File.WriteAllText(outputHeaderFilePath, output);
        }
        public static void ImportBitmap(string bitmapImageFilePath, string outputHeaderFilePath)
        {
            string name = Path.GetFileNameWithoutExtension(bitmapImageFilePath);
            string path = bitmapImageFilePath;
            Bitmap bitmap = new Bitmap(bitmapImageFilePath);
            string width = bitmap.Width.ToString();
            string height = bitmap.Height.ToString();
            string buffer = BytesToHex(GetPixelData(bitmap));
            bitmap.Dispose();

            Stream stream = typeof(EZ.AssetManager).Assembly.GetManifestResourceStream(BitmapAsset_Template_ResourceName);
            StreamReader reader = new StreamReader(stream);
            string output = reader.ReadToEnd();
            reader.Dispose();
            stream.Dispose();

            output = output.Replace("#NAME#", name);
            output = output.Replace("#PATH#", path);
            output = output.Replace("#WIDTH#", width);
            output = output.Replace("#HEIGHT#", height);
            output = output.Replace("#BUFFER#", buffer);

            File.WriteAllText(outputHeaderFilePath, output);
        }
        // Converts any System.Drawing.Bitmap into a byte array.
        // Pixels are stored in the B8G8R8A8_UNORM pixel format.
        public static byte[] GetPixelData(Bitmap source)
        {
            byte[] output = new byte[source.Width * source.Height * 4];

            int offset = 0;
            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    Color pixel = source.GetPixel(x, y);
                    output[offset] = pixel.B; offset++;
                    output[offset] = pixel.G; offset++;
                    output[offset] = pixel.R; offset++;
                    output[offset] = pixel.A; offset++;
                }
            }

            return output;
        }
        // Constructs a c++ byte array definition from a c# byte array.
        // In the following form:
        // { 0x10, 0x12, 0xC1 }
        public static string BytesToHex(byte[] data)
        {
            char[] charset = new char[16] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

            char[] output = new char["{ ".Length + (data.Length * "0x00".Length) + ((data.Length - 1) * ", ".Length) + " }".Length];

            int index = 0;

            output[index] = '{'; index++;
            output[index] = ' '; index++;

            output[index] = '0'; index++;
            output[index] = 'x'; index++;
            output[index] = charset[data[0] & 0x0F]; index++;
            output[index] = charset[data[0] >> 4]; index++;

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

        public enum AudioAsset_FormatTags : ushort
        {
            WAVE_FORMAT_UNKNOWN = 0,
            WAVE_FORMAT_PCM = 1,
            WAVE_FORMAT_ADPCM = 2,
            WAVE_FORMAT_IEEE_FLOAT = 3,
            WAVE_FORMAT_VSELP = 4,
            WAVE_FORMAT_IBM_CVSD = 5,
            WAVE_FORMAT_ALAW = 6,
            WAVE_FORMAT_MULAW = 7,
            WAVE_FORMAT_DTS = 8,
            WAVE_FORMAT_DRM = 9,
            WAVE_FORMAT_WMAVOICE9 = 10,
            WAVE_FORMAT_OKI_ADPCM = 16,
            WAVE_FORMAT_DVI_ADPCM = 17,
            WAVE_FORMAT_IMA_ADPCM = 17,
            WAVE_FORMAT_MEDIASPACE_ADPCM = 18,
            WAVE_FORMAT_SIERRA_ADPCM = 19,
            WAVE_FORMAT_G723_ADPCM = 20,
            WAVE_FORMAT_DIGISTD = 21,
            WAVE_FORMAT_DIGIFIX = 22,
            WAVE_FORMAT_DIALOGIC_OKI_ADPCM = 23,
            WAVE_FORMAT_MEDIAVISION_ADPCM = 24,
            WAVE_FORMAT_CU_CODEC = 25,
            WAVE_FORMAT_YAMAHA_ADPCM = 32,
            WAVE_FORMAT_SONARC = 33,
            WAVE_FORMAT_DSPGROUP_TRUESPEECH = 34,
            WAVE_FORMAT_ECHOSC1 = 35,
            WAVE_FORMAT_AUDIOFILE_AF36 = 36,
            WAVE_FORMAT_APTX = 37,
            WAVE_FORMAT_AUDIOFILE_AF10 = 38,
            WAVE_FORMAT_PROSODY_1612 = 39,
            WAVE_FORMAT_LRC = 40,
            WAVE_FORMAT_DOLBY_AC2 = 48,
            WAVE_FORMAT_GSM610 = 49,
            WAVE_FORMAT_MSNAUDIO = 50,
            WAVE_FORMAT_ANTEX_ADPCME = 51,
            WAVE_FORMAT_CONTROL_RES_VQLPC = 52,
            WAVE_FORMAT_DIGIREAL = 53,
            WAVE_FORMAT_DIGIADPCM = 54,
            WAVE_FORMAT_CONTROL_RES_CR10 = 55,
            WAVE_FORMAT_NMS_VBXADPCM = 56,
            WAVE_FORMAT_CS_IMAADPCM = 57,
            WAVE_FORMAT_ECHOSC3 = 58,
            WAVE_FORMAT_ROCKWELL_ADPCM = 59,
            WAVE_FORMAT_ROCKWELL_DIGITALK = 60,
            WAVE_FORMAT_XEBEC = 61,
            WAVE_FORMAT_G721_ADPCM = 64,
            WAVE_FORMAT_G728_CELP = 65,
            WAVE_FORMAT_MSG723 = 66,
            WAVE_FORMAT_MPEG = 80,
            WAVE_FORMAT_RT24 = 82,
            WAVE_FORMAT_PAC = 83,
            WAVE_FORMAT_MPEGLAYER3 = 85,
            WAVE_FORMAT_LUCENT_G723 = 89,
            WAVE_FORMAT_CIRRUS = 96,
            WAVE_FORMAT_ESPCM = 97,
            WAVE_FORMAT_VOXWARE = 98,
            WAVE_FORMAT_CANOPUS_ATRAC = 99,
            WAVE_FORMAT_G726_ADPCM = 100,
            WAVE_FORMAT_G722_ADPCM = 101,
            WAVE_FORMAT_DSAT_DISPLAY = 103,
            WAVE_FORMAT_VOXWARE_BYTE_ALIGNED = 105,
            WAVE_FORMAT_VOXWARE_AC8 = 112,
            WAVE_FORMAT_VOXWARE_AC10 = 113,
            WAVE_FORMAT_VOXWARE_AC16 = 114,
            WAVE_FORMAT_VOXWARE_AC20 = 115,
            WAVE_FORMAT_VOXWARE_RT24 = 116,
            WAVE_FORMAT_VOXWARE_RT29 = 117,
            WAVE_FORMAT_VOXWARE_RT29HW = 118,
            WAVE_FORMAT_VOXWARE_VR12 = 119,
            WAVE_FORMAT_VOXWARE_VR18 = 120,
            WAVE_FORMAT_VOXWARE_TQ40 = 121,
            WAVE_FORMAT_SOFTSOUND = 128,
            WAVE_FORMAT_VOXWARE_TQ60 = 129,
            WAVE_FORMAT_MSRT24 = 130,
            WAVE_FORMAT_G729A = 131,
            WAVE_FORMAT_MVI_MVI2 = 132,
            WAVE_FORMAT_DF_G726 = 133,
            WAVE_FORMAT_DF_GSM610 = 134,
            WAVE_FORMAT_ISIAUDIO = 136,
            WAVE_FORMAT_ONLIVE = 137,
            WAVE_FORMAT_SBC24 = 145,
            WAVE_FORMAT_DOLBY_AC3_SPDIF = 146,
            WAVE_FORMAT_MEDIASONIC_G723 = 147,
            WAVE_FORMAT_PROSODY_8KBPS = 148,
            WAVE_FORMAT_ZYXEL_ADPCM = 151,
            WAVE_FORMAT_PHILIPS_LPCBB = 152,
            WAVE_FORMAT_PACKED = 153,
            WAVE_FORMAT_MALDEN_PHONYTALK = 160,
            WAVE_FORMAT_GSM = 161,
            WAVE_FORMAT_G729 = 162,
            WAVE_FORMAT_G723 = 163,
            WAVE_FORMAT_ACELP = 164,
            WAVE_FORMAT_RAW_AAC1 = 255,
            WAVE_FORMAT_RHETOREX_ADPCM = 256,
            WAVE_FORMAT_IRAT = 257,
            WAVE_FORMAT_VIVO_G723 = 273,
            WAVE_FORMAT_VIVO_SIREN = 274,
            WAVE_FORMAT_DIGITAL_G723 = 291,
            WAVE_FORMAT_SANYO_LD_ADPCM = 293,
            WAVE_FORMAT_SIPROLAB_ACEPLNET = 304,
            WAVE_FORMAT_SIPROLAB_ACELP4800 = 305,
            WAVE_FORMAT_SIPROLAB_ACELP8V3 = 306,
            WAVE_FORMAT_SIPROLAB_G729 = 307,
            WAVE_FORMAT_SIPROLAB_G729A = 308,
            WAVE_FORMAT_SIPROLAB_KELVIN = 309,
            WAVE_FORMAT_G726ADPCM = 320,
            WAVE_FORMAT_QUALCOMM_PUREVOICE = 336,
            WAVE_FORMAT_QUALCOMM_HALFRATE = 337,
            WAVE_FORMAT_TUBGSM = 341,
            WAVE_FORMAT_MSAUDIO1 = 352,
            WAVE_FORMAT_WMAUDIO2 = 353,
            WAVE_FORMAT_WMAUDIO3 = 354,
            WAVE_FORMAT_WMAUDIO_LOSSLESS = 355,
            WAVE_FORMAT_WMASPDIF = 356,
            WAVE_FORMAT_UNISYS_NAP_ADPCM = 368,
            WAVE_FORMAT_UNISYS_NAP_ULAW = 369,
            WAVE_FORMAT_UNISYS_NAP_ALAW = 370,
            WAVE_FORMAT_UNISYS_NAP_16K = 371,
            WAVE_FORMAT_CREATIVE_ADPCM = 512,
            WAVE_FORMAT_CREATIVE_FASTSPEECH8 = 514,
            WAVE_FORMAT_CREATIVE_FASTSPEECH10 = 515,
            WAVE_FORMAT_UHER_ADPCM = 528,
            WAVE_FORMAT_QUARTERDECK = 544,
            WAVE_FORMAT_ILINK_VC = 560,
            WAVE_FORMAT_RAW_SPORT = 576,
            WAVE_FORMAT_ESST_AC3 = 577,
            WAVE_FORMAT_IPI_HSX = 592,
            WAVE_FORMAT_IPI_RPELP = 593,
            WAVE_FORMAT_CS2 = 608,
            WAVE_FORMAT_SONY_SCX = 624,
            WAVE_FORMAT_FM_TOWNS_SND = 768,
            WAVE_FORMAT_BTV_DIGITAL = 1024,
            WAVE_FORMAT_QDESIGN_MUSIC = 1104,
            WAVE_FORMAT_VME_VMPCM = 1664,
            WAVE_FORMAT_TPC = 1665,
            WAVE_FORMAT_OLIGSM = 4096,
            WAVE_FORMAT_OLIADPCM = 4097,
            WAVE_FORMAT_OLICELP = 4098,
            WAVE_FORMAT_OLISBC = 4099,
            WAVE_FORMAT_OLIOPR = 4100,
            WAVE_FORMAT_LH_CODEC = 4352,
            WAVE_FORMAT_NORRIS = 5120,
            WAVE_FORMAT_SOUNDSPACE_MUSICOMPRESS = 5376,
            WAVE_FORMAT_MPEG_ADTS_AAC = 5632,
            MPEG_RAW_AAC = 5633, // wmCodec.h required
            WAVE_FORMAT_MPEG_LOAS = 5634,
            NOKIA_MPEG_ADTS_AAC = 5640, // wmCodec.h required
            NOKIA_MPEG_RAW_AAC = 5641, // wmCodec.h required
            VODAFONE_MPEG_ADTS_AAC = 5642, // wmCodec.h required
            VODAFONE_MPEG_RAW_AAC = 5643, // wmCodec.h required
            MPEG_HEAAC = 5648, // Not supported by EZAssetManager
            WAVE_FORMAT_DVM = 8192,
            WAVE_FORMAT_VORBIS1 = 26447,
            WAVE_FORMAT_VORBIS2 = 26448,
            WAVE_FORMAT_VORBIS3 = 26449,
            WAVE_FORMAT_VORBIS1P = 26479,
            WAVE_FORMAT_VORBIS2P = 26480,
            WAVE_FORMAT_VORBIS3P = 26481,
            WAVE_FORMAT_EXTENSIBLE = 65534,
            WAVE_FORMAT_DEVELOPMENT = ushort.MaxValue
        }
    }
}