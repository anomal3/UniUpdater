using System;
using System.Runtime.Serialization;

namespace Ionic.Zip
{
	// Token: 0x02000009 RID: 9
	[Serializable]
	public class BadPasswordException : ZipException
	{
		// Token: 0x06000041 RID: 65 RVA: 0x000034E0 File Offset: 0x000016E0
		public BadPasswordException()
		{
		}

		// Token: 0x06000042 RID: 66 RVA: 0x000034EC File Offset: 0x000016EC
		public BadPasswordException(string message) : base(message)
		{
		}

		// Token: 0x06000043 RID: 67 RVA: 0x000034F8 File Offset: 0x000016F8
		public BadPasswordException(string message, Exception innerException) : base(message, innerException)
		{
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00003508 File Offset: 0x00001708
		protected BadPasswordException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
		{
		}
	}
}
