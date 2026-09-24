using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace LittleWeeps.Adapters
{
    // Per-Windows-user DPAPI. This is deliberately not a plaintext fallback when
    // a copied enrollment cannot be opened by another OS account or computer.
    public static class WindowsPairingVault
    {
        [StructLayout(LayoutKind.Sequential)] private struct Blob {public int length;public IntPtr data;}
        [DllImport("crypt32.dll",SetLastError=true)] [return:MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptUnprotectData(ref Blob input,IntPtr description,IntPtr entropy,IntPtr reserved,IntPtr prompt,int flags,out Blob output);
        [DllImport("kernel32.dll")] private static extern IntPtr LocalFree(IntPtr memory);
        public static string Read(string path)
        {
            if(Environment.OSVersion.Platform!=PlatformID.Win32NT)throw new PlatformNotSupportedException("Windows enrollment required.");
            var file=new FileInfo(path);if(!file.Exists || file.Length<16 || file.Length>65536)throw new InvalidDataException("Enrollment unavailable.");
            var bytes=File.ReadAllBytes(path);var input=new Blob{length=bytes.Length,data=Marshal.AllocHGlobal(bytes.Length)};
            Blob output=default;
            try
            {
                Marshal.Copy(bytes,0,input.data,bytes.Length);
                if(!CryptUnprotectData(ref input,IntPtr.Zero,IntPtr.Zero,IntPtr.Zero,IntPtr.Zero,1,out output))
                    throw new InvalidDataException("Enrollment could not be unlocked for this Windows user.");
                if(output.length<1 || output.length>32768)throw new InvalidDataException("Invalid enrollment size.");
                var plain=new byte[output.length];Marshal.Copy(output.data,plain,0,plain.Length);
                try{return new UTF8Encoding(false,true).GetString(plain);}finally{Array.Clear(plain,0,plain.Length);}
            }
            finally
            {
                if(output.data!=IntPtr.Zero){for(var i=0;i<output.length;i++)Marshal.WriteByte(output.data,i,0);LocalFree(output.data);}
                Marshal.FreeHGlobal(input.data);Array.Clear(bytes,0,bytes.Length);
            }
        }
    }
}
