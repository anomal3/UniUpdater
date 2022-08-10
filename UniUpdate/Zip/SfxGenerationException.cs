using System;
using System.Runtime.Serialization;

namespace Ionic.Zip
{
	// Token: 0x0200000C RID: 12
	[Serializable]
	public class SfxGenerationException : ZipException
	{
		// Token: 0x0600004D RID: 77 RVA: 0x00003588 File Offset: 0x00001788
		public SfxGenerationException()
		{
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00003594 File Offset: 0x00001794
		public SfxGenerationException(string message) : base(message)
		{
		}

		// Token: 0x0600004F RID: 79 RVA: 0x000035A0 File Offset: 0x000017A0
		public SfxGenerationException(string message, Exception innerException) : base(message, innerException)
		{
		}

		// Token: 0x06000050 RID: 80 RVA: 0x000035B0 File Offset: 0x000017B0
		protected SfxGenerationException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
		{
		}
	}
}
