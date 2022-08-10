using System;
using System.Text;

namespace Ionic.Zip
{
	// Token: 0x02000003 RID: 3
	internal class Util
	{
		// Token: 0x0600000F RID: 15 RVA: 0x00002488 File Offset: 0x00000688
		internal static string FormatByteArray(byte[] b)
		{
			int num = 96;
			StringBuilder stringBuilder = new StringBuilder(num * 2);
			if (num * 2 > b.Length)
			{
				for (int i = 0; i < b.Length; i++)
				{
					if (i != 0 && i % 16 == 0)
					{
						stringBuilder.Append("\n");
					}
					stringBuilder.Append(string.Format("{0:X2} ", b[i]));
				}
			}
			else
			{
				for (int i = 0; i < num; i++)
				{
					if (i != 0 && i % 16 == 0)
					{
						stringBuilder.Append("\n");
					}
					stringBuilder.Append(string.Format("{0:X2} ", b[i]));
				}
				if (b.Length > num * 2)
				{
					stringBuilder.Append(string.Format("\n   ...({0} other bytes here)....\n", b.Length - num * 2));
				}
				for (int i = 0; i < num; i++)
				{
					if (i != 0 && i % 16 == 0)
					{
						stringBuilder.Append("\n");
					}
					stringBuilder.Append(string.Format("{0:X2} ", b[b.Length - num + i]));
				}
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002610 File Offset: 0x00000810
		internal static string FormatByteArray(byte[] b, int limit)
		{
			byte[] array = new byte[limit];
			Array.Copy(b, 0, array, 0, limit);
			return Util.FormatByteArray(array);
		}
	}
}
