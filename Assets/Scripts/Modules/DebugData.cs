using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DebugData : MonoBehaviour
{
	public const string LogFormat = "[DoubleMe]";

	protected List<string> _logTextList = new List<string>();
	protected List<string> _errorTextList = new List<string>();
	protected List<string> _allTextList = new List<string>();

	private string _logTextOnPanel = string.Empty;
	private string _errorTextOnPanel = string.Empty;
	private string _errorTextForSave = string.Empty;

	private string _errorDate = string.Empty;
	protected virtual void AddDebugOnPanel(string disPlayText) { }
	protected virtual async Task WriteErrorLogOnTextFile(string logError) { }

	protected void HandleLog(string logString, string stackTrace, LogType type)
	{
		string[] stackTraceArray = stackTrace.Split('\n');
		_errorDate = DateTime.Now.ToString();
		if (type == LogType.Log)
		{
			if (logString.Contains(LogFormat))
			{
				_logTextOnPanel = $"<color=white>\n[{type}] : {logString}\n{stackTraceArray[1]}\n</color>";
				_logTextList.Add(_logTextOnPanel);
				AddDebugOnPanel(_logTextOnPanel);
			}
		}
		else
		{
			if (string.IsNullOrEmpty(stackTrace) == true) _errorTextOnPanel = $"<color=red>\n[{type}] : {logString}\n</color>";
			else _errorTextOnPanel = $"<color=red>\n[{type}] : {logString}\n{stackTraceArray[1]}\n</color>";
			_errorTextForSave = $"[{_errorDate}]\n[{type}] : {logString}\n{stackTrace}\n";
			_errorTextList.Add(_errorTextOnPanel);
			AddDebugOnPanel(_errorTextOnPanel);
			WriteErrorLogOnTextFile(_errorTextForSave);
		}
	}
}
