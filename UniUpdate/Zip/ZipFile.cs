using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zlib;
using Microsoft.CSharp;

namespace Ionic.Zip
{
	// Token: 0x0200001D RID: 29
	public class ZipFile : IEnumerable<ZipEntry>, IEnumerable, IDisposable
	{
		// Token: 0x1700004C RID: 76
		// (get) Token: 0x0600011E RID: 286 RVA: 0x00009B70 File Offset: 0x00007D70
		public string Name
		{
			get
			{
				return this._name;
			}
		}

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x0600011F RID: 287 RVA: 0x00009B90 File Offset: 0x00007D90
		// (set) Token: 0x06000120 RID: 288 RVA: 0x00009BB0 File Offset: 0x00007DB0
		public CompressionLevel CompressionLevel { get; set; }

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x06000121 RID: 289 RVA: 0x00009BBC File Offset: 0x00007DBC
		// (set) Token: 0x06000122 RID: 290 RVA: 0x00009BDC File Offset: 0x00007DDC
		public string Comment
		{
			get
			{
				return this._Comment;
			}
			set
			{
				this._Comment = value;
				this._contentsChanged = true;
			}
		}

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x06000123 RID: 291 RVA: 0x00009BF0 File Offset: 0x00007DF0
		private bool Verbose
		{
			get
			{
				return this._StatusMessageTextWriter != null;
			}
		}

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x06000124 RID: 292 RVA: 0x00009C18 File Offset: 0x00007E18
		// (set) Token: 0x06000125 RID: 293 RVA: 0x00009C38 File Offset: 0x00007E38
		public bool CaseSensitiveRetrieval
		{
			get
			{
				return this._CaseSensitiveRetrieval;
			}
			set
			{
				this._CaseSensitiveRetrieval = value;
			}
		}

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x06000126 RID: 294 RVA: 0x00009C44 File Offset: 0x00007E44
		// (set) Token: 0x06000127 RID: 295 RVA: 0x00009C70 File Offset: 0x00007E70
		public bool UseUnicodeAsNecessary
		{
			get
			{
				return this._provisionalAlternateEncoding == Encoding.GetEncoding("UTF-8");
			}
			set
			{
				this._provisionalAlternateEncoding = (value ? Encoding.GetEncoding("UTF-8") : ZipFile.DefaultEncoding);
			}
		}

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x06000128 RID: 296 RVA: 0x00009C94 File Offset: 0x00007E94
		// (set) Token: 0x06000129 RID: 297 RVA: 0x00009CB4 File Offset: 0x00007EB4
		public Zip64Option UseZip64WhenSaving
		{
			get
			{
				return this._zip64;
			}
			set
			{
				this._zip64 = value;
			}
		}

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x0600012A RID: 298 RVA: 0x00009CC0 File Offset: 0x00007EC0
		public bool? RequiresZip64
		{
			get
			{
				bool? result;
				if (this._entries.Count > 65534)
				{
					result = new bool?(true);
				}
				else if (!this._hasBeenSaved || this._contentsChanged)
				{
					result = null;
				}
				else
				{
					foreach (ZipEntry zipEntry in this._entries)
					{
						if (zipEntry.RequiresZip64.Value)
						{
							return new bool?(true);
						}
					}
					result = new bool?(false);
				}
				return result;
			}
		}

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x0600012B RID: 299 RVA: 0x00009DA8 File Offset: 0x00007FA8
		public bool? OutputUsedZip64
		{
			get
			{
				return this._OutputUsesZip64;
			}
		}

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x0600012C RID: 300 RVA: 0x00009DC8 File Offset: 0x00007FC8
		// (set) Token: 0x0600012D RID: 301 RVA: 0x00009DE8 File Offset: 0x00007FE8
		public Encoding ProvisionalAlternateEncoding
		{
			get
			{
				return this._provisionalAlternateEncoding;
			}
			set
			{
				this._provisionalAlternateEncoding = value;
			}
		}

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x0600012E RID: 302 RVA: 0x00009DF4 File Offset: 0x00007FF4
		// (set) Token: 0x0600012F RID: 303 RVA: 0x00009E14 File Offset: 0x00008014
		public TextWriter StatusMessageTextWriter
		{
			get
			{
				return this._StatusMessageTextWriter;
			}
			set
			{
				this._StatusMessageTextWriter = value;
			}
		}

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x06000130 RID: 304 RVA: 0x00009E20 File Offset: 0x00008020
		// (set) Token: 0x06000131 RID: 305 RVA: 0x00009E40 File Offset: 0x00008040
		public bool ForceNoCompression
		{
			get
			{
				return this._ForceNoCompression;
			}
			set
			{
				this._ForceNoCompression = value;
			}
		}

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x06000132 RID: 306 RVA: 0x00009E4C File Offset: 0x0000804C
		// (set) Token: 0x06000133 RID: 307 RVA: 0x00009EAC File Offset: 0x000080AC
		public string TempFileFolder
		{
			get
			{
				if (this._TempFileFolder == null)
				{
					this._TempFileFolder = Path.GetTempPath();
					if (this._TempFileFolder == null)
					{
						this._TempFileFolder = ".";
					}
				}
				return this._TempFileFolder;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentException("You may not set the TempFileFolder to a null value.");
				}
				if (!Directory.Exists(value))
				{
					throw new FileNotFoundException(string.Format("That directory ({0}) does not exist.", value));
				}
				this._TempFileFolder = value;
			}
		}

		// Token: 0x17000059 RID: 89
		// (set) Token: 0x06000134 RID: 308 RVA: 0x00009EFC File Offset: 0x000080FC
		public string Password
		{
			set
			{
				this._Password = value;
				if (this._Password == null)
				{
					this.Encryption = EncryptionAlgorithm.None;
				}
				else if (this.Encryption == EncryptionAlgorithm.None)
				{
					this.Encryption = EncryptionAlgorithm.PkzipWeak;
				}
			}
		}

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x06000135 RID: 309 RVA: 0x00009F54 File Offset: 0x00008154
		// (set) Token: 0x06000136 RID: 310 RVA: 0x00009F74 File Offset: 0x00008174
		public EncryptionAlgorithm Encryption { get; set; }

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x06000137 RID: 311 RVA: 0x00009F80 File Offset: 0x00008180
		// (set) Token: 0x06000138 RID: 312 RVA: 0x00009FA0 File Offset: 0x000081A0
		public ReReadApprovalCallback WillReadTwiceOnInflation { get; set; }

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x06000139 RID: 313 RVA: 0x00009FAC File Offset: 0x000081AC
		// (set) Token: 0x0600013A RID: 314 RVA: 0x00009FCC File Offset: 0x000081CC
		public WantCompressionCallback WantCompression { get; set; }

		// Token: 0x0600013B RID: 315 RVA: 0x00009FD8 File Offset: 0x000081D8
		internal void NotifyEntryChanged()
		{
			this._contentsChanged = true;
		}

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x0600013C RID: 316 RVA: 0x00009FE4 File Offset: 0x000081E4
		internal Stream ReadStream
		{
			get
			{
				if (this._readstream == null && this._name != null)
				{
					try
					{
						this._readstream = File.OpenRead(this._name);
						this._ReadStreamIsOurs = true;
					}
					catch (IOException innerException)
					{
						throw new ZipException("Error opening the file", innerException);
					}
				}
				return this._readstream;
			}
		}

		// Token: 0x0600013D RID: 317 RVA: 0x0000A064 File Offset: 0x00008264
		internal void Reset()
		{
			if (this._JustSaved)
			{
				ZipFile zipFile = new ZipFile();
				zipFile._name = this._name;
				zipFile.ProvisionalAlternateEncoding = this.ProvisionalAlternateEncoding;
				ZipFile.ReadIntoInstance(zipFile);
				foreach (ZipEntry zipEntry in zipFile)
				{
					foreach (ZipEntry zipEntry2 in this)
					{
						if (zipEntry.FileName == zipEntry2.FileName)
						{
							zipEntry2.CopyMetaData(zipEntry);
						}
					}
				}
				this._JustSaved = false;
			}
		}

		// Token: 0x0600013E RID: 318 RVA: 0x0000A16C File Offset: 0x0000836C
		public ZipFile(string zipFileName)
		{
			try
			{
				this.InitFile(zipFileName, null);
			}
			catch (Exception innerException)
			{
				throw new ZipException(string.Format("{0} is not a valid zip file", zipFileName), innerException);
			}
		}

		// Token: 0x0600013F RID: 319 RVA: 0x0000A1F8 File Offset: 0x000083F8
		public ZipFile(string zipFileName, Encoding encoding)
		{
			try
			{
				this.InitFile(zipFileName, null);
				this.ProvisionalAlternateEncoding = encoding;
			}
			catch (Exception innerException)
			{
				throw new ZipException(string.Format("{0} is not a valid zip file", zipFileName), innerException);
			}
		}

		// Token: 0x06000140 RID: 320 RVA: 0x0000A28C File Offset: 0x0000848C
		public ZipFile()
		{
			this.InitFile(null, null);
		}

		// Token: 0x06000141 RID: 321 RVA: 0x0000A2F0 File Offset: 0x000084F0
		public ZipFile(Encoding encoding)
		{
			this.InitFile(null, null);
			this.ProvisionalAlternateEncoding = encoding;
		}

		// Token: 0x06000142 RID: 322 RVA: 0x0000A35C File Offset: 0x0000855C
		public ZipFile(string zipFileName, TextWriter statusMessageWriter)
		{
			try
			{
				this.InitFile(zipFileName, statusMessageWriter);
			}
			catch (Exception innerException)
			{
				throw new ZipException(string.Format("{0} is not a valid zip file", zipFileName), innerException);
			}
		}

