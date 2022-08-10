using System;
using System.Runtime.Serialization;

namespace Ionic.Zip
{
	// Token: 0x0200000A RID: 10
	[Serializable]
	public class BadReadException : ZipException
	{
		// Token: 0x06000045 RID: 69 RVA: 0x00003518 File Offset: 0x00001718
		public BadReadException()
		{
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00003524 File Offset: 0x00001724
		public BadReadException(string message) : base(message)
		{
		}

		// Token: 0x06000047 RID: 71 RVA: 0x00003530 File Offset: 0x00001730
		public BadReadException(string message, Exception innerException) : base(message, innerException)
		{
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00003540 File Offset: 0x00001740
		protected BadReadException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
		{
		}
	}
}
