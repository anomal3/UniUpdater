using System;
using System.IO;
using System.Text;

namespace Ionic.Zlib
{
	// Token: 0x0200002D RID: 45
	internal class SharedUtils
	{
		// Token: 0x06000204 RID: 516 RVA: 0x000155C0 File Offset: 0x000137C0
		public static int URShift(int number, int bits)
		{
			int result;
			if (number >= 0)
			{
				result = number >> bits;
			}
			else
			{
				result = (number >> bits) + (2 << ~bits);
			}
			return result;
		}

		// Token: 0x06000205 RID: 517 RVA: 0x00015600 File Offset: 0x00013800
		public static int URShift(int number, long bits)
		{
			return SharedUtils.URShift(number, (int)bits);
		}

		// Token: 0x06000206 RID: 518 RVA: 0x00015624 File Offset: 0x00013824
		public static long URShift(long number, int bits)
		{
			long result;
			if (number >= 0L)
			{
				result = number >> bits;
			}
			else
			{
				result = (number >> bits) + (2L << ~bits);
			}
			return result;
		}

		// Token: 0x06000207 RID: 519 RVA: 0x00015664 File Offset: 0x00013864
		public static long URShift(long number, long bits)
		{
			return SharedUtils.URShift(number, (int)bits);
		}

		// Token: 0x06000208 RID: 520 RVA: 0x00015688 File Offset: 0x00013888
		public static int ReadInput(Stream sourceStream, byte[] target, int start, int count)
		{
			int result;
			if (target.Length == 0)
			{
				result = 0;
			}
			else if (count == 0)
			{
				result = 0;
			}
			else
			{
				int num = sourceStream.Read(target, start, count);
				result = num;
			}
			return result;
		}

		// Token: 0x06000209 RID: 521 RVA: 0x000156D8 File Offset: 0x000138D8
		public static int ReadInput(TextReader sourceTextReader, byte[] target, int start, int count)
		{
			int result;
			if (target.Length == 0)
			{
				result = 0;
			}
			else
			{
				char[] array = new char[target.Length];
				int num = sourceTextReader.Read(array, start, count);
				if (num == 0)
				{
					result = -1;
				}
				else
				{
					for (int i = start; i < start + num; i++)
					{
						target[i] = (byte)array[i];
					}
					result = num;
				}
			}
			return result;
		}

		// Token: 0x0600020A RID: 522 RVA: 0x00015754 File Offset: 0x00013954
		internal static byte[] ToByteArray(string sourceString)
		{
			return Encoding.UTF8.GetBytes(sourceString);
		}

		// Token: 0x0600020B RID: 523 RVA: 0x00015778 File Offset: 0x00013978
		internal static char[] ToCharArray(byte[] byteArray)
		{
			return Encoding.UTF8.GetChars(byteArray);
		}
	}
}
