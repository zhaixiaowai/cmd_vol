using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CMD_VOL
{
	class Program
	{
		static void Main(string[] args)
		{
			//通过GetVolumeInformation获取
			var diskID = GetdiskID();
			Console.WriteLine($"GetVolumeInformation C={diskID}");
			CmdResult result;
			//获取进程所在盘符序列号
			result = CmdExecute(new string[] { "vol" });
			Console.WriteLine($"Output={result.OutputData}");
			Console.WriteLine($"Error={result.ErrorData}");
			//获取c盘序列号
			result = CmdExecute(new string[] { "c:", "vol" });
			Console.WriteLine($"Output={result.OutputData}");
			Console.WriteLine($"Error={result.ErrorData}");
			//获取d盘序列号
			result = CmdExecute(new string[] { "d:", "vol" });
			Console.WriteLine($"Output={result.OutputData}");
			Console.WriteLine($"Error={result.ErrorData}");
			Console.WriteLine("回车退出程序");
			Console.ReadLine();
		}
		/// <summary>
		/// GetVolumeInformation
		/// </summary>
		/// <param name="lpRootPathName">欲获取信息的那个卷的根路径</param>
		/// <param name="lpVolumeNameBuffer">用于装载卷名（卷标）的一个字串 </param>
		/// <param name="nVolumeNameSize">lpVolumeNameBuffer字串的长度</param>
		/// <param name="lpVolumeSerialNumber">用于装载磁盘卷序列号的变量</param>
		/// <param name="lpMaximumComponentLength">指定一个变量，用于装载文件名每一部分的长度。例如，在“c:\component1\component2.ext”的情况下，它就代表component1或component2名称的长度 .</param>
		/// <param name="lpFileSystemFlags">用于装载一个或多个二进制位标志的变量。对这些标志位的解释如下：
		/// FS_CASE_IS_PRESERVED 文件名的大小写记录于文件系统
		/// FS_CASE_SENSITIVE 文件名要区分大小写
		/// FS_UNICODE_STORED_ON_DISK 文件名保存为Unicode格式
		/// FS_PERSISTANT_ACLS 文件系统支持文件的访问控制列表（ACL）安全机制
		/// FS_FILE_COMPRESSION 文件系统支持逐文件的进行文件压缩
		/// FS_VOL_IS_COMPRESSED 整个磁盘卷都是压缩的
		///</param>
		/// <param name="lpFileSystemNameBuffer">指定一个缓冲区,用于装载文件系统的名称（如FAT，NTFS以及其他）       </param>
		/// <param name="nFileSystemNameSize">lpFileSystemNameBuffer字串的长度</param>
		/// <returns></returns>
		[DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
		public static extern bool GetVolumeInformation(string lpRootPathName, string lpVolumeNameBuffer, int nVolumeNameSize, ref int lpVolumeSerialNumber, int lpMaximumComponentLength, int lpFileSystemFlags, string lpFileSystemNameBuffer, int nFileSystemNameSize);
		/// <summary>
		/// 获取硬盘ID
		/// </summary>
		/// <returns></returns>
		public static string GetdiskID()
		{

			const int MAX_FILENAME_LEN = 256;
			int retVal = 0;
			int a = 0;
			int b = 0;
			string str1 = null;
			string str2 = null;


			GetVolumeInformation(
				@"C:\",
				str1,
				MAX_FILENAME_LEN,
				ref retVal,
				a,
				b,
				str2,
				MAX_FILENAME_LEN);

			return Convert.ToString(retVal, 16).ToUpper();

		}
		/// <summary>
		/// 执行DOS命令
		/// </summary>
		/// <param name="commands">顺序执行命令列表</param>
		/// <param name="timeoutSecond">等待命令执行的时间（单位：秒），如果设定为0，则无限等待</param>
		/// <returns></returns>
		static CmdResult CmdExecute(string[] commands, int timeoutSecond = 0)
		{
			var output = new StringBuilder();
			var error = new StringBuilder();
			if (commands != null)
			{
				try
				{
					using (var process = new Process())
					{
						var startInfo = new ProcessStartInfo();
						startInfo.FileName = "cmd.exe";
						//设定需要执行的命令
						startInfo.UseShellExecute = false;
						//不使用系统外壳程序启动
						startInfo.RedirectStandardInput = true;
						//重定向输入
						startInfo.RedirectStandardOutput = true;
						var filter = new Regex(@"^(Microsoft Windows|版权所有|(\(c\) \d{4} Microsoft Corporation)|([a-zA-Z]:(\\[^\\]*)+)\>)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
						process.OutputDataReceived += (object s, DataReceivedEventArgs e) =>
						{
							if (e.Data == null || filter.IsMatch(e.Data)) return;
							output.Append(e.Data);
						};
						startInfo.RedirectStandardError = true;
						process.ErrorDataReceived += (object s, DataReceivedEventArgs e) =>
						{
							if (e.Data == null) return;
							error.Append(e.Data);
						};
						//重定向输出
						startInfo.CreateNoWindow = true;
						//不创建窗口
						process.StartInfo = startInfo;
						if (process.Start())
						{
							process.BeginOutputReadLine();
							process.BeginErrorReadLine();
							foreach (var command in commands)
							{
								process.StandardInput.WriteLine(command);
							}
							process.StandardInput.WriteLine("exit");
							if (timeoutSecond == 0)
							{
								process.WaitForExit();
							}
							else
							{
								process.WaitForExit(timeoutSecond);
							}
						}
					}
				}
				catch (Exception ex)
				{
					error.Append(ex.ToString());
				}
			}
			return new CmdResult()
			{
				OutputData = output.ToString(),
				ErrorData = error.ToString()
			};
		}
		/// <summary>
		/// cmd执行结果
		/// </summary>
		class CmdResult
		{
			/// <summary>
			/// 程序正常输出
			/// </summary>
			public string OutputData { get; set; }
			/// <summary>
			/// 异常输出
			/// </summary>
			public string ErrorData { get; set; }
		}
	}
}