		// Token: 0x06000143 RID: 323 RVA: 0x0000A3E8 File Offset: 0x000085E8
		public ZipFile(string zipFileName, TextWriter statusMessageWriter, Encoding encoding)
		{
			try
			{
				this.InitFile(zipFileName, statusMessageWriter);
				this.ProvisionalAlternateEncoding = encoding;
			}
			catch (Exception innerException)
			{
				throw new ZipException(string.Format("{0} is not a valid zip file", zipFileName), innerException);
			}
		}

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x06000144 RID: 324 RVA: 0x0000A47C File Offset: 0x0000867C
		public static Version LibraryVersion
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Version;
			}
		}

		// Token: 0x06000145 RID: 325 RVA: 0x0000A4A4 File Offset: 0x000086A4
		private void InitFile(string zipFileName, TextWriter statusMessageWriter)
		{
			this._name = zipFileName;
			this._StatusMessageTextWriter = statusMessageWriter;
			this._contentsChanged = true;
			this.CompressionLevel = CompressionLevel.DEFAULT;
			if (File.Exists(this._name))
			{
				ZipFile.ReadIntoInstance(this);
				this._fileAlreadyExists = true;
			}
			else
			{
				this._entries = new List<ZipEntry>();
			}
		}

		// Token: 0x06000146 RID: 326 RVA: 0x0000A510 File Offset: 0x00008710
		public ZipEntry AddItem(string fileOrDirectoryName)
		{
			return this.AddItem(fileOrDirectoryName, null);
		}

		// Token: 0x06000147 RID: 327 RVA: 0x0000A534 File Offset: 0x00008734
		public ZipEntry AddItem(string fileOrDirectoryName, string directoryPathInArchive)
		{
			ZipEntry result;
			if (File.Exists(fileOrDirectoryName))
			{
				result = this.AddFile(fileOrDirectoryName, directoryPathInArchive);
			}
			else
			{
				if (!Directory.Exists(fileOrDirectoryName))
				{
					throw new FileNotFoundException(string.Format("That file or directory ({0}) does not exist!", fileOrDirectoryName));
				}
				result = this.AddDirectory(fileOrDirectoryName, directoryPathInArchive);
			}
			return result;
		}

		// Token: 0x06000148 RID: 328 RVA: 0x0000A594 File Offset: 0x00008794
		public ZipEntry AddFile(string fileName)
		{
			return this.AddFile(fileName, null);
		}

		// Token: 0x06000149 RID: 329 RVA: 0x0000A5B8 File Offset: 0x000087B8
		public ZipEntry AddFile(string fileName, string directoryPathInArchive)
		{
			string nameInArchive = ZipEntry.NameInArchive(fileName, directoryPathInArchive);
			ZipEntry zipEntry = ZipEntry.Create(fileName, nameInArchive);
			zipEntry.ForceNoCompression = this.ForceNoCompression;
			zipEntry.WillReadTwiceOnInflation = this.WillReadTwiceOnInflation;
			zipEntry.WantCompression = this.WantCompression;
			zipEntry.ProvisionalAlternateEncoding = this.ProvisionalAlternateEncoding;
			zipEntry._Source = EntrySource.Filesystem;
			zipEntry._zipfile = this;
			zipEntry.Encryption = this.Encryption;
			zipEntry.Password = this._Password;
			if (this.Verbose)
			{
				this.StatusMessageTextWriter.WriteLine("adding {0}...", fileName);
			}
			this.InsureUniqueEntry(zipEntry);
			this._entries.Add(zipEntry);
			this._contentsChanged = true;
			return zipEntry;
		}

		// Token: 0x0600014A RID: 330 RVA: 0x0000A67C File Offset: 0x0000887C
		public ZipEntry UpdateFile(string fileName)
		{
			return this.UpdateFile(fileName, null);
		}

		// Token: 0x0600014B RID: 331 RVA: 0x0000A6A0 File Offset: 0x000088A0
		public ZipEntry UpdateFile(string fileName, string directoryPathInArchive)
		{
			string fileName2 = ZipEntry.NameInArchive(fileName, directoryPathInArchive);
			if (this[fileName2] != null)
			{
				this.RemoveEntry(fileName2);
			}
			return this.AddFile(fileName, directoryPathInArchive);
		}

		// Token: 0x0600014C RID: 332 RVA: 0x0000A6E4 File Offset: 0x000088E4
		public ZipEntry UpdateDirectory(string directoryName)
		{
			return this.UpdateDirectory(directoryName, null);
		}

		// Token: 0x0600014D RID: 333 RVA: 0x0000A708 File Offset: 0x00008908
		public ZipEntry UpdateDirectory(string directoryName, string directoryPathInArchive)
		{
			return this.AddOrUpdateDirectoryImpl(directoryName, directoryPathInArchive, AddOrUpdateAction.AddOrUpdate);
		}

		// Token: 0x0600014E RID: 334 RVA: 0x0000A72C File Offset: 0x0000892C
		public void UpdateItem(string itemName)
		{
			this.UpdateItem(itemName, null);
		}

		// Token: 0x0600014F RID: 335 RVA: 0x0000A738 File Offset: 0x00008938
		public void UpdateItem(string itemName, string directoryPathInArchive)
		{
			if (File.Exists(itemName))
			{
				this.UpdateFile(itemName, directoryPathInArchive);
			}
			else
			{
				if (!Directory.Exists(itemName))
				{
					throw new FileNotFoundException(string.Format("That file or directory ({0}) does not exist!", itemName));
				}
				this.UpdateDirectory(itemName, directoryPathInArchive);
			}
		}

		// Token: 0x06000150 RID: 336 RVA: 0x0000A798 File Offset: 0x00008998
		public ZipEntry AddFileStream(string fileName, string directoryPathInArchive, Stream stream)
		{
			string nameInArchive = ZipEntry.NameInArchive(fileName, directoryPathInArchive);
			ZipEntry zipEntry = ZipEntry.Create(fileName, nameInArchive, stream);
			zipEntry.ForceNoCompression = this.ForceNoCompression;
			zipEntry.WillReadTwiceOnInflation = this.WillReadTwiceOnInflation;
			zipEntry.WantCompression = this.WantCompression;
			zipEntry.ProvisionalAlternateEncoding = this.ProvisionalAlternateEncoding;
			zipEntry._Source = EntrySource.Stream;
			zipEntry._zipfile = this;
			zipEntry.Encryption = this.Encryption;
			zipEntry.Password = this._Password;
			if (this.Verbose)
			{
				this.StatusMessageTextWriter.WriteLine("adding {0}...", fileName);
			}
			this.InsureUniqueEntry(zipEntry);
			this._entries.Add(zipEntry);
			this._contentsChanged = true;
			return zipEntry;
		}

		// Token: 0x06000151 RID: 337 RVA: 0x0000A860 File Offset: 0x00008A60
		public ZipEntry AddFileFromString(string fileName, string directoryPathInArchive, string content)
		{
			MemoryStream stream = SharedUtilities.StringToMemoryStream(content);
			return this.AddFileStream(fileName, directoryPathInArchive, stream);
		}

		// Token: 0x06000152 RID: 338 RVA: 0x0000A88C File Offset: 0x00008A8C
		public ZipEntry UpdateFileStream(string fileName, string directoryPathInArchive, Stream stream)
		{
			string fileName2 = ZipEntry.NameInArchive(fileName, directoryPathInArchive);
			if (this[fileName2] != null)
			{
				this.RemoveEntry(fileName2);
			}
			return this.AddFileStream(fileName, directoryPathInArchive, stream);
		}

		// Token: 0x06000153 RID: 339 RVA: 0x0000A8D0 File Offset: 0x00008AD0
		private void InsureUniqueEntry(ZipEntry ze1)
		{
			foreach (ZipEntry zipEntry in this._entries)
			{
				if (SharedUtilities.TrimVolumeAndSwapSlashes(ze1.FileName) == zipEntry.FileName)
				{
					throw new ArgumentException(string.Format("The entry '{0}' already exists in the zip archive.", ze1.FileName));
				}
			}
		}

		// Token: 0x06000154 RID: 340 RVA: 0x0000A960 File Offset: 0x00008B60
		public ZipEntry AddDirectory(string directoryName)
		{
			return this.AddDirectory(directoryName, null);
		}

		// Token: 0x06000155 RID: 341 RVA: 0x0000A984 File Offset: 0x00008B84
		public ZipEntry AddDirectory(string directoryName, string directoryPathInArchive)
		{
			return this.AddOrUpdateDirectoryImpl(directoryName, directoryPathInArchive, AddOrUpdateAction.AddOnly);
		}

		// Token: 0x06000156 RID: 342 RVA: 0x0000A9A8 File Offset: 0x00008BA8
		public ZipEntry AddDirectoryByName(string directoryNameInArchive)
		{
			ZipEntry zipEntry = ZipEntry.Create(directoryNameInArchive, directoryNameInArchive);
			zipEntry._Source = EntrySource.Filesystem;
			zipEntry.MarkAsDirectory();
			zipEntry._zipfile = this;
			this.InsureUniqueEntry(zipEntry);
			this._entries.Add(zipEntry);
			this._contentsChanged = true;
			return zipEntry;
		}

		// Token: 0x06000157 RID: 343 RVA: 0x0000A9FC File Offset: 0x00008BFC
		private ZipEntry AddOrUpdateDirectoryImpl(string directoryName, string rootDirectoryPathInArchive, AddOrUpdateAction action)
		{
			if (rootDirectoryPathInArchive == null)
			{
				rootDirectoryPathInArchive = "";
			}
			return this.AddOrUpdateDirectoryImpl(directoryName, rootDirectoryPathInArchive, action, 0);
		}

		// Token: 0x06000158 RID: 344 RVA: 0x0000AA38 File Offset: 0x00008C38
		private ZipEntry AddOrUpdateDirectoryImpl(string directoryName, string rootDirectoryPathInArchive, AddOrUpdateAction action, int level)
		{
			if (this.Verbose)
			{
				this.StatusMessageTextWriter.WriteLine("{0} {1}...", (action == AddOrUpdateAction.AddOnly) ? "adding" : "Adding or updating", directoryName);
			}
			string text = rootDirectoryPathInArchive;
			ZipEntry zipEntry = null;
			if (level > 0)
			{
				int num = directoryName.Length;
				for (int i = level; i > 0; i--)
				{
					num = directoryName.LastIndexOfAny("/\\".ToCharArray(), num - 1, num - 1);
				}
				text = directoryName.Substring(num + 1);
				text = Path.Combine(rootDirectoryPathInArchive, text);
			}
			if (level > 0 || rootDirectoryPathInArchive != "")
			{
				zipEntry = ZipEntry.Create(directoryName, text);
				zipEntry.ProvisionalAlternateEncoding = this.ProvisionalAlternateEncoding;
				zipEntry._Source = EntrySource.Filesystem;
				zipEntry.MarkAsDirectory();
				zipEntry._zipfile = this;
				ZipEntry zipEntry2 = this[zipEntry.FileName];
				if (zipEntry2 == null)
				{
					this._entries.Add(zipEntry);
					this._contentsChanged = true;
				}
				text = zipEntry.FileName;
			}
			string[] files = Directory.GetFiles(directoryName);
			foreach (string fileName in files)
			{
				if (action == AddOrUpdateAction.AddOnly)
				{
					this.AddFile(fileName, text);
				}
				else
				{
					this.UpdateFile(fileName, text);
				}
			}
			string[] directories = Directory.GetDirectories(directoryName);
			foreach (string directoryName2 in directories)
			{
				this.AddOrUpdateDirectoryImpl(directoryName2, rootDirectoryPathInArchive, action, level + 1);
			}
			return zipEntry;
		}

		// Token: 0x06000159 RID: 345 RVA: 0x0000AC08 File Offset: 0x00008E08
		public void Save()
		{
			try
			{
				bool flag = false;
				this._saveOperationCanceled = false;
				this.OnSaveStarted();
				if (this.WriteStream == null)
				{
					throw new BadStateException("You haven't specified where to save the zip.");
				}
				if (this._contentsChanged)
				{
					if (this.Verbose)
					{
						this.StatusMessageTextWriter.WriteLine("Saving....");
					}
					if (this._entries.Count >= 65535 && this._zip64 == Zip64Option.Default)
					{
						throw new ZipException("The number of entries is 0xFFFF or greater. Consider setting the UseZip64WhenSaving property on the ZipFile instance.");
					}
					int num = 0;
					foreach (ZipEntry zipEntry in this._entries)
					{
						this.OnSaveEntry(num, zipEntry, true);
						zipEntry.Write(this.WriteStream);
						zipEntry._zipfile = this;
						num++;
						this.OnSaveEntry(num, zipEntry, false);
						if (this._saveOperationCanceled)
						{
							break;
						}
						flag |= zipEntry.OutputUsedZip64.Value;
					}
					if (!this._saveOperationCanceled)
					{
						this.WriteCentralDirectoryStructure(this.WriteStream);
						this.OnSaveEvent(ZipProgressEventType.Saving_AfterSaveTempArchive);
						this._hasBeenSaved = true;
						this._contentsChanged = false;
						flag |= this._NeedZip64CentralDirectory;
						this._OutputUsesZip64 = new bool?(flag);
						if (this._temporaryFileName != null && this._name != null)
						{
							this.WriteStream.Close();
							this.WriteStream.Dispose();
							this.WriteStream = null;
							if (this._saveOperationCanceled)
							{
								return;
							}
							if (this._fileAlreadyExists && this._readstream != null)
							{
								this._readstream.Close();
								this._readstream = null;
							}
							if (this._fileAlreadyExists)
							{
								File.Delete(this._name);
								this.OnSaveEvent(ZipProgressEventType.Saving_BeforeRenameTempArchive);
								File.Move(this._temporaryFileName, this._name);
								this.OnSaveEvent(ZipProgressEventType.Saving_AfterRenameTempArchive);
							}
							else
							{
								File.Move(this._temporaryFileName, this._name);
							}
							this._fileAlreadyExists = true;
						}
						this.OnSaveCompleted();
						this._JustSaved = true;
					}
				}
			}
			finally
			{
				this.CleanupAfterSaveOperation();
			}
		}

		// Token: 0x0600015A RID: 346 RVA: 0x0000AEE0 File Offset: 0x000090E0
		private void RemoveTempFile()
		{
			try
			{
				if (File.Exists(this._temporaryFileName))
				{
					File.Delete(this._temporaryFileName);
				}
			}
			catch (Exception ex)
			{
				this.StatusMessageTextWriter.WriteLine("ZipFile::Save: could not delete temp file: {0}.", ex.Message);
			}
		}

		// Token: 0x0600015B RID: 347 RVA: 0x0000AF48 File Offset: 0x00009148
		private void CleanupAfterSaveOperation()
		{
			if (this._temporaryFileName != null && this._name != null)
			{
				if (this._writestream != null)
				{
					try
					{
						this._writestream.Close();
					}
					catch
					{
					}
					try
					{
						this._writestream.Dispose();
					}
					catch
					{
					}
				}
				this._writestream = null;
				this.RemoveTempFile();
			}
		}

		// Token: 0x0600015C RID: 348 RVA: 0x0000AFF0 File Offset: 0x000091F0
		public void Save(string zipFileName)
		{
			if (this._name == null)
			{
				this._writestream = null;
			}
			this._name = zipFileName;
			if (Directory.Exists(this._name))
			{
				throw new ZipException("Bad Directory", new ArgumentException("That name specifies an existing directory. Please specify a filename.", "zipFileName"));
			}
			this._contentsChanged = true;
			this._fileAlreadyExists = File.Exists(this._name);
			this.Save();
		}

		// Token: 0x0600015D RID: 349 RVA: 0x0000B074 File Offset: 0x00009274
		public void Save(Stream outputStream)
		{
			if (!outputStream.CanWrite)
			{
				throw new ArgumentException("The outputStream must be a writable stream.");
			}
			this._name = null;
			this._writestream = new CountingStream(outputStream);
			this._contentsChanged = true;
			this._fileAlreadyExists = false;
			this.Save();
		}

		// Token: 0x0600015E RID: 350 RVA: 0x0000B0C8 File Offset: 0x000092C8
		private void WriteCentralDirectoryStructure(Stream s)
		{
			CountingStream countingStream = s as CountingStream;
			long num = (countingStream != null) ? countingStream.BytesWritten : s.Position;
			foreach (ZipEntry zipEntry in this._entries)
			{
				zipEntry.WriteCentralDirectoryEntry(s);
			}
			long num2 = (countingStream != null) ? countingStream.BytesWritten : s.Position;
			long num3 = num2 - num;
			this._NeedZip64CentralDirectory = (this._zip64 == Zip64Option.Always || this._entries.Count >= 65535 || num3 > (long)(-1) || num > (long)(-1));
			if (this._NeedZip64CentralDirectory)
			{
				if (this._zip64 == Zip64Option.Default)
				{
					throw new ZipException("The archive requires a ZIP64 Central Directory. Consider setting the UseZip64WhenSaving property.");
				}
				this.WriteZip64EndOfCentralDirectory(s, num, num2);
			}
			this.WriteCentralDirectoryFooter(s, num, num2);
		}

		// Token: 0x0600015F RID: 351 RVA: 0x0000B1F0 File Offset: 0x000093F0
		private void WriteZip64EndOfCentralDirectory(Stream s, long StartOfCentralDirectory, long EndOfCentralDirectory)
		{
			int num = 76;
			byte[] array = new byte[num];
			int num2 = 0;
			array[num2++] = 80;
			array[num2++] = 75;
			array[num2++] = 6;
			array[num2++] = 6;
			long value = 44L;
			Array.Copy(BitConverter.GetBytes(value), 0, array, num2, 8);
			num2 += 8;
			array[num2++] = 45;
			array[num2++] = 0;
			array[num2++] = 45;
			array[num2++] = 0;
			for (int i = 0; i < 8; i++)
			{
				array[num2++] = 0;
			}
			long value2 = (long)this._entries.Count;
			Array.Copy(BitConverter.GetBytes(value2), 0, array, num2, 8);
			num2 += 8;
			Array.Copy(BitConverter.GetBytes(value2), 0, array, num2, 8);
			num2 += 8;
			long value3 = EndOfCentralDirectory - StartOfCentralDirectory;
			Array.Copy(BitConverter.GetBytes(value3), 0, array, num2, 8);
			num2 += 8;
			Array.Copy(BitConverter.GetBytes(StartOfCentralDirectory), 0, array, num2, 8);
			num2 += 8;
			array[num2++] = 80;
			array[num2++] = 75;
			array[num2++] = 6;
			array[num2++] = 7;
			array[num2++] = 0;
			array[num2++] = 0;
			array[num2++] = 0;
			array[num2++] = 0;
			Array.Copy(BitConverter.GetBytes(EndOfCentralDirectory), 0, array, num2, 8);
			num2 += 8;
			array[num2++] = 1;
			array[num2++] = 0;
			array[num2++] = 0;
			array[num2++] = 0;
			s.Write(array, 0, num2);
		}

		// Token: 0x06000160 RID: 352 RVA: 0x0000B374 File Offset: 0x00009574
		private void WriteCentralDirectoryFooter(Stream s, long StartOfCentralDirectory, long EndOfCentralDirectory)
		{
			int num = 24;
			byte[] array = null;
			short num2 = 0;
			if (this.Comment != null && this.Comment.Length != 0)
			{
				array = this.ProvisionalAlternateEncoding.GetBytes(this.Comment);
				num2 = (short)array.Length;
			}
			num += (int)num2;
			byte[] array2 = new byte[num];
			int num3 = 0;
			array2[num3++] = 80;
			array2[num3++] = 75;
			array2[num3++] = 5;
			array2[num3++] = 6;
			array2[num3++] = 0;
			array2[num3++] = 0;
			array2[num3++] = 0;
			array2[num3++] = 0;
			if (this._entries.Count >= 65535 || this._zip64 == Zip64Option.Always)
			{
				for (int i = 0; i < 4; i++)
				{
					array2[num3++] = byte.MaxValue;
				}
			}
			else
			{
				array2[num3++] = (byte)(this._entries.Count & 255);
				array2[num3++] = (byte)((this._entries.Count & 65280) >> 8);
				array2[num3++] = (byte)(this._entries.Count & 255);
				array2[num3++] = (byte)((this._entries.Count & 65280) >> 8);
			}
			long num4 = EndOfCentralDirectory - StartOfCentralDirectory;
			if (num4 >= (long)(-1) || StartOfCentralDirectory >= (long)(-1))
			{
				for (int i = 0; i < 8; i++)
				{
					array2[num3++] = byte.MaxValue;
				}
			}
			else
			{
				array2[num3++] = (byte)(num4 & 255L);
				array2[num3++] = (byte)((num4 & 65280L) >> 8);
				array2[num3++] = (byte)((num4 & 16711680L) >> 16);
				array2[num3++] = (byte)((num4 & (long)(-16777216)) >> 24);
				array2[num3++] = (byte)(StartOfCentralDirectory & 255L);
				array2[num3++] = (byte)((StartOfCentralDirectory & 65280L) >> 8);
				array2[num3++] = (byte)((StartOfCentralDirectory & 16711680L) >> 16);
				array2[num3++] = (byte)((StartOfCentralDirectory & (long)(-16777216)) >> 24);
			}
			if (this.Comment == null || this.Comment.Length == 0)
			{
				array2[num3++] = 0;
				array2[num3++] = 0;
			}
			else
			{
				if ((int)num2 + num3 + 2 > array2.Length)
				{
					num2 = (short)(array2.Length - num3 - 2);
				}
				array2[num3++] = (byte)(num2 & 255);
				array2[num3++] = (byte)(((int)num2 & 65280) >> 8);
				if (num2 != 0)
				{
					int i = 0;
					while (i < (int)num2 && num3 + i < array2.Length)
					{
						array2[num3 + i] = array[i];
						i++;
					}
					num3 += i;
				}
			}
			s.Write(array2, 0, num3);
		}

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x06000161 RID: 353 RVA: 0x0000B6F0 File Offset: 0x000098F0
		private string ArchiveNameForEvent
		{
			get
			{
				return (this._name != null) ? this._name : "(stream)";
			}
		}

		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000162 RID: 354 RVA: 0x0000B724 File Offset: 0x00009924
		// (remove) Token: 0x06000163 RID: 355 RVA: 0x0000B740 File Offset: 0x00009940
		public event EventHandler<SaveProgressEventArgs> SaveProgress;

		// Token: 0x06000164 RID: 356 RVA: 0x0000B75C File Offset: 0x0000995C
		internal bool OnSaveBlock(ZipEntry entry, long bytesXferred, long totalBytesToXfer)
		{
			if (this.SaveProgress != null)
			{
				lock (this.LOCK)
				{
					SaveProgressEventArgs saveProgressEventArgs = SaveProgressEventArgs.ByteUpdate(this.ArchiveNameForEvent, entry, bytesXferred, totalBytesToXfer);
					this.SaveProgress(this, saveProgressEventArgs);
					if (saveProgressEventArgs.Cancel)
					{
						this._saveOperationCanceled = true;
					}
				}
			}
			return this._saveOperationCanceled;
		}

		// Token: 0x06000165 RID: 357 RVA: 0x0000B7EC File Offset: 0x000099EC
		private void OnSaveEntry(int current, ZipEntry entry, bool before)
		{
			if (this.SaveProgress != null)
			{
				lock (this.LOCK)
				{
					SaveProgressEventArgs saveProgressEventArgs = new SaveProgressEventArgs(this.ArchiveNameForEvent, before, this._entries.Count, current, entry);
					this.SaveProgress(this, saveProgressEventArgs);
					if (saveProgressEventArgs.Cancel)
					{
						this._saveOperationCanceled = true;
					}
				}
			}
		}

		// Token: 0x06000166 RID: 358 RVA: 0x0000B87C File Offset: 0x00009A7C
		private void OnSaveEvent(ZipProgressEventType eventFlavor)
		{
			if (this.SaveProgress != null)
			{
				lock (this.LOCK)
				{
					SaveProgressEventArgs saveProgressEventArgs = new SaveProgressEventArgs(this.ArchiveNameForEvent, eventFlavor);
					this.SaveProgress(this, saveProgressEventArgs);
					if (saveProgressEventArgs.Cancel)
					{
						this._saveOperationCanceled = true;
					}
				}
			}
		}

		// Token: 0x06000167 RID: 359 RVA: 0x0000B8FC File Offset: 0x00009AFC
		private void OnSaveStarted()
		{
			if (this.SaveProgress != null)
			{
				lock (this.LOCK)
				{
					SaveProgressEventArgs e = SaveProgressEventArgs.Started(this.ArchiveNameForEvent);
					this.SaveProgress(this, e);
				}
			}
		}

		// Token: 0x06000168 RID: 360 RVA: 0x0000B964 File Offset: 0x00009B64
		private void OnSaveCompleted()
		{
			if (this.SaveProgress != null)
			{
				lock (this.LOCK)
				{
					SaveProgressEventArgs e = SaveProgressEventArgs.Completed(this.ArchiveNameForEvent);
					this.SaveProgress(this, e);
				}
			}
		}

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x06000169 RID: 361 RVA: 0x0000B9CC File Offset: 0x00009BCC
		// (remove) Token: 0x0600016A RID: 362 RVA: 0x0000B9E8 File Offset: 0x00009BE8
		public event EventHandler<ReadProgressEventArgs> ReadProgress;

		// Token: 0x0600016B RID: 363 RVA: 0x0000BA04 File Offset: 0x00009C04
		private void OnReadStarted()
		{
			if (this.ReadProgress != null)
			{
				lock (this.LOCK)
				{
					ReadProgressEventArgs e = ReadProgressEventArgs.Started(this.ArchiveNameForEvent);
					this.ReadProgress(this, e);
				}
			}
		}

		// Token: 0x0600016C RID: 364 RVA: 0x0000BA6C File Offset: 0x00009C6C
		private void OnReadCompleted()
		{
			if (this.ReadProgress != null)
			{
				lock (this.LOCK)
				{
					ReadProgressEventArgs e = ReadProgressEventArgs.Completed(this.ArchiveNameForEvent);
					this.ReadProgress(this, e);
				}
			}
		}

		// Token: 0x0600016D RID: 365 RVA: 0x0000BAD4 File Offset: 0x00009CD4
		internal void OnReadBytes(ZipEntry entry)
		{
			if (this.ReadProgress != null)
			{
				lock (this.LOCK)
				{
					ReadProgressEventArgs e = ReadProgressEventArgs.ByteUpdate(this.ArchiveNameForEvent, entry, this.ReadStream.Position, this.LengthOfReadStream);
					this.ReadProgress(this, e);
				}
			}
		}

		// Token: 0x0600016E RID: 366 RVA: 0x0000BB50 File Offset: 0x00009D50
		internal void OnReadEntry(bool before, ZipEntry entry)
		{
			if (this.ReadProgress != null)
			{
				lock (this.LOCK)
				{
					ReadProgressEventArgs e = before ? ReadProgressEventArgs.Before(this.ArchiveNameForEvent, this._entries.Count) : ReadProgressEventArgs.After(this.ArchiveNameForEvent, entry, this._entries.Count);
					this.ReadProgress(this, e);
				}
			}
		}

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x0600016F RID: 367 RVA: 0x0000BBE8 File Offset: 0x00009DE8
		private long LengthOfReadStream
		{
			get
			{
				if (this._lengthOfReadStream == -99L)
				{
					if (this._ReadStreamIsOurs)
					{
						FileInfo fileInfo = new FileInfo(this._name);
						this._lengthOfReadStream = fileInfo.Length;
					}
					else
					{
						this._lengthOfReadStream = -1L;
					}
				}
				return this._lengthOfReadStream;
			}
		}

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x06000170 RID: 368 RVA: 0x0000BC58 File Offset: 0x00009E58
		// (remove) Token: 0x06000171 RID: 369 RVA: 0x0000BC74 File Offset: 0x00009E74
		public event EventHandler<ExtractProgressEventArgs> ExtractProgress;

		// Token: 0x06000172 RID: 370 RVA: 0x0000BC90 File Offset: 0x00009E90
		private void OnExtractEntry(int current, bool before, ZipEntry currentEntry, string path, bool overwrite)
		{
			if (this.ExtractProgress != null)
			{
				lock (this.LOCK)
				{
					ExtractProgressEventArgs extractProgressEventArgs = new ExtractProgressEventArgs(this.ArchiveNameForEvent, before, this._entries.Count, current, currentEntry, path, overwrite);
					this.ExtractProgress(this, extractProgressEventArgs);
					if (extractProgressEventArgs.Cancel)
					{
						this._extractOperationCanceled = true;
					}
				}
			}
		}

		// Token: 0x06000173 RID: 371 RVA: 0x0000BD24 File Offset: 0x00009F24
		internal bool OnExtractBlock(ZipEntry entry, long bytesWritten, long totalBytesToWrite)
		{
			if (this.ExtractProgress != null)
			{
				lock (this.LOCK)
				{
					ExtractProgressEventArgs extractProgressEventArgs = ExtractProgressEventArgs.ByteUpdate(this.ArchiveNameForEvent, entry, bytesWritten, totalBytesToWrite);
					this.ExtractProgress(this, extractProgressEventArgs);
					if (extractProgressEventArgs.Cancel)
					{
						this._extractOperationCanceled = true;
					}
				}
			}
			return this._extractOperationCanceled;
		}

		// Token: 0x06000174 RID: 372 RVA: 0x0000BDB4 File Offset: 0x00009FB4
		internal bool OnSingleEntryExtract(ZipEntry entry, string path, bool before, bool overwrite)
		{
			if (this.ExtractProgress != null)
			{
				lock (this.LOCK)
				{
					ExtractProgressEventArgs extractProgressEventArgs = before ? ExtractProgressEventArgs.BeforeExtractEntry(this.ArchiveNameForEvent, entry, path, overwrite) : ExtractProgressEventArgs.AfterExtractEntry(this.ArchiveNameForEvent, entry, path, overwrite);
					this.ExtractProgress(this, extractProgressEventArgs);
					if (extractProgressEventArgs.Cancel)
					{
						this._extractOperationCanceled = true;
					}
				}
			}
			return this._extractOperationCanceled;
		}

		// Token: 0x06000175 RID: 373 RVA: 0x0000BE60 File Offset: 0x0000A060
		private void OnExtractAllCompleted(string path, bool wantOverwrite)
		{
			if (this.ExtractProgress != null)
			{
				lock (this.LOCK)
				{
					ExtractProgressEventArgs e = ExtractProgressEventArgs.ExtractAllCompleted(this.ArchiveNameForEvent, path, wantOverwrite);
					this.ExtractProgress(this, e);
				}
			}
		}

		// Token: 0x06000176 RID: 374 RVA: 0x0000BECC File Offset: 0x0000A0CC
		private void OnExtractAllStarted(string path, bool wantOverwrite)
		{
			if (this.ExtractProgress != null)
			{
				lock (this.LOCK)
				{
					ExtractProgressEventArgs e = ExtractProgressEventArgs.ExtractAllStarted(this.ArchiveNameForEvent, path, wantOverwrite);
					this.ExtractProgress(this, e);
				}
			}
		}

		// Token: 0x06000177 RID: 375 RVA: 0x0000BF38 File Offset: 0x0000A138
		public static bool IsZipFile(string fileName)
		{
			return ZipFile.IsZipFile(fileName, false);
		}

		// Token: 0x06000178 RID: 376 RVA: 0x0000BF58 File Offset: 0x0000A158
		public static bool IsZipFile(string fileName, bool testExtract)
		{
			bool result = false;
			try
			{
				if (!File.Exists(fileName))
				{
					return false;
				}
				Stream @null = Stream.Null;
				using (ZipFile zipFile = ZipFile.Read(fileName, null, Encoding.GetEncoding("IBM437")))
				{
					if (testExtract)
					{
						foreach (ZipEntry zipEntry in zipFile)
						{
							if (!zipEntry.IsDirectory)
							{
								zipEntry.Extract(@null);
							}
						}
					}
				}
				result = true;
			}
			catch (Exception)
			{
			}
			return result;
		}

		// Token: 0x06000179 RID: 377 RVA: 0x0000C05C File Offset: 0x0000A25C
		public static ZipFile Read(string zipFileName)
		{
			return ZipFile.Read(zipFileName, null, ZipFile.DefaultEncoding);
		}

		// Token: 0x0600017A RID: 378 RVA: 0x0000C084 File Offset: 0x0000A284
		public static ZipFile Read(string zipFileName, EventHandler<ReadProgressEventArgs> readProgress)
		{
			return ZipFile.Read(zipFileName, null, ZipFile.DefaultEncoding, readProgress);
		}

		// Token: 0x0600017B RID: 379 RVA: 0x0000C0AC File Offset: 0x0000A2AC
		public static ZipFile Read(string zipFileName, TextWriter statusMessageWriter)
		{
			return ZipFile.Read(zipFileName, statusMessageWriter, ZipFile.DefaultEncoding);
		}

		// Token: 0x0600017C RID: 380 RVA: 0x0000C0D4 File Offset: 0x0000A2D4
		public static ZipFile Read(string zipFileName, TextWriter statusMessageWriter, EventHandler<ReadProgressEventArgs> readProgress)
		{
			return ZipFile.Read(zipFileName, statusMessageWriter, ZipFile.DefaultEncoding, readProgress);
		}

		// Token: 0x0600017D RID: 381 RVA: 0x0000C0FC File Offset: 0x0000A2FC
		public static ZipFile Read(string zipFileName, Encoding encoding)
		{
			return ZipFile.Read(zipFileName, null, encoding);
		}

		// Token: 0x0600017E RID: 382 RVA: 0x0000C120 File Offset: 0x0000A320
		public static ZipFile Read(string zipFileName, Encoding encoding, EventHandler<ReadProgressEventArgs> readProgress)
		{
			return ZipFile.Read(zipFileName, null, encoding, readProgress);
		}

		// Token: 0x0600017F RID: 383 RVA: 0x0000C144 File Offset: 0x0000A344
		public static ZipFile Read(string zipFileName, TextWriter statusMessageWriter, Encoding encoding)
		{
			return ZipFile.Read(zipFileName, statusMessageWriter, encoding, null);
		}

		// Token: 0x06000180 RID: 384 RVA: 0x0000C168 File Offset: 0x0000A368
		public static ZipFile Read(string zipFileName, TextWriter statusMessageWriter, Encoding encoding, EventHandler<ReadProgressEventArgs> readProgress)
		{
			ZipFile zipFile = new ZipFile();
			zipFile.ProvisionalAlternateEncoding = encoding;
			zipFile._StatusMessageTextWriter = statusMessageWriter;
			zipFile._name = zipFileName;
			if (readProgress != null)
			{
				zipFile.ReadProgress = readProgress;
			}
			try
			{
				ZipFile.ReadIntoInstance(zipFile);
				zipFile._fileAlreadyExists = true;
			}
			catch (Exception innerException)
			{
				throw new ZipException(string.Format("{0} is not a valid zip file", zipFileName), innerException);
			}
			return zipFile;
		}

		// Token: 0x06000181 RID: 385 RVA: 0x0000C1E8 File Offset: 0x0000A3E8
		public static ZipFile Read(Stream zipStream)
		{
			return ZipFile.Read(zipStream, null, ZipFile.DefaultEncoding);
		}

		// Token: 0x06000182 RID: 386 RVA: 0x0000C210 File Offset: 0x0000A410
		public static ZipFile Read(Stream zipStream, EventHandler<ReadProgressEventArgs> readProgress)
		{
			return ZipFile.Read(zipStream, null, ZipFile.DefaultEncoding, readProgress);
		}

		// Token: 0x06000183 RID: 387 RVA: 0x0000C238 File Offset: 0x0000A438
		public static ZipFile Read(Stream zipStream, TextWriter statusMessageWriter)
		{
			return ZipFile.Read(zipStream, statusMessageWriter, ZipFile.DefaultEncoding);
		}

		// Token: 0x06000184 RID: 388 RVA: 0x0000C260 File Offset: 0x0000A460
		public static ZipFile Read(Stream zipStream, TextWriter statusMessageWriter, EventHandler<ReadProgressEventArgs> readProgress)
		{
			return ZipFile.Read(zipStream, statusMessageWriter, ZipFile.DefaultEncoding, readProgress);
		}

		// Token: 0x06000185 RID: 389 RVA: 0x0000C288 File Offset: 0x0000A488
		public static ZipFile Read(Stream zipStream, Encoding encoding)
		{
			return ZipFile.Read(zipStream, null, encoding);
		}

		// Token: 0x06000186 RID: 390 RVA: 0x0000C2AC File Offset: 0x0000A4AC
		public static ZipFile Read(Stream zipStream, Encoding encoding, EventHandler<ReadProgressEventArgs> readProgress)
		{
			return ZipFile.Read(zipStream, null, encoding, readProgress);
		}

		// Token: 0x06000187 RID: 391 RVA: 0x0000C2D0 File Offset: 0x0000A4D0
		public static ZipFile Read(Stream zipStream, TextWriter statusMessageWriter, Encoding encoding)
		{
			return ZipFile.Read(zipStream, statusMessageWriter, encoding, null);
		}

		// Token: 0x06000188 RID: 392 RVA: 0x0000C2F4 File Offset: 0x0000A4F4
		public static ZipFile Read(Stream zipStream, TextWriter statusMessageWriter, Encoding encoding, EventHandler<ReadProgressEventArgs> readProgress)
		{
			if (zipStream == null)
			{
				throw new ZipException("Cannot read.", new ArgumentException("The stream must be non-null", "zipStream"));
			}
			ZipFile zipFile = new ZipFile();
			zipFile._provisionalAlternateEncoding = encoding;
			if (readProgress != null)
			{
				ZipFile zipFile2 = zipFile;
				zipFile2.ReadProgress = (EventHandler<ReadProgressEventArgs>)Delegate.Combine(zipFile2.ReadProgress, readProgress);
			}
			zipFile._StatusMessageTextWriter = statusMessageWriter;
			zipFile._readstream = zipStream;
			zipFile._ReadStreamIsOurs = false;
			ZipFile.ReadIntoInstance(zipFile);
			return zipFile;
		}

		// Token: 0x06000189 RID: 393 RVA: 0x0000C384 File Offset: 0x0000A584
		public static ZipFile Read(byte[] buffer)
		{
			return ZipFile.Read(buffer, null, ZipFile.DefaultEncoding);
		}

		// Token: 0x0600018A RID: 394 RVA: 0x0000C3AC File Offset: 0x0000A5AC
		public static ZipFile Read(byte[] buffer, TextWriter statusMessageWriter)
		{
			return ZipFile.Read(buffer, statusMessageWriter, ZipFile.DefaultEncoding);
		}

		// Token: 0x0600018B RID: 395 RVA: 0x0000C3D4 File Offset: 0x0000A5D4
		public static ZipFile Read(byte[] buffer, TextWriter statusMessageWriter, Encoding encoding)
		{
			ZipFile zipFile = new ZipFile();
			zipFile._StatusMessageTextWriter = statusMessageWriter;
			zipFile._provisionalAlternateEncoding = encoding;
			zipFile._readstream = new MemoryStream(buffer);
			zipFile._ReadStreamIsOurs = true;
			ZipFile.ReadIntoInstance(zipFile);
			return zipFile;
		}

		// Token: 0x0600018C RID: 396 RVA: 0x0000C41C File Offset: 0x0000A61C
		private static void ReadIntoInstance(ZipFile zf)
		{
			Stream readStream = zf.ReadStream;
			try
			{
				if (!readStream.CanSeek)
				{
					ZipFile.ReadIntoInstance_Orig(zf);
					return;
				}
				long position = readStream.Position;
				uint num = ZipFile.VerifyBeginningOfZipFile(readStream);
				if (num == 101010256U)
				{
					return;
				}
				int num2 = 0;
				bool flag = false;
				long num3 = readStream.Length - 64L;
				long num4 = Math.Max(readStream.Length - 16384L, 10L);
				do
				{
					readStream.Seek(num3, SeekOrigin.Begin);
					long num5 = SharedUtilities.FindSignature(readStream, 101010256);
					if (num5 != -1L)
					{
						flag = true;
					}
					else
					{
						num2++;
						num3 -= (long)(32 * (num2 + 1) * num2);
						if (num3 < 0L)
						{
							num3 = 0L;
						}
					}
				}
				while (!flag && num3 > num4);
				if (flag)
				{
					byte[] array = new byte[16];
					zf.ReadStream.Read(array, 0, array.Length);
					int num6 = 12;
					uint num7 = (uint)array[num6++] + (uint)array[num6++] * 256U + (uint)array[num6++] * 256U * 256U + (uint)array[num6++] * 256U * 256U * 256U;
					if (num7 == 4294967295U)
					{
						ZipFile.Zip64SeekToCentralDirectory(readStream);
					}
					else
					{
						readStream.Seek((long)((ulong)num7), SeekOrigin.Begin);
					}
					ZipFile.ReadCentralDirectory(zf);
				}
				else
				{
					readStream.Seek(position, SeekOrigin.Begin);
					ZipFile.ReadIntoInstance_Orig(zf);
				}
			}
			catch
			{
				if (zf._ReadStreamIsOurs && zf._readstream != null)
				{
					try
					{
						zf._readstream.Close();
						zf._readstream.Dispose();
						zf._readstream = null;
					}
					finally
					{
					}
				}
				throw;
			}
			zf._contentsChanged = false;
		}

		// Token: 0x0600018D RID: 397 RVA: 0x0000C668 File Offset: 0x0000A868
		private static void Zip64SeekToCentralDirectory(Stream s)
		{
			byte[] array = new byte[16];
			s.Seek(-40L, SeekOrigin.Current);
			s.Read(array, 0, 16);
			long offset = BitConverter.ToInt64(array, 8);
			s.Seek(offset, SeekOrigin.Begin);
			uint num = (uint)SharedUtilities.ReadInt(s);
			if (num != 101075792U)
			{
				throw new BadReadException(string.Format("  ZipFile::Read(): Bad signature (0x{0:X8}) looking for ZIP64 EoCD Record at position 0x{1:X8}", num, s.Position));
			}
			s.Read(array, 0, 8);
			long num2 = BitConverter.ToInt64(array, 0);
			array = new byte[num2];
			s.Read(array, 0, array.Length);
			offset = BitConverter.ToInt64(array, 36);
			s.Seek(offset, SeekOrigin.Begin);
		}

		// Token: 0x0600018E RID: 398 RVA: 0x0000C71C File Offset: 0x0000A91C
		private static uint VerifyBeginningOfZipFile(Stream s)
		{
			uint num = (uint)SharedUtilities.ReadInt(s);
			if (num != 808471376U && num != 134695760U && num != 67324752U && num != 101010256U && (num & 65535U) != 23117U)
			{
				throw new BadReadException(string.Format("  ZipFile::Read(): Bad signature (0x{0:X8}) at start of file at position 0x{1:X8}", num, s.Position));
			}
			return num;
		}

		// Token: 0x0600018F RID: 399 RVA: 0x0000C7A8 File Offset: 0x0000A9A8
		private static void ReadCentralDirectory(ZipFile zf)
		{
			zf._entries = new List<ZipEntry>();
			ZipEntry zipEntry;
			while ((zipEntry = ZipEntry.ReadDirEntry(zf.ReadStream, zf.ProvisionalAlternateEncoding)) != null)
			{
				zipEntry.ResetDirEntry();
				zipEntry._zipfile = zf;
				zipEntry._Source = EntrySource.Zipfile;
				zipEntry._archiveStream = zf.ReadStream;
				zf.OnReadEntry(true, null);
				if (zf.Verbose)
				{
					zf.StatusMessageTextWriter.WriteLine("  {0}", zipEntry.FileName);
				}
				zf._entries.Add(zipEntry);
			}
			ZipFile.ReadCentralDirectoryFooter(zf);
			if (zf.Verbose && !string.IsNullOrEmpty(zf.Comment))
			{
				zf.StatusMessageTextWriter.WriteLine("Zip file Comment: {0}", zf.Comment);
			}
			zf.OnReadCompleted();
		}

		// Token: 0x06000190 RID: 400 RVA: 0x0000C890 File Offset: 0x0000AA90
		private static void ReadIntoInstance_Orig(ZipFile zf)
		{
			zf.OnReadStarted();
			zf._entries = new List<ZipEntry>();
			if (zf.Verbose)
			{
				if (zf.Name == null)
				{
					zf.StatusMessageTextWriter.WriteLine("Reading zip from stream...");
				}
				else
				{
					zf.StatusMessageTextWriter.WriteLine("Reading zip {0}...", zf.Name);
				}
			}
			bool first = true;
			ZipEntry zipEntry;
			while ((zipEntry = ZipEntry.Read(zf, first)) != null)
			{
				if (zf.Verbose)
				{
					zf.StatusMessageTextWriter.WriteLine("  {0}", zipEntry.FileName);
				}
				zf._entries.Add(zipEntry);
				first = false;
			}
			ZipEntry zipEntry2;
			while ((zipEntry2 = ZipEntry.ReadDirEntry(zf.ReadStream, zf.ProvisionalAlternateEncoding)) != null)
			{
				foreach (ZipEntry zipEntry3 in zf._entries)
				{
					if (zipEntry3.FileName == zipEntry2.FileName)
					{
						zipEntry3._Comment = zipEntry2.Comment;
						if (zipEntry2.AttributesIndicateDirectory)
						{
							zipEntry3.MarkAsDirectory();
						}
						break;
					}
				}
			}
			ZipFile.ReadCentralDirectoryFooter(zf);
			if (zf.Verbose && !string.IsNullOrEmpty(zf.Comment))
			{
				zf.StatusMessageTextWriter.WriteLine("Zip file Comment: {0}", zf.Comment);
			}
			zf.OnReadCompleted();
		}

		// Token: 0x06000191 RID: 401 RVA: 0x0000CA5C File Offset: 0x0000AC5C
		private static void ReadCentralDirectoryFooter(ZipFile zf)
		{
			Stream readStream = zf.ReadStream;
			int num = SharedUtilities.ReadSignature(readStream);
			byte[] array;
			if ((long)num == 101075792L)
			{
				array = new byte[52];
				readStream.Read(array, 0, array.Length);
				long num2 = BitConverter.ToInt64(array, 0);
				if (num2 < 44L)
				{
					throw new ZipException("Bad DataSize in the ZIP64 Central Directory.");
				}
				int num3 = 8;
				num3 += 2;
				num3 += 2;
				num3 += 4;
				num3 += 4;
				num3 += 8;
				num3 += 8;
				num3 += 8;
				num3 += 8;
				array = new byte[num2 - 44L];
				readStream.Read(array, 0, array.Length);
				num = SharedUtilities.ReadSignature(readStream);
				if ((long)num != 117853008L)
				{
					throw new ZipException("Inconsistent metadata in the ZIP64 Central Directory.");
				}
				array = new byte[16];
				readStream.Read(array, 0, array.Length);
				num = SharedUtilities.ReadSignature(readStream);
			}
			if ((long)num != 101010256L)
			{
				readStream.Seek(-4L, SeekOrigin.Current);
				throw new BadReadException(string.Format("  ZipFile::Read(): Bad signature ({0:X8}) at position 0x{1:X8}", num, readStream.Position));
			}
			array = new byte[16];
			zf.ReadStream.Read(array, 0, array.Length);
			ZipFile.ReadZipFileComment(zf);
		}

		// Token: 0x06000192 RID: 402 RVA: 0x0000CBB0 File Offset: 0x0000ADB0
		private static void ReadZipFileComment(ZipFile zf)
		{
			byte[] array = new byte[2];
			zf.ReadStream.Read(array, 0, array.Length);
			short num = (short)((int)array[0] + (int)array[1] * 256);
			if (num > 0)
			{
				array = new byte[(int)num];
				zf.ReadStream.Read(array, 0, array.Length);
				string @string = ZipFile.DefaultEncoding.GetString(array, 0, array.Length);
				byte[] bytes = ZipFile.DefaultEncoding.GetBytes(@string);
				if (ZipFile.BlocksAreEqual(array, bytes))
				{
					zf.Comment = @string;
				}
				else
				{
					Encoding encoding = (zf._provisionalAlternateEncoding.CodePage == 437) ? Encoding.UTF8 : zf._provisionalAlternateEncoding;
					zf.Comment = encoding.GetString(array, 0, array.Length);
				}
			}
		}

		// Token: 0x06000193 RID: 403 RVA: 0x0000CC8C File Offset: 0x0000AE8C
		private static bool BlocksAreEqual(byte[] a, byte[] b)
		{
			bool result;
			if (a.Length != b.Length)
			{
				result = false;
			}
			else
			{
				for (int i = 0; i < a.Length; i++)
				{
					if (a[i] != b[i])
					{
						return false;
					}
				}
				result = true;
			}
			return result;
		}

		// Token: 0x06000194 RID: 404 RVA: 0x0000CCEC File Offset: 0x0000AEEC
		public IEnumerator<ZipEntry> GetEnumerator()
		{
			foreach (ZipEntry e in this._entries)
			{
				yield return e;
			}
			yield break;
		}

		// Token: 0x06000195 RID: 405 RVA: 0x0000CD14 File Offset: 0x0000AF14
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x06000196 RID: 406 RVA: 0x0000CD34 File Offset: 0x0000AF34
		public void ExtractAll(string path)
		{
			this.ExtractAll(path, false);
		}

		// Token: 0x06000197 RID: 407 RVA: 0x0000CD40 File Offset: 0x0000AF40
		public async Task<bool>ExtractAll(string path, bool wantOverwrite)
		{
			bool flag = this.Verbose;
			this._inExtractAll = true;
			try
			{
				this.OnExtractAllStarted(path, wantOverwrite);
				int num = 0;
				foreach (ZipEntry zipEntry in this._entries)
				{
					if (flag)
					{
						this.StatusMessageTextWriter.WriteLine("\n{1,-22} {2,-8} {3,4}   {4,-8}  {0}", new object[]
						{
							"Name",
							"Modified",
							"Size",
							"Ratio",
							"Packed"
						});
						this.StatusMessageTextWriter.WriteLine(new string('-', 72));
						flag = false;
					}
					if (this.Verbose)
					{
						this.StatusMessageTextWriter.WriteLine("{1,-22} {2,-8} {3,4:F0}%   {4,-8} {0}", new object[]
						{
							zipEntry.FileName,
							zipEntry.LastModified.ToString("yyyy-MM-dd HH:mm:ss"),
							zipEntry.UncompressedSize,
							zipEntry.CompressionRatio,
							zipEntry.CompressedSize
						});
						if (!string.IsNullOrEmpty(zipEntry.Comment))
						{
							this.StatusMessageTextWriter.WriteLine("  Comment: {0}", zipEntry.Comment);
						}
					}
					zipEntry.Password = this._Password;
					this.OnExtractEntry(num, true, zipEntry, path, wantOverwrite);
					zipEntry.Extract(path, wantOverwrite);
					num++;
					this.OnExtractEntry(num, false, zipEntry, path, wantOverwrite);
					if (this._extractOperationCanceled)
					{
						break;
					}
				}
				this.OnExtractAllCompleted(path, wantOverwrite);
			}
			finally
			{
				this._inExtractAll = false;
			}
			return true;
		}

		// Token: 0x06000198 RID: 408 RVA: 0x0000CF58 File Offset: 0x0000B158
		public void Extract(string fileName)
		{
			ZipEntry zipEntry = this[fileName];
			zipEntry.Password = this._Password;
			zipEntry.Extract();
		}

		// Token: 0x06000199 RID: 409 RVA: 0x0000CF88 File Offset: 0x0000B188
		public void Extract(string fileName, string directoryName)
		{
			ZipEntry zipEntry = this[fileName];
			zipEntry.Password = this._Password;
			zipEntry.Extract(directoryName);
		}

		// Token: 0x0600019A RID: 410 RVA: 0x0000CFB8 File Offset: 0x0000B1B8
		public void Extract(string fileName, bool wantOverwrite)
		{
			ZipEntry zipEntry = this[fileName];
			zipEntry.Password = this._Password;
			zipEntry.Extract(wantOverwrite);
		}

		// Token: 0x0600019B RID: 411 RVA: 0x0000CFE8 File Offset: 0x0000B1E8
		public void Extract(string fileName, string directoryName, bool wantOverwrite)
		{
			ZipEntry zipEntry = this[fileName];
			zipEntry.Password = this._Password;
			zipEntry.Extract(directoryName, wantOverwrite);
		}

		// Token: 0x0600019C RID: 412 RVA: 0x0000D018 File Offset: 0x0000B218
		public void Extract(string fileName, Stream outputStream)
		{
			if (outputStream == null || !outputStream.CanWrite)
			{
				throw new ZipException("Cannot extract.", new ArgumentException("The OutputStream must be a writable stream.", "outputStream"));
			}
			if (string.IsNullOrEmpty(fileName))
			{
				throw new ZipException("Cannot extract.", new ArgumentException("The file name must be neither null nor empty.", "fileName"));
			}
			ZipEntry zipEntry = this[fileName];
			zipEntry.Password = this._Password;
			zipEntry.Extract(outputStream);
		}

		// Token: 0x17000061 RID: 97
		public ZipEntry this[int ix]
		{
			get
			{
				return this._entries[ix];
			}
			set
			{
				if (value != null)
				{
					throw new ArgumentException("You may not set this to a non-null ZipEntry value.");
				}
				this.RemoveEntry(this._entries[ix]);
			}
		}

		// Token: 0x17000062 RID: 98
		public ZipEntry this[string fileName]
		{
			get
			{
				foreach (ZipEntry zipEntry in this._entries)
				{
					if (this.CaseSensitiveRetrieval)
					{
						if (zipEntry.FileName == fileName)
						{
							return zipEntry;
						}
						if (fileName.Replace("\\", "/") == zipEntry.FileName)
						{
							return zipEntry;
						}
						if (zipEntry.FileName.Replace("\\", "/") == fileName)
						{
							return zipEntry;
						}
						if (zipEntry.FileName.EndsWith("/"))
						{
							string text = zipEntry.FileName.Trim("/".ToCharArray());
							if (text == fileName)
							{
								return zipEntry;
							}
							if (fileName.Replace("\\", "/") == text)
							{
								return zipEntry;
							}
							if (text.Replace("\\", "/") == fileName)
							{
								return zipEntry;
							}
						}
					}
					else
					{
						if (string.Compare(zipEntry.FileName, fileName, StringComparison.CurrentCultureIgnoreCase) == 0)
						{
							return zipEntry;
						}
						if (string.Compare(fileName.Replace("\\", "/"), zipEntry.FileName, StringComparison.CurrentCultureIgnoreCase) == 0)
						{
							return zipEntry;
						}
						if (string.Compare(zipEntry.FileName.Replace("\\", "/"), fileName, StringComparison.CurrentCultureIgnoreCase) == 0)
						{
							return zipEntry;
						}
						if (zipEntry.FileName.EndsWith("/"))
						{
							string text = zipEntry.FileName.Trim("/".ToCharArray());
							if (string.Compare(text, fileName, StringComparison.CurrentCultureIgnoreCase) == 0)
							{
								return zipEntry;
							}
							if (string.Compare(fileName.Replace("\\", "/"), text, StringComparison.CurrentCultureIgnoreCase) == 0)
							{
								return zipEntry;
							}
							if (string.Compare(text.Replace("\\", "/"), fileName, StringComparison.CurrentCultureIgnoreCase) == 0)
							{
								return zipEntry;
							}
						}
					}
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					throw new ArgumentException("You may not set this to a non-null ZipEntry value.");
				}
				this.RemoveEntry(fileName);
			}
		}

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x060001A1 RID: 417 RVA: 0x0000D400 File Offset: 0x0000B600
		public ReadOnlyCollection<string> EntryFileNames
		{
			get
			{
				List<string> list = this._entries.ConvertAll<string>((ZipEntry e) => e.FileName);
				return list.AsReadOnly();
			}
		}

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x060001A2 RID: 418 RVA: 0x0000D450 File Offset: 0x0000B650
		public ReadOnlyCollection<ZipEntry> Entries
		{
			get
			{
				return this._entries.AsReadOnly();
			}
		}

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x060001A3 RID: 419 RVA: 0x0000D474 File Offset: 0x0000B674
		public int Count
		{
			get
			{
				return this._entries.Count;
			}
		}

		// Token: 0x060001A4 RID: 420 RVA: 0x0000D498 File Offset: 0x0000B698
		public void RemoveEntry(ZipEntry entry)
		{
			if (!this._entries.Contains(entry))
			{
				throw new ArgumentException("The entry you specified does not exist in the zip archive.");
			}
			this._entries.Remove(entry);
			this._contentsChanged = true;
		}

		// Token: 0x060001A5 RID: 421 RVA: 0x0000D4DC File Offset: 0x0000B6DC
		public void RemoveEntry(string fileName)
		{
			string fileName2 = ZipEntry.NameInArchive(fileName, null);
			ZipEntry zipEntry = this[fileName2];
			if (zipEntry == null)
			{
				throw new ArgumentException("The entry you specified was not found in the zip archive.");
			}
			this.RemoveEntry(zipEntry);
		}

		// Token: 0x060001A6 RID: 422 RVA: 0x0000D520 File Offset: 0x0000B720
		~ZipFile()
		{
			this.Dispose(false);
		}

		// Token: 0x060001A7 RID: 423 RVA: 0x0000D558 File Offset: 0x0000B758
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		// Token: 0x060001A8 RID: 424 RVA: 0x0000D56C File Offset: 0x0000B76C
		protected virtual void Dispose(bool disposeManagedResources)
		{
			if (!this._disposed)
			{
				if (disposeManagedResources)
				{
					if (this._ReadStreamIsOurs)
					{
						if (this._readstream != null)
						{
							this._readstream.Dispose();
							this._readstream = null;
						}
					}
					if (this._temporaryFileName != null && this._name != null && this._writestream != null)
					{
						this._writestream.Dispose();
						this._writestream = null;
					}
				}
				this._disposed = true;
			}
		}

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x060001A9 RID: 425 RVA: 0x0000D620 File Offset: 0x0000B820
		// (set) Token: 0x060001AA RID: 426 RVA: 0x0000D6B0 File Offset: 0x0000B8B0
		private Stream WriteStream
		{
			get
			{
				if (this._writestream == null)
				{
					if (this._name != null)
					{
						this._temporaryFileName = ((this.TempFileFolder != ".") ? Path.Combine(this.TempFileFolder, SharedUtilities.GetTempFilename()) : SharedUtilities.GetTempFilename());
						this._writestream = new FileStream(this._temporaryFileName, FileMode.CreateNew);
					}
				}
				return this._writestream;
			}
			set
			{
				if (value != null)
				{
					throw new ZipException("Whoa!", new ArgumentException("Cannot set the stream to a non-null value.", "value"));
				}
				this._writestream = null;
			}
		}

		// Token: 0x060001AB RID: 427 RVA: 0x0000D6F0 File Offset: 0x0000B8F0
		private string SfxSaveTemporary()
		{
			string text = Path.Combine(this.TempFileFolder, Path.GetRandomFileName() + ".zip");
			Stream stream = null;
			try
			{
				bool contentsChanged = this._contentsChanged;
				stream = new FileStream(text, FileMode.CreateNew);
				if (stream == null)
				{
					throw new BadStateException(string.Format("Cannot open the temporary file ({0}) for writing.", text));
				}
				if (this.Verbose)
				{
					this.StatusMessageTextWriter.WriteLine("Saving temp zip file....");
				}
				int num = 0;
				foreach (ZipEntry zipEntry in this._entries)
				{
					this.OnSaveEntry(num, zipEntry, true);
					zipEntry.Write(stream);
					num++;
					this.OnSaveEntry(num, zipEntry, false);
					if (this._saveOperationCanceled)
					{
						break;
					}
				}
				if (!this._saveOperationCanceled)
				{
					this.WriteCentralDirectoryStructure(stream);
					stream.Close();
					stream = null;
				}
				this._contentsChanged = contentsChanged;
			}
			finally
			{
				if (stream != null)
				{
					try
					{
						stream.Close();
					}
					catch
					{
					}
					try
					{
						stream.Dispose();
					}
					catch
					{
					}
				}
			}
			return text;
		}

		// Token: 0x060001AC RID: 428 RVA: 0x0000D894 File Offset: 0x0000BA94
		public void SaveSelfExtractor(string exeToGenerate, SelfExtractorFlavor flavor, string defaultExtractDirectory)
		{
			this._defaultExtractLocation = defaultExtractDirectory;
			this.SaveSelfExtractor(exeToGenerate, flavor);
			this._defaultExtractLocation = null;
		}

		// Token: 0x060001AD RID: 429 RVA: 0x0000D8B0 File Offset: 0x0000BAB0
		public void SaveSelfExtractor(string exeToGenerate, SelfExtractorFlavor flavor)
		{
			if (File.Exists(exeToGenerate))
			{
				if (this.Verbose)
				{
					this.StatusMessageTextWriter.WriteLine("The existing file ({0}) will be overwritten.", exeToGenerate);
				}
			}
			if (!exeToGenerate.EndsWith(".exe"))
			{
				if (this.Verbose)
				{
					this.StatusMessageTextWriter.WriteLine("Warning: The generated self-extracting file will not have an .exe extension.");
				}
			}
			string text = this.SfxSaveTemporary();
			this.OnSaveEvent(ZipProgressEventType.Saving_AfterSaveTempArchive);
			if (text != null)
			{
				Assembly assembly = typeof(ZipFile).Assembly;
				CSharpCodeProvider csharpCodeProvider = new CSharpCodeProvider();
				ZipFile.ExtractorSettings extractorSettings = null;
				foreach (ZipFile.ExtractorSettings extractorSettings2 in ZipFile.SettingsList)
				{
					if (extractorSettings2.Flavor == flavor)
					{
						extractorSettings = extractorSettings2;
						break;
					}
				}
				if (extractorSettings == null)
				{
					throw new BadStateException(string.Format("While saving a Self-Extracting Zip, Cannot find that flavor ({0})?", flavor));
				}
				CompilerParameters compilerParameters = new CompilerParameters();
				compilerParameters.ReferencedAssemblies.Add(assembly.Location);
				if (extractorSettings.ReferencedAssemblies != null)
				{
					foreach (string value in extractorSettings.ReferencedAssemblies)
					{
						compilerParameters.ReferencedAssemblies.Add(value);
					}
				}
				compilerParameters.GenerateInMemory = false;
				compilerParameters.GenerateExecutable = true;
				compilerParameters.IncludeDebugInformation = false;
				compilerParameters.OutputAssembly = exeToGenerate;
				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				string text2 = ZipFile.GenerateUniquePathname("tmp", null);
				if (extractorSettings.CopyThroughResources != null && extractorSettings.CopyThroughResources.Count != 0)
				{
					Directory.CreateDirectory(text2);
					byte[] array = new byte[1024];
					foreach (string text3 in extractorSettings.CopyThroughResources)
					{
						string text4 = Path.Combine(text2, text3);
						using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(text3))
						{
							using (FileStream fileStream = File.OpenWrite(text4))
							{
								int num;
								do
								{
									num = manifestResourceStream.Read(array, 0, array.Length);
									fileStream.Write(array, 0, num);
								}
								while (num > 0);
							}
						}
						compilerParameters.EmbeddedResources.Add(text4);
					}
				}
				compilerParameters.EmbeddedResources.Add(text);
				compilerParameters.EmbeddedResources.Add(assembly.Location);
				StringBuilder stringBuilder = new StringBuilder();
				bool flag = flavor == SelfExtractorFlavor.WinFormsApplication && this._defaultExtractLocation != null;
				if (flag)
				{
					this._defaultExtractLocation = this._defaultExtractLocation.Replace("\"", "");
				}
				foreach (string name in extractorSettings.ResourcesToCompile)
				{
					Stream manifestResourceStream2 = executingAssembly.GetManifestResourceStream(name);
					using (StreamReader streamReader = new StreamReader(manifestResourceStream2))
					{
						while (streamReader.Peek() >= 0)
						{
							string text5 = streamReader.ReadLine();
							if (flag)
							{
								text5 = text5.Replace("@@VALUE", this._defaultExtractLocation);
							}
							stringBuilder.Append(text5).Append("\n");
						}
					}
					stringBuilder.Append("\n\n");
				}
				string text6 = stringBuilder.ToString();
				CompilerResults compilerResults = csharpCodeProvider.CompileAssemblyFromSource(compilerParameters, new string[]
				{
					text6
				});
				if (compilerResults == null)
				{
					throw new SfxGenerationException("Cannot compile the extraction logic!");
				}
				if (this.Verbose)
				{
					foreach (string value2 in compilerResults.Output)
					{
						this.StatusMessageTextWriter.WriteLine(value2);
					}
				}
				if (compilerResults.Errors.Count != 0)
				{
					throw new SfxGenerationException("Errors compiling the extraction logic!");
				}
				this.OnSaveEvent(ZipProgressEventType.Saving_AfterCompileSelfExtractor);
				try
				{
					if (Directory.Exists(text2))
					{
						try
						{
							Directory.Delete(text2, true);
						}
						catch
						{
						}
					}
					if (File.Exists(text))
					{
						try
						{
							File.Delete(text);
						}
						catch
						{
						}
					}
				}
				catch
				{
				}
				this.OnSaveCompleted();
				if (this.Verbose)
				{
					this.StatusMessageTextWriter.WriteLine("Created self-extracting zip file {0}.", compilerResults.PathToAssembly);
				}
			}
		}

		// Token: 0x060001AE RID: 430 RVA: 0x0000DEB8 File Offset: 0x0000C0B8
		internal static string GenerateUniquePathname(string extension, string ContainingDirectory)
		{
			string name = Assembly.GetExecutingAssembly().GetName().Name;
			string text = (ContainingDirectory == null) ? Environment.GetEnvironmentVariable("TEMP") : ContainingDirectory;
			string result;
			if (text == null)
			{
				result = null;
			}
			else
			{
				int num = 0;
				string text2;
				do
				{
					num++;
					string path = string.Format("{0}-{1}-{2}.{3}", new object[]
					{
						name,
						DateTime.Now.ToString("yyyyMMMdd-HHmmss"),
						num,
						extension
					});
					text2 = Path.Combine(text, path);
				}
				while (File.Exists(text2) || Directory.Exists(text2));
				result = text2;
			}
			return result;
		}

		// Token: 0x040000A4 RID: 164
		public static readonly Encoding DefaultEncoding = Encoding.GetEncoding("IBM437");

		// Token: 0x040000A7 RID: 167
		private long _lengthOfReadStream = -99L;

		// Token: 0x040000A9 RID: 169
		private TextWriter _StatusMessageTextWriter;

		// Token: 0x040000AA RID: 170
		private bool _CaseSensitiveRetrieval;

		// Token: 0x040000AB RID: 171
		private Stream _readstream;

		// Token: 0x040000AC RID: 172
		private Stream _writestream;

		// Token: 0x040000AD RID: 173
		private bool _disposed;

		// Token: 0x040000AE RID: 174
		private List<ZipEntry> _entries;

		// Token: 0x040000AF RID: 175
		private bool _ForceNoCompression;

		// Token: 0x040000B0 RID: 176
		private string _name;

		// Token: 0x040000B1 RID: 177
		private string _Comment;

		// Token: 0x040000B2 RID: 178
		internal string _Password;

		// Token: 0x040000B3 RID: 179
		private bool _fileAlreadyExists;

		// Token: 0x040000B4 RID: 180
		private string _temporaryFileName;

		// Token: 0x040000B5 RID: 181
		private bool _contentsChanged;

		// Token: 0x040000B6 RID: 182
		private bool _hasBeenSaved;

		// Token: 0x040000B7 RID: 183
		private string _TempFileFolder;

		// Token: 0x040000B8 RID: 184
		private bool _ReadStreamIsOurs = true;

		// Token: 0x040000B9 RID: 185
		private object LOCK = new object();

		// Token: 0x040000BA RID: 186
		private bool _saveOperationCanceled;

		// Token: 0x040000BB RID: 187
		private bool _extractOperationCanceled;

		// Token: 0x040000BC RID: 188
		private bool _JustSaved;

		// Token: 0x040000BD RID: 189
		private bool _NeedZip64CentralDirectory;

		// Token: 0x040000BE RID: 190
		private bool? _OutputUsesZip64;

		// Token: 0x040000BF RID: 191
		internal bool _inExtractAll = false;

		// Token: 0x040000C0 RID: 192
		private Encoding _provisionalAlternateEncoding = Encoding.GetEncoding("IBM437");

		// Token: 0x040000C1 RID: 193
		internal Zip64Option _zip64 = Zip64Option.Default;

		// Token: 0x040000C2 RID: 194
		private static ZipFile.ExtractorSettings[] SettingsList = new ZipFile.ExtractorSettings[]
		{
			new ZipFile.ExtractorSettings
			{
				Flavor = SelfExtractorFlavor.WinFormsApplication,
				ReferencedAssemblies = new List<string>
				{
					"System.Windows.Forms.dll",
					"System.dll",
					"System.Drawing.dll"
				},
				CopyThroughResources = new List<string>
				{
					"Ionic.Zip.WinFormsSelfExtractorStub.resources",
					"Ionic.Zip.PasswordDialog.resources",
					"Ionic.Zip.ZipContentsDialog.resources"
				},
				ResourcesToCompile = new List<string>
				{
					"Ionic.Zip.Resources.WinFormsSelfExtractorStub.cs",
					"Ionic.Zip.WinFormsSelfExtractorStub",
					"Ionic.Zip.Resources.PasswordDialog.cs",
					"Ionic.Zip.PasswordDialog",
					"Ionic.Zip.Resources.ZipContentsDialog.cs",
					"Ionic.Zip.ZipContentsDialog"
				}
			},
			new ZipFile.ExtractorSettings
			{
				Flavor = SelfExtractorFlavor.ConsoleApplication,
				ReferencedAssemblies = null,
				CopyThroughResources = null,
				ResourcesToCompile = new List<string>
				{
					"Ionic.Zip.Resources.CommandLineSelfExtractorStub.cs"
				}
			}
		};

		// Token: 0x040000C3 RID: 195
		private string _defaultExtractLocation = null;

		// Token: 0x02000035 RID: 53
		private class ExtractorSettings
		{
			// Token: 0x04000219 RID: 537
			public SelfExtractorFlavor Flavor;

			// Token: 0x0400021A RID: 538
			public List<string> ReferencedAssemblies;

			// Token: 0x0400021B RID: 539
			public List<string> CopyThroughResources;

			// Token: 0x0400021C RID: 540
			public List<string> ResourcesToCompile;
		}
	}
}
