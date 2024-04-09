using Microsoft.Extensions.Logging;

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
/// This file is responsible for creating ILogger instances tailored to specific categories.
/// </summary>
namespace LoggerLibrary
{
    public class CustomFileLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new CustomFileLogger(categoryName);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}