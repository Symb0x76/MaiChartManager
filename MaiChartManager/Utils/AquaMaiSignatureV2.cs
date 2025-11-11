using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace MaiChartManager.Utils;

public static class AquaMaiSignatureV2
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct AquaMaiSignatureBlock
    {
        public PubKeyId KeyId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 132)]
        public byte[] Signature;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public byte[] Magic;
        public byte Version;
    }

    private static AquaMaiSignatureBlock? parseFromBytes(byte[] data)
    {
        var size = Marshal.SizeOf<AquaMaiSignatureBlock>();
        if (data.Length < size)
        {
            return null;
        }

        var block = data.AsSpan(data.Length - size);
        IntPtr ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.Copy(block.ToArray(), 0, ptr, size);
            var stru = Marshal.PtrToStructure<AquaMaiSignatureBlock>(ptr);
            if (!stru.Magic.AsSpan().SequenceEqual("AquaMaiSig"u8))
            {
                return null;
            }
            return stru;
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    public enum PubKeyId : byte
    {
        None,
        Local,
        CI,
    }

    private static readonly Dictionary<PubKeyId, byte[]> pubKeys = new()
    {
        {
            PubKeyId.Local,
            Convert.FromBase64String(
                "MIGbMBAGByqGSM49AgEGBSuBBAAjA4GGAAQA3ekmuUGESGdUfOuGfWxZ2tIGsWJaAGsciSP2nyouGquEJf2k+6fm21ESJQAXg9XOUaf3jcsZU+YZdzczDIorMNMBcxQXet1B/B3Mqz7CLdRthDhLelVkrqeRE8TNcPUCQjT/pxKLWBAQWDkwdzsUQS0LLpaZ0NbG4880RzNY5ia7zqg=")
        },
        {
            PubKeyId.CI,
            Convert.FromBase64String(
                "MIGbMBAGByqGSM49AgEGBSuBBAAjA4GGAAQAvi9gtqbPF0g7K52lumBRiztMb5lVKbTwhzwVSVsMBUo5wXp9w86CnIh3/VErXtyneP1BBMLFDEtd4Cb11eQmxBMBuPjY61oca4gZhIxgQ8e0ki/pUhtUQwIQ48AN/gba/lq0GWBaPrwEyhSvHArHsPo2WxFczdsOO0mTgwq0bAw/tTw=")
        },
    };

    public enum VerifyStatus
    {
        NotFound,
        InvalidKeyId,
        InvalidSignature,
        Valid,
    }

    public record VerifyResult(VerifyStatus Status, PubKeyId KeyId);

    public static VerifyResult VerifySignature(byte[] data)
    {
        var block = parseFromBytes(data);
        if (block == null)
        {
            return new VerifyResult(VerifyStatus.NotFound, PubKeyId.None);
        }

        if (!pubKeys.ContainsKey(block.Value.KeyId))
        {
            return new VerifyResult(VerifyStatus.InvalidKeyId, block.Value.KeyId);
        }

        using var ecdsa = ECDsa.Create();
        ecdsa.ImportSubjectPublicKeyInfo(pubKeys[block.Value.KeyId], out _);

        var size = Marshal.SizeOf<AquaMaiSignatureBlock>();
        var dataToVerify = data.AsSpan(0, data.Length - size);
        var isValid = ecdsa.VerifyData(dataToVerify, block.Value.Signature, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        return new VerifyResult(isValid ? VerifyStatus.Valid : VerifyStatus.InvalidSignature, block.Value.KeyId);
    }
}