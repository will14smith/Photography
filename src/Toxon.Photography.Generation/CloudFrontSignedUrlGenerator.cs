using System.Security.Cryptography;
using System.Text;

namespace Toxon.Photography.Generation;

public class CloudFrontSignedUrlGenerator(string baseUrl, string keyPairId, string privateKeyPem, TimeSpan expirationPeriod)
{
    public string GetSignedUrl(string objectKey)
    {
        var normalizedBaseUrl = baseUrl.TrimEnd('/');
        var normalizedKey = EncodeObjectKey(objectKey);
        var resourceUrl = $"{normalizedBaseUrl}/{normalizedKey}";

        var expiresAt = DateTimeOffset.UtcNow.Add(expirationPeriod);
        var expiresUnix = expiresAt.ToUnixTimeSeconds();

        var cannedPolicy = $"{{\"Statement\":[{{\"Resource\":\"{resourceUrl}\",\"Condition\":{{\"DateLessThan\":{{\"AWS:EpochTime\":{expiresUnix}}}}}}}]}}";
        var signature = SignPolicy(cannedPolicy);

        var separator = resourceUrl.Contains('?') ? '&' : '?';
        return $"{resourceUrl}{separator}Expires={expiresUnix}&Signature={signature}&Key-Pair-Id={Uri.EscapeDataString(keyPairId)}";
    }

    private static string EncodeObjectKey(string objectKey)
    {
        var segments = objectKey
            .TrimStart('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString);

        return string.Join('/', segments);
    }

    private string SignPolicy(string policy)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);

        var policyBytes = Encoding.UTF8.GetBytes(policy);
        var signatureBytes = rsa.SignData(policyBytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

        return ToCloudFrontSafeBase64(signatureBytes);
    }

    private static string ToCloudFrontSafeBase64(byte[] data) =>
        Convert.ToBase64String(data)
            .Replace('+', '-')
            .Replace('=', '_')
            .Replace('/', '~');
}