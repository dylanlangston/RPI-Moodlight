using System.Diagnostics;
using System.Collections.Concurrent;

namespace RPI_Moodlight
{
    /// <summary>
    /// Utility methods to handle Input/Output related tasks. 
    /// </summary>
    class IO_Utilities
    {
        /// <summary>
        /// Write string to log file
        ///
        /// Writes to the executing folder or Desktop depending on permissions.
        /// </summary>
        /// <param name="log">string to write</param>
        public static void Logging(string log)
        {
            static void WriteTextToLogFile(string text)
            {
                string fileName = Process.GetCurrentProcess().ProcessName.ToLower().Replace(" ", "-") + "-http.log";
                string f = null;
                try
                {
                    // Write to Executing folder
                    f = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                    CreateFolder(f);
                }
                catch
                {
                    // Write to Desktop
                    f = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                    CreateFolder(f);
                }
                finally
                {
                    WriteTextToFile(text.TrimEnd(), fileName, true);
                }
            }

            string message = Environment.NewLine + "Timestamp - " + DateTime.Now.ToString() + Environment.NewLine + log + Environment.NewLine;
            WriteTextToLogFile(message);
#if DEBUG
            Console.Write(message);
#endif
        }

        /// <summary>
        /// Write text to a file, meant to be used with multiple processes accessing the same file
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fileName"></param>
        public static void WriteTextToFile(string text, string fileName, bool trim = false, int maxLines = 500000)
        {
            messageQueue.Enqueue((text, fileName, trim, maxLines));

            lock (messageWriteTasks)
            {
                var task = Task.Run(() =>
                {
                    if (!messageQueue.TryDequeue(out var currentMessage))
                        return;

                    try
                    {
                        locker.AcquireWriterLock(Int32.MaxValue);
                        if (currentMessage.trim)
                        {
                            locker.AcquireReaderLock(Int32.MaxValue);
                            var lines = File.ReadAllLines(currentMessage.fileName).Append(currentMessage.message)
                                .TakeLast(Math.Max(1, currentMessage.maxLines - currentMessage.message.Count(c => c.Equals('\n'))));
                            File.WriteAllLines(currentMessage.fileName, lines);
                        }
                        else
                        {
                            File.AppendAllLines(currentMessage.fileName, new[] { currentMessage.message });
                        }
                    }
                    finally
                    {
                        if (currentMessage.trim) locker.ReleaseReaderLock();
                        locker.ReleaseWriterLock();
                    }
                });
                task.ContinueWith(t => { lock (messageWriteTasks) { messageWriteTasks.Remove(task); }});

                messageWriteTasks.Add(task);
            }
        }
        static List<Task> messageWriteTasks = new();
        static ConcurrentQueue<(string message, string fileName, bool trim, int maxLines)> messageQueue = new();
        static ReaderWriterLock locker = new ReaderWriterLock();

        public static void FlushAllTextToFiles()
        {
            lock (messageWriteTasks)
            {
                foreach (var task in messageWriteTasks) task.Wait();
            }
        }

        /// <summary>
        /// Try to create a folder for the specified file.
        /// </summary>
        /// <param name="filename">Filename to create folder for.</param>
        public static void CreateFolder(string filename)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filename))) Directory.CreateDirectory(Path.GetDirectoryName(filename));
        }
    }
}
