using System;
using System.IO;
using System.Text;

namespace Ionic.Zip
{
	// Token: 0x0200000E RID: 14
	internal class SharedUtilities
	{
		// Token: 0x06000055 RID: 85 RVA: 0x000035F8 File Offset: 0x000017F8
		private SharedUtilities()
		{
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00003604 File Offset: 0x00001804
		public static DateTime RoundToEvenSecond(DateTime source)
		{
			if (source.Second % 2 == 1)
			{
				source += new TimeSpan(0, 0, 1);
			}
			DateTime result = new DateTime(source.Year, source.Month, source.Day, source.Hour, source.Minute, source.Second);
			return result;
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00003678 File Offset: 0x00001878
		public static string TrimVolumeAndSwapSlashes(string pathName)
		{
			string result;
			if (string.IsNullOrEmpty(pathName))
			{
				result = pathName;
			}
			else if (pathName.Length < 2)
			{
				result = pathName.Replace('\\', '/');
			}
			else
			{
				result = ((pathName[1] == ':' && pathName[2] == '\\') ? pathName.Substring(3) : pathName).Replace('\\', '/');
			}
			return result;
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00003700 File Offset: 0x00001900
		internal static byte[] StringToByteArray(string value, Encoding encoding)
		{
			return encoding.GetBytes(value);
		}

		// Token: 0x06000059 RID: 89 RVA: 0x00003724 File Offset: 0x00001924
		internal static byte[] StringToByteArray(string value)
		{
			return SharedUtilities.StringToByteArray(value, SharedUtilities.ibm437);
		}

		// Token: 0x0600005A RID: 90 RVA: 0x00003748 File Offset: 0x00001948
		internal static byte[] Utf8StringToByteArray(string value)
		{
			return SharedUtilities.StringToByteArray(value, SharedUtilities.utf8);
		}

		// Token: 0x0600005B RID: 91 RVA: 0x0000376C File Offset: 0x0000196C
		internal static string StringFromBuffer(byte[] buf, int maxlength)
		{
			return SharedUtilities.StringFromBuffer(buf, maxlength, SharedUtilities.ibm437);
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00003794 File Offset: 0x00001994
		internal static string Utf8StringFromBuffer(byte[] buf, int maxlength)
		{
			return SharedUtilities.StringFromBuffer(buf, maxlength, SharedUtilities.utf8);
		}

		// Token: 0x0600005D RID: 93 RVA: 0x000037BC File Offset: 0x000019BC
		internal static string StringFromBuffer(byte[] buf, int maxlength, Encoding encoding)
		{
			return encoding.GetString(buf, 0, buf.Length);
		}

		// Token: 0x0600005E RID: 94 RVA: 0x000037E4 File Offset: 0x000019E4
		internal static int ReadSignature(Stream s)
		{
			int result = 0;
			try
			{
				result = SharedUtilities._ReadFourBytes(s, "nul");
			}
			catch
			{
			}
			return result;
		}

		// Token: 0x0600005F RID: 95 RVA: 0x00003828 File Offset: 0x00001A28
		internal static int ReadInt(Stream s)
		{
			return SharedUtilities._ReadFourBytes(s, "Could not read block - no data!  (position 0x{0:X8})");
		}

		// Token: 0x06000060 RID: 96 RVA: 0x0000384C File Offset: 0x00001A4C
		private static int _ReadFourBytes(Stream s, string message)
		{
			byte[] array = new byte[4];
			int num = s.Read(array, 0, array.Length);
			if (num != array.Length)
			{
				throw new BadReadException(string.Format(message, s.Position));
			}
			return (((int)array[3] * 256 + (int)array[2]) * 256 + (int)array[1]) * 256 + (int)array[0];
		}

		// Token: 0x06000061 RID: 97 RVA: 0x000038C4 File Offset: 0x00001AC4
		protected internal static long FindSignature(Stream stream, int SignatureToFind)
		{
			long position = stream.Position;
			int num = 65536;
			byte[] array = new byte[]
			{
				(byte)(SignatureToFind >> 24),
				(byte)((SignatureToFind & 16711680) >> 16),
				(byte)((SignatureToFind & 65280) >> 8),
				(byte)(SignatureToFind & 255)
			};
			byte[] array2 = new byte[num];
			bool flag = false;
			do
			{
				int num2 = stream.Read(array2, 0, array2.Length);
				if (num2 == 0)
				{
					break;
				}
				for (int i = 0; i < num2; i++)
				{
					if (array2[i] == array[3])
					{
						long position2 = stream.Position;
						stream.Seek((long)(i - num2), SeekOrigin.Current);
						int num3 = SharedUtilities.ReadSignature(stream);
						flag = (num3 == SignatureToFind);
						if (flag)
						{
							break;
						}
						stream.Seek(position2, SeekOrigin.Begin);
					}
				}
			}
			while (!flag);
			long result;
			if (!flag)
			{
				stream.Seek(position, SeekOrigin.Begin);
				result = -1L;
			}
			else
			{
				long num4 = stream.Position - position - 4L;
				result = num4;
			}
			return result;
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00003A18 File Offset: 0x00001C18
		internal static DateTime PackedToDateTime(int packedDateTime)
		{
			DateTime result;
			if (packedDateTime == 65535 || packedDateTime == 0)
			{
				result = new DateTime(1995, 1, 1, 0, 0, 0, 0);
			}
			else
			{
				short num = (short)(packedDateTime & 65535);
				short num2 = (short)(((long)packedDateTime & (long)(-65536)) >> 16);
				int num3 = 1980 + (((int)num2 & 65024) >> 9);
				int num4 = (num2 & 480) >> 5;
				int num5 = (int)(num2 & 31);
				int num6 = ((int)num & 63488) >> 11;
				int num7 = (num & 2016) >> 5;
				int num8 = (int)((num & 31) * 2);
				if (num8 >= 60)
				{
					num7++;
					num8 = 0;
				}
				if (num7 >= 60)
				{
					num6++;
					num7 = 0;
				}
				if (num6 >= 24)
				{
					num5++;
					num6 = 0;
				}
				DateTime dateTime = DateTime.Now;
				bool flag = false;
				try
				{
					dateTime = new DateTime(num3, num4, num5, num6, num7, num8, 0);
					flag = true;
				}
				catch (ArgumentOutOfRangeException)
				{
					if (num3 == 1980 && num4 == 0 && num5 == 0)
					{
						try
						{
							dateTime = new DateTime(1980, 1, 1, num6, num7, num8, 0);
							flag = true;
						}
						catch (ArgumentOutOfRangeException)
						{
							try
							{
								dateTime = new DateTime(1980, 1, 1, 0, 0, 0, 0);
								flag = true;
							}
							catch (ArgumentOutOfRangeException)
							{
							}
						}
					}
				}
				if (!flag)
				{
					string arg = string.Format("y({0}) m({1}) d({2}) h({3}) m({4}) s({5})", new object[]
					{
						num3,
						num4,
						num5,
						num6,
						num7,
						num8
					});
					throw new ZipException(string.Format("Bad date/time format in the zip file. ({0})", arg));
				}
				result = dateTime;
			}
			return result;
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00003C4C File Offset: 0x00001E4C
		internal static int DateTimeToPacked(DateTime time)
		{
			ushort num = (ushort)((time.Day & 31) | (time.Month << 5 & 480) | (time.Year - 1980 << 9 & 65024));
			ushort num2 = (ushort)((time.Second / 2 & 31) | (time.Minute << 5 & 2016) | (time.Hour << 11 & 63488));
			return (int)num << 16 | (int)num2;
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00003CD0 File Offset: 0x00001ED0
		public static MemoryStream StringToMemoryStream(string s)
		{
			MemoryStream memoryStream = new MemoryStream();
			StreamWriter streamWriter = new StreamWriter(memoryStream);
			streamWriter.Write(s);
			streamWriter.Flush();
			return memoryStream;
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00003D08 File Offset: 0x00001F08
		public static string GetTempFilename()
		{
			string text;
			do
			{
				text = "DotNetZip-" + SharedUtilities.GenerateRandomStringImpl(8, 97) + ".tmp";
			}
			while (File.Exists(text));
			return text;
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00003D48 File Offset: 0x00001F48
		private static string GenerateRandomStringImpl(int length, int delta)
		{
			bool flag = delta == 0;
			char[] array = new char[length];
			for (int i = 0; i < length; i++)
			{
				if (flag)
				{
					delta = ((SharedUtilities._rnd.Next(2) == 0) ? 65 : 97);
				}
				array[i] = SharedUtilities.GetOneRandomChar(delta);
			}
			return new string(array);
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00003DC8 File Offset: 0x00001FC8
		private static char GetOneRandomChar(int delta)
		{
			return (char)(SharedUtilities._rnd.Next(26) + delta);
		}

		// Token: 0x04000031 RID: 49
		private static Encoding ibm437 = Encoding.GetEncoding("IBM437");

		// Token: 0x04000032 RID: 50
		private static Encoding utf8 = Encoding.GetEncoding("UTF-8");

		// Token: 0x04000033 RID: 51
		private static Random _rnd = new Random();
	}
}
