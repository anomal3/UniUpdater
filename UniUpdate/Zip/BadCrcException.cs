using System;
using System.Runtime.Serialization;

namespace Ionic.Zip
{
	// Token: 0x0200000B RID: 11
	[Serializable]
	public class BadCrcException : ZipException
	{
		// Token: 0x06000049 RID: 73 RVA: 0x00003550 File Offset: 0x00001750
		public BadCrcException()
		{
		}

		// Token: 0x0600004A RID: 74 RVA: 0x0000355C File Offset: 0x0000175C
		public BadCrcException(string message) : base(message)
		{
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00003568 File Offset: 0x00001768
		public BadCrcException(string message, Exception innerException) : base(message, innerException)
		{
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00003578 File Offset: 0x00001778
		protected BadCrcException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
		{
		}
	}
}
