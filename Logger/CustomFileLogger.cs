using Microsoft.Extensions.Logging;
using System.Diagnostics;

/// <summary>
/// Author:    Shu Chen
/// Partner:   Ping-Hsun Hsieh
/// Date:      25/3/2024
/// Course:    CS 3500, University of Utah, School of Computing
/// Copyright: CS 3500 and Shu Chen - This work may not
///            be copied for use in Academic Coursework.
///
/// I, Shu Chen, certify that I wrote this code from scratch and
/// did not copy it in part or whole from another source.  All
/// references used in the completion of the assignments are cited
/// in my README file.
///
/// File Contents
/// This file contains the implementation of the CustomFileLogger class, which is dealing with logging messages to a custom log file.
/// It creates log files specific to the provided category name in the application's data folder.
/// </summary>

namespace LoggerLibrary
{
    public class CustomFileLogger : ILogger
    {
        private string _FileName;

        public CustomFileLogger(string categoryName)
        {
            int procress = Process.GetCurrentProcess().Id;
            _FileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + $"CS3500-{categoryName}.log";
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            lock (this)
            {
                File.AppendAllText(_FileName, $"{DateTime.Now} Thread{Thread.CurrentThread.ManagedThreadId}\t-{logLevel}-  {formatter(state, exception)} {Environment.NewLine}");
            }
        }
    }
}