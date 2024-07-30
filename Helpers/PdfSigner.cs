
//using iText.Kernel.Pdf;

//This was written using iTextSharp , now using iText7 so commenting below code and remving iTextSharp


//using iTextSharp.text.pdf.security;
//using iTextSharp.text.pdf;
//using iTextSharp.text;
//using Org.BouncyCastle.Crypto.Parameters;
//using Org.BouncyCastle.Pkcs;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;

//using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

//namespace PdfDigitalSigner.Helpers
//{
//    public class PdfSigner
//    {
//        public static MemoryStream EncryptAndSignPdf(MemoryStream pdfStream, List<int> pagesToSign, string password = "owner")
//        {
//            MemoryStream signedPdfStrem = new MemoryStream();
//            try
//            {
//                string basePath = AppDomain.CurrentDomain.BaseDirectory;
//                string parentDirectory = Directory.GetParent(basePath).Parent.Parent.FullName;

//                string pfxFilePath = Path.Combine(parentDirectory, "Security", "certificate.pfx");
//                string pfxPassword = "1234";

//                if (!File.Exists(pfxFilePath))
//                {
//                    throw new FileNotFoundException("Signing Certificate Not Found.");
//                }

//                using (var fs = new FileStream(pfxFilePath, FileMode.Open, FileAccess.Read))
//                {
//                    // Load the PFX certificate
//                    Pkcs12Store store = new Pkcs12Store(fs, pfxPassword.ToCharArray());

//                    // Searching for private key alias
//                    string alias = FindPrivateKeyAlias(store);

//                    // Retrieve private key and certificate chain
//                    AsymmetricKeyEntry pk = store.GetKey(alias);
//                    ICollection<X509Certificate> chain = GetCertificateChain(store, alias);

//                    // Retrieve private key parameters
//                    RsaPrivateCrtKeyParameters parameters = pk.Key as RsaPrivateCrtKeyParameters;

//                    MemoryStream encryptedPdfStrem = EncryptPdf(new MemoryStream(pdfStream.ToArray()), password);

//                    PdfReader pdfReader;
//                    signedPdfStrem = encryptedPdfStrem;

//                    foreach (var page in pagesToSign)
//                    {
//                        PdfReader reader = new PdfReader(new signedPdfStrem(signedPdfStrem.ToArray()), Encoding.UTF8.GetBytes(password));

//                        signedPdfStrem = SignPages(reader, parameters, chain, page);
//                    }

//                }
//            }
//            catch
//            {
//                throw new Exception("Some Error Occured While Signing Pdf.");
//            }

//            return signedPdfStrem;

//        }


//        private static MemoryStream SignPages(PdfReader reader, RsaPrivateCrtKeyParameters parameters, ICollection<X509Certificate> chain, int page)
//        {
//            MemoryStream signedPdfStream = new MemoryStream();

//            using (PdfStamper stamper = PdfStamper.CreateSignature(reader, signedPdfStream, '\0', null, true))
//            {
//                // Set signature appearance
//                PdfSignatureAppearance appearance = stamper.SignatureAppearance;
//                //appearance.Reason = "Test Signing";
//                //appearance.Location = "Test Location";
//                appearance.SetVisibleSignature(new Rectangle(100, 390, 250, 600), page, "sig" + page);
//                appearance.Acro6Layers = true;

//                // Create the signature
//                IExternalSignature pks = new PrivateKeySignature(parameters, DigestAlgorithms.SHA256);
//                MakeSignature.SignDetached(appearance, pks, chain, null, null, null, 0, CryptoStandard.CMS);
//            }

//            return signedPdfStream;
//        }

//        public static MemoryStream EncryptPdf(MemoryStream pdfStream, string password)
//        {
//            using (MemoryStream encryptedPdfStream = new MemoryStream())
//            {

//                try
//                {
//                    pdfStream.Seek(0, SeekOrigin.Begin);
//                    PdfReader reader = new PdfReader(pdfStream);

//                    string userPassword = password;
//                    string ownerPassword = password;
//                    int permissions = PdfWriter.ALLOW_SCREENREADERS;

//                    PdfEncryptor.Encrypt(reader, encryptedPdfStream, true, userPassword, ownerPassword, permissions);
//                    return encryptedPdfStream;
//                }
//                catch
//                {
//                    throw new Exception("Some Error Occured While Encypting Pdf");
//                }
//            }
//        }

//        static string FindPrivateKeyAlias(Pkcs12Store store)
//        {
//            foreach (string al in store.Aliases)
//            {
//                if (store.IsKeyEntry(al) && store.GetKey(al).Key.IsPrivate)
//                {
//                    return al;
//                }
//            }
//            throw new Exception("Private key not found in the certificate store.");
//        }

//        static ICollection<X509Certificate> GetCertificateChain(Pkcs12Store store, string alias)
//        {
//            ICollection<X509Certificate> chain = new List<X509Certificate>();
//            foreach (X509CertificateEntry c in store.GetCertificateChain(alias))
//            {
//                chain.Add(c.Certificate);
//            }
//            return chain;
//        }
//    }
//}
