using System;
using System.Runtime.Serialization;

namespace Ionic.Zip
{
	// Token: 0x0200000D RID: 13
	[Serializable]
	public class BadStateException : ZipException
	{
		// Token: 0x06000051 RID: 81 RVA: 0x000035C0 File Offset: 0x000017C0
		public BadStateException()
		{
		}

		// Token: 0x06000052 RID: 82 RVA: 0x000035CC File Offset: 0x000017CC
		public BadStateException(string message) : base(message)
		{
		}

		// Token: 0x06000053 RID: 83 RVA: 0x000035D8 File Offset: 0x000017D8
		public BadStateException(string message, Exception innerException) : base(message, innerException)
		{
		}

		// Token: 0x06000054 RID: 84 RVA: 0x000035E8 File Offset: 0x000017E8
		protected BadStateException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
		{
		}
	}
}
