using System.Security.Cryptography;
using System.Text;

namespace ConfigCompare.Desktop.Security;

/// <summary>
/// Provides Windows DPAPI-based encryption for sensitive credential values
/// (e.g. Service Principal client secrets) stored in the settings file.
/// Data is protected per-user: only the Windows account that encrypted the
/// value can decrypt it on the same machine.
/// </summary>
internal static class CredentialProtection
{
    /// <summary>
    /// Encrypts a plaintext secret using DPAPI (CurrentUser scope) and returns
    /// a Base64 string safe for storage in JSON.
    /// Returns <see langword="null"/> if <paramref name="plaintext"/> is null or empty.
    /// </summary>
    internal static string? Protect(string? plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            return null;

        var bytes = Encoding.UTF8.GetBytes(plaintext);
        var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    /// <summary>
    /// Decrypts a Base64-encoded DPAPI-protected value and returns the original
    /// plaintext.  Returns <see langword="null"/> if <paramref name="ciphertext"/>
    /// is null, empty, or cannot be decrypted (e.g. data from a different user/machine).
    /// </summary>
    internal static string? Unprotect(string? ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
            return null;

        try
        {
            var encrypted = Convert.FromBase64String(ciphertext);
            var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch (CryptographicException)
        {
            // Value encrypted by a different user / machine or corrupted – treat as absent.
            return null;
        }
        catch (FormatException)
        {
            // Not a valid Base64 string – could be a legacy plain-text value; return as-is.
            return ciphertext;
        }
    }
}
