using System;
using System.Runtime.Serialization;

namespace Ionic.Zip
{
	// Token: 0x02000008 RID: 8
	[Serializable]
	public class ZipException : Exception
	{
		// Token: 0x0600003D RID: 61 RVA: 0x000034A8 File Offset: 0x000016A8
		public ZipException()
		{
		}

		// Token: 0x0600003E RID: 62 RVA: 0x000034B4 File Offset: 0x000016B4
		public ZipException(string message) : base(message)
		{
		}

		// Token: 0x0600003F RID: 63 RVA: 0x000034C0 File Offset: 0x000016C0
		public ZipException(string message, Exception innerException) : base(message, innerException)
		{
		}

		// Token: 0x06000040 RID: 64 RVA: 0x000034D0 File Offset: 0x000016D0
		protected ZipException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
		{
		}
	}
}
