using System;

namespace ShortDash.Core.Interfaces
{
    public interface IDataProtectionService
    {
        void AddKeyChangedEventHandler(EventHandler handler);

        bool Initialized();

        string Protect(string plaintext);

        void RemoveKeyChangedEventHandler(EventHandler handler);

        void SetKey(byte[] key = null);

        string Unprotect(string ciphertext);
    }
}