using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CMD_VOL
{
	class Program
	{
		static void Main(string[] args)
		{
			CmdResult result;
			//获取进程所在盘符序列号
			result = CmdExecute(new string[] {"vol" });
			Console.WriteLine($"Output={result.OutputData}");
			Console.WriteLine($"Error={result.ErrorData}");
			//获取d盘序列号
			result = CmdExecute(new string[] {"d:", "vol" });
			Console.WriteLine($"Output={result.OutputData}");
			Console.WriteLine($"Error={result.ErrorData}");
			Console.WriteLine("回车退出程序");
			Console.ReadLine();
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
						startInfo.UseShellExecute =false;
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
			return new CmdResult() {
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
