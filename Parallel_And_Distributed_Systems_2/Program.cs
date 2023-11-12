namespace Parallel_And_Distributed_Systems_2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var fileSize = new FileInfo(
                "C:\\Users\\ivelin.georgiev\\Downloads\\mongodb-compass-1.40.4-win32-x64.exe")
                .Length;

            var numThreads = 5d;
            var blockSize = (int)Math.Floor((fileSize / 5d));
            var blocks = new List<(int From, int To)>();
            var accumulatedBlocks = 0;
            var task = new List<Task>();
            var thread = new List<Thread>();

            for (int i = 0; i < numThreads; i++)
            {
                blocks.Add((accumulatedBlocks, blockSize));
                accumulatedBlocks += blockSize;
            }

            var paddingBytes = (int)fileSize - accumulatedBlocks;
            var finalBlock = blocks.Last();

            finalBlock.To += paddingBytes;

            //for(var i = 0; i <numThreads; i++)
            //{
            //    task.Add(
            //        ReadWriteAsync(blocks[i].From, blocks[i].To));
            //}
            //
            //Task.WhenAll(task).Wait();

            for (var i = 0; i < numThreads; i++)
            {
                var t = ReadWritehreadAsync(blocks[i].From, blocks[i].To);
                t.Start();
                thread.Add(t);
            }

            for (var i = 0; i < numThreads; i++)
            {
                thread[i].Join();
            }
        }

        static Task ReadWriteAsync(int position, int count)
        {
            return Task.Run(() =>
            {
                using (var reader = new FileReader(
                "C:\\Users\\ivelin.georgiev\\Downloads\\mongodb-compass-1.40.4-win32-x64.exe"))
                {
                    reader.Open();
                    var bytes = reader.Read(position, count);

                    using (var writer = new FileWriter("testing.txt"))
                    {
                        writer.Open();
                        writer.Write(position, bytes);
                    }
                }
            });
        }

        static Thread ReadWritehreadAsync(int position, int count)
        {
            return new Thread(() =>
            {
                using (var reader = new FileReader(
                "C:\\Users\\ivelin.georgiev\\Downloads\\mongodb-compass-1.40.4-win32-x64.exe"))
                {
                    reader.Open();
                    var bytes = reader.Read(position, count);

                    using (var writer = new FileWriter("testing.txt"))
                    {
                        writer.Open();
                        writer.Write(position, bytes);
                    }
                }
            });
        }
    }

    internal class FileReader : IDisposable
    {
        private FileStream? Stream;
        private bool disposedValue;
        private readonly string _path;

        public FileReader(string path)
        {
            _path = path;
        }

        public void Open()
        {
            if (!File.Exists(_path))
            {
                throw new FileNotFoundException(
                    $"File could not be found [{_path}]");
            }

            Stream = new FileStream(
                path: _path,
                mode: FileMode.Open,
                access: FileAccess.Read,
                share: FileShare.Read);
        }

        public byte[] Read(int position, int count)
        {
            if (Stream is null)
            {
                throw new ArgumentNullException(
                    nameof(Stream),
                    "The stream has not been opened");
            }

            var buffer = new byte[count];
            var bytesRead = 0;

            Stream.Seek(position, SeekOrigin.Begin);

            while (bytesRead < count)
            {
                bytesRead += Stream.Read(
                    buffer,
                    bytesRead,
                    bytesRead + 4096 > count
                        ? count - bytesRead
                        : 4096);
            }

            return buffer;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stream?.Dispose();
                    Stream = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    internal class FileWriter : IDisposable
    {
        private FileStream? Stream;
        private bool disposedValue;
        private readonly string _path;

        public FileWriter(string path)
        {
            _path = path;
        }

        public void Open()
        {
            Stream = new FileStream(
                path: _path,
                mode: FileMode.OpenOrCreate,
                access: FileAccess.Write,
                share: FileShare.ReadWrite);
        }

        public void Write(int position, byte[] buffer)
        {
            if (Stream is null)
            {
                throw new ArgumentNullException(
                    nameof(Stream),
                    "The stream has not been opened");
            }

            Stream.Seek(position, SeekOrigin.Begin);

            var bytesWritten = 0;

            var batchCount = buffer.Length < 4096
                ? buffer.Length
                : 4096;

            while (bytesWritten < buffer.Length)
            {
                Stream.Write(
                    buffer,
                    bytesWritten,
                    bytesWritten + 4096 > buffer.Length
                        ? buffer.Length - bytesWritten
                        : 4096);

                bytesWritten += batchCount;
            }
        }

        protected virtual void Dispose(bool disposing)
        {

            if (!disposedValue)
            {
                if (disposing)
                {
                    Stream?.Dispose();
                    Stream = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}