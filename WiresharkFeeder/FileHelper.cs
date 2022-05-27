using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WiresharkFeeder
{
    class FileHelper : IDisposable
    {
        private string _filePath;
        private FileStream _fileStream;
        private BinaryReader _binaryReader;

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private Task _taskWriteHandleChecking;

        private object _streamLocker;

        private bool _isWriteHandleReleased;

        public event FileStreamIsReset OnFileStreamIsReset;
        public delegate void FileStreamIsReset();

        private bool _clearFileOnStartupOrStreamReset;

        public FileHelper(string filePath, bool clearFileOnStartupOrStreamReset = true)
        {
            _filePath = filePath;
            _clearFileOnStartupOrStreamReset = clearFileOnStartupOrStreamReset;

            _streamLocker = new object();
            
            IntializeFileReader();

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            _taskWriteHandleChecking = new Task(WriteHandleChecking, _cancellationToken, TaskCreationOptions.LongRunning);
            _taskWriteHandleChecking.Start();
        }

        private void IntializeFileReader()
        {
            TryCreateOrClearFile();
            InitializeFileReader();
        }

        private void InitializeFileReader()
        {
            lock (_streamLocker)
            {
                _binaryReader?.Close();
                _binaryReader?.Dispose();

                _fileStream?.Close();
                _binaryReader?.Dispose();

                _fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Write);
                _binaryReader = new BinaryReader(_fileStream);
            }
        }

        private async void WriteHandleChecking()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                if (IsWriteHandleReleased())
                {
                    if (!_isWriteHandleReleased)
                    {
                        Console.WriteLine("File source: Write handle is released");

                        TryCreateOrClearFile();
                        InitializeFileReader();
                        OnFileStreamIsReset?.Invoke();
                    }

                    _isWriteHandleReleased = true;
                }
                else
                {
                    if (_isWriteHandleReleased)
                    {
                        Console.WriteLine("File source: Write handle is held");

                        _isWriteHandleReleased = false;
                    }
                }

                await Task.Delay(500);
            }
        }

        private bool IsWriteHandleReleased()
        {
            bool isWriteHandleReleased;
            
            try
            {
                //lock (_streamLocker)
                //{
                    using FileStream stream =
                        new FileStream(_filePath, FileMode.Open, FileAccess.Write, FileShare.Read);
                //}
                isWriteHandleReleased = true;
            }
            catch (IOException)
            {
                isWriteHandleReleased = false;
            }

            return isWriteHandleReleased;
        }

        public void TryCreateOrClearFile()
        {
            if (File.Exists(_filePath))
            {
                if (_clearFileOnStartupOrStreamReset)
                {
                    try
                    {
                        File.WriteAllBytes(_filePath, new byte[] { });
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                try
                {
                    File.Create(_filePath).Close();
                }
                catch
                {
                }
            }
        }

        public int ReadBytes(byte[] buffer, int offset, int count)
        {
            int readCount = 0;

            lock (_streamLocker)
            {
                if (_binaryReader.BaseStream.Length - _binaryReader.BaseStream.Position >= count)
                {
                    readCount = _binaryReader.Read(buffer, offset, count);
                }
            }

            return readCount;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();

            _binaryReader?.Close();
            _binaryReader?.Dispose();

            _fileStream?.Close();
            _binaryReader?.Dispose();
        }
    }
}
