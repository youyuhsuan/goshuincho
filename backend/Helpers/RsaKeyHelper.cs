using System.Security.Cryptography;

namespace backend.Helpers
{
    // RSA Key Management Helper for JWT Authentication
    public static class RsaKeyHelper
    {
        // Load RSA public key for JWT token validation
        public static RSA LoadPublicKey(IConfiguration configuration)
        {
            var rsa = RSA.Create();

            // Recommended for development with user-secrets
            var publicKeyPem = configuration["Jwt:PublicKey"];
            if (!string.IsNullOrEmpty(publicKeyPem))
            {
                rsa.ImportFromPem(publicKeyPem);
                return rsa;
            }

            throw new InvalidOperationException("Unable to load RSA public key. Please check configuration settings.");
        }


        // Load RSA private key for JWT token signing
        public static RSA LoadPrivateKey(IConfiguration configuration)
        {
            var rsa = RSA.Create();

            // Recommended for development with user-secrets
            var privateKeyPem = configuration["Jwt:PrivateKey"];
            if (!string.IsNullOrEmpty(privateKeyPem))
            {
                rsa.ImportFromPem(privateKeyPem);
                return rsa;
            }

            throw new InvalidOperationException("Unable to load RSA private key. Please check configuration settings.");
        }


        // Generate a new RSA key pair (for system initialization only)
        public static (string publicKey, string privateKey) GenerateKeyPair(int keySize = 2048)
        {
            using var rsa = RSA.Create(keySize);

            var publicKey = rsa.ExportRSAPublicKeyPem();
            var privateKey = rsa.ExportRSAPrivateKeyPem();

            return (publicKey, privateKey);
        }


        // Save key pair to files (for system initialization only)
        // <param name="publicKeyPath">Path to save public key</param>
        // <param name="privateKeyPath">Path to save private key</param>
        public static void SaveKeyPairToFiles(string publicKeyPath, string privateKeyPath)
        {
            var (publicKey, privateKey) = GenerateKeyPair();

            File.WriteAllText(publicKeyPath, publicKey);
            File.WriteAllText(privateKeyPath, privateKey);

            Console.WriteLine($"Public key saved to: {publicKeyPath}");
            Console.WriteLine($"Private key saved to: {privateKeyPath}");
            Console.WriteLine("⚠️ Keep the private key secure and set appropriate file permissions!");
        }
    }
}