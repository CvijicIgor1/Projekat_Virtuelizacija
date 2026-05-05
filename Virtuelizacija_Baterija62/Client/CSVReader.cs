using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Client
{
    // klasa koja implementira IDisposable
    public class CsvReader : IDisposable
    {
        private StreamReader _reader;
        private FileStream _fileStream;
        private bool _disposed = false;  // zaustavi da ne Dispose-ujemo dva puta

        public string FilePath { get; }

        public CsvReader(string filePath)
        {
            FilePath = filePath;
            _fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            _reader = new StreamReader(_fileStream);
        }

        public string CitajSledeciRed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(CsvReader));

            return _reader.ReadLine();
        }

        public bool KrajFajla()
        {
            return _reader.EndOfStream;
        }

        // Glavni Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);  // Garabage collector resava ako nismo pozvali Dispose, ali ako smo, ne treba da zove finalizer
        }

        // IDisposable patern
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed) // ulazimo samo ako nismo vec oslobodili resurse
            {
                if (disposing)
                {
                    // Oslobadjamo managed resurse
                    _reader?.Dispose();
                    _fileStream?.Dispose();
                    Console.WriteLine($"[Dispose] Resursi zatvoreni za: {Path.GetFileName(FilePath)}");
                }
                _disposed = true;
            }
        }

        // Destruktor (finalizer) - poziva se ako zaboravimo Dispose
        ~CsvReader()
        {
            Dispose(false);
            Console.WriteLine("[Finalizer] GC pozvao destruktor - trebalo je koristiti using!");
        }
    }
}
