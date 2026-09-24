using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LittleWeeps.Adapters
{
    public enum CheckpointStatus { Missing, Loaded, Recovered, Corrupt, Unsupported }
    public readonly struct CheckpointRead
    {
        public readonly CheckpointStatus Status;
        public readonly string Payload, Source;
        public CheckpointRead(CheckpointStatus status, string payload = null, string source = null)
        { Status = status; Payload = payload; Source = source; }
    }
    // One local writer, synchronous small checkpoints. Not the final world journal.
    public sealed class CheckpointStore
    {
        private const string Header = "LITTLEWEEPS-SOLO-1";
        private const int MaxBytes = 1024 * 1024;
        private readonly string path;
        private readonly Func<string, bool> validate;
        public CheckpointStore(string path, Func<string, bool> validate)
        { this.path = Path.GetFullPath(path); this.validate = validate ?? throw new ArgumentNullException(nameof(validate)); }
        private static string Hash(byte[] bytes)
        { using var sha = SHA256.Create(); return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant(); }
        private CheckpointRead Read(string file)
        {
            // File.Exists suppresses access errors and returns false for directories.
            // Only an actual missing path may be treated as a new world's empty slot.
            FileAttributes attributes;
            try { attributes = File.GetAttributes(file); }
            catch (FileNotFoundException) { return new CheckpointRead(CheckpointStatus.Missing); }
            catch (DirectoryNotFoundException) { return new CheckpointRead(CheckpointStatus.Missing); }
            if ((attributes & FileAttributes.Directory) != 0) throw new IOException("A directory occupies the checkpoint file path.");
            if (new FileInfo(file).Length > MaxBytes) return new CheckpointRead(CheckpointStatus.Corrupt);
            var text = File.ReadAllText(file, new UTF8Encoding(false, true));
            var first = text.IndexOf('\n'); var second = first < 0 ? -1 : text.IndexOf('\n', first + 1);
            if (first < 0 || second < 0) return new CheckpointRead(CheckpointStatus.Corrupt);
            if (text.Substring(0, first) != Header) return new CheckpointRead(text.StartsWith("LITTLEWEEPS-SOLO-", StringComparison.Ordinal) ? CheckpointStatus.Unsupported : CheckpointStatus.Corrupt);
            var payload = text.Substring(second + 1);
            if (Hash(Encoding.UTF8.GetBytes(payload)) != text.Substring(first + 1, second - first - 1) || !validate(payload)) return new CheckpointRead(CheckpointStatus.Corrupt);
            return new CheckpointRead(CheckpointStatus.Loaded, payload, file);
        }
        private CheckpointRead ReadChecked(string file)
        {
            try { return Read(file); }
            catch (DecoderFallbackException) { return new CheckpointRead(CheckpointStatus.Corrupt); }
            catch (NotSupportedException) { return new CheckpointRead(CheckpointStatus.Unsupported); }
            // IO/access failures propagate. Never turn an unreadable disk into a new world.
        }
        public CheckpointRead Load()
        {
            var main = ReadChecked(path);
            if (main.Status == CheckpointStatus.Loaded || main.Status == CheckpointStatus.Unsupported) return main;
            var backup = ReadChecked(path + ".bak");
            if (backup.Status == CheckpointStatus.Unsupported) return backup;
            if (backup.Status == CheckpointStatus.Loaded) return new CheckpointRead(CheckpointStatus.Recovered, backup.Payload, backup.Source);
            // A first-ever interrupted commit may have only a completed staged file.
            var staged = ReadChecked(path + ".pending");
            if (staged.Status == CheckpointStatus.Unsupported) return staged;
            if (staged.Status == CheckpointStatus.Loaded) return new CheckpointRead(CheckpointStatus.Recovered, staged.Payload, staged.Source);
            return new CheckpointRead(main.Status == CheckpointStatus.Missing && backup.Status == CheckpointStatus.Missing && staged.Status == CheckpointStatus.Missing ? CheckpointStatus.Missing : CheckpointStatus.Corrupt);
        }
        public void Save(string payload)
        {
            if (payload == null || !validate(payload)) throw new InvalidDataException("Refusing invalid solo checkpoint.");
            var current = Load();
            if (current.Status == CheckpointStatus.Unsupported || current.Status == CheckpointStatus.Corrupt) throw new InvalidDataException("Existing progress requires recovery; it will not be replaced with a blank world.");
            var bytes = Encoding.UTF8.GetBytes(Header + "\n" + Hash(Encoding.UTF8.GetBytes(payload)) + "\n" + payload);
            if (bytes.Length > MaxBytes) throw new InvalidDataException("Solo checkpoint exceeds its size budget.");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var staged = path + ".pending";
            // Preserve a recovered staged checkpoint before reusing its filename.
            if (current.Source == staged) File.Copy(staged, path + ".bak", true);
            using (var stream = new FileStream(staged, FileMode.Create, FileAccess.Write, FileShare.None))
            { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            if (ReadChecked(path).Status == CheckpointStatus.Loaded)
                File.Replace(staged, path, path + ".bak");
            else
            {
                // Keep invalid originals for recovery instead of rotating corruption over the good backup.
                if (File.Exists(path)) File.Move(path, path + ".damaged-" + Guid.NewGuid().ToString("N"));
                File.Move(staged, path);
            }
        }
    }
}
