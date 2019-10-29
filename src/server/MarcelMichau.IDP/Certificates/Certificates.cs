using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace MarcelMichau.IDP.Certificates
{
    public static class Certificates
    {
        public static X509Certificate2 Get()
        {
            const string certificatePath = "/app/certs/identitykeymaterial.pfx";

            if (!File.Exists(certificatePath))
                throw new FileNotFoundException($"Key material not found at path: {certificatePath}");

            return new X509Certificate2(certificatePath);
        }
    }
}
