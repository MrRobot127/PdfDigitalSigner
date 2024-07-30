using System;
using System.IO;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Crypto;
using iText.Kernel.Geom;
using System.Collections.Generic;
using System.Text;
using iText.Bouncycastle.Crypto;
using iText.Bouncycastle.X509;
using iText.Commons.Bouncycastle.Cert;
using iText.Forms.Fields.Properties;
using iText.Forms.Form.Element;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Layout.Font;
using iText.IO.Font;
using iText.Kernel.Pdf.Canvas.Parser.Filter;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;

namespace PdfDigitalSigner.Helpers
{
    //This is the iText7 Implementation
    public class PdfSigner2
    {
        static string GetDirectory(string flag)
        {
#if DEBUG
            int levels = 3;
#else
            int levels = 1;
#endif
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            for (int i = 0; i < levels; i++)
            {
                basePath = Directory.GetParent(basePath).FullName;
            }
            return System.IO.Path.Combine(basePath, flag);
        }

        public static MemoryStream EncryptAndSignPdf(MemoryStream pdfStream, List<int> pagesToSign, string LetterType, string pdfPassword, string[] certificateDetails)
        {
            try
            {
                MemoryStream signedPdfStrem = new MemoryStream();

                //string pfxFileName = "e-Mudhra Sub CA for Class 2 Document Signer 2022.pfx";
                //string pfxFilePath = System.IO.Path.Combine(GetDirectory("Security"), pfxFileName);

                //string pfxPassword = "emudhra";

                string pfxFilePath = certificateDetails[0];

                string pfxPassword = certificateDetails[1];

                if (!File.Exists(pfxFilePath))
                    throw new FileNotFoundException("Signing Certificate Not Found.");

                using (var fs = new FileStream(pfxFilePath, FileMode.Open, FileAccess.Read))
                {
                    Pkcs12Store pk12 = new Pkcs12StoreBuilder().Build();

                    pk12.Load(new FileStream(pfxFilePath, FileMode.Open, FileAccess.Read), pfxPassword.ToCharArray());

                    string alias = FindPrivateKeyAlias(pk12);

                    ICipherParameters pk = pk12.GetKey(alias).Key;

                    X509CertificateEntry[] ce = pk12.GetCertificateChain(alias);

                    X509Certificate[] chain = new X509Certificate[ce.Length];

                    for (int k = 0; k < ce.Length; ++k)
                    {
                        chain[k] = ce[k].Certificate;
                    }

                    MemoryStream encryptedPdfStream = EncryptPdf(new MemoryStream(pdfStream.ToArray()), pdfPassword);

                    PdfReader pdfReader;
                    signedPdfStrem = encryptedPdfStream;

                    bool areFourLinesInParagraph = false;

                    foreach (var page in pagesToSign)
                    {
                        pdfReader = new PdfReader(new MemoryStream(signedPdfStrem.ToArray()), new ReaderProperties().SetPassword(Encoding.UTF8.GetBytes(pdfPassword)));

                        if (page == 1)
                        {
                            string text = ReadTextFromLocation(pdfReader, new Rectangle(45, 505, 15, 15));

                            if (!string.IsNullOrWhiteSpace(text))
                                areFourLinesInParagraph = true;

                            pdfReader = new PdfReader(new MemoryStream(signedPdfStrem.ToArray()), new ReaderProperties().SetPassword(Encoding.UTF8.GetBytes(pdfPassword)));
                        }
                        signedPdfStrem = SignPages(pdfReader, pk, chain, page, LetterType, areFourLinesInParagraph);
                    }
                }

                return signedPdfStrem;
            }
            catch
            {
                throw;
            }
        }

        static string ReadTextFromLocation(PdfReader reader, Rectangle rect)
        {
            try
            {

                using (PdfDocument pdfDoc = new PdfDocument(reader))
                {
                    PdfPage page = pdfDoc.GetFirstPage();

                    // Create a TextRegionEventFilter with the specified rectangle
                    TextRegionEventFilter filter = new TextRegionEventFilter(rect);

                    // Create a FilteredTextEventListener with the TextRegionEventFilter and LocationTextExtractionStrategy
                    FilteredTextEventListener listener = new FilteredTextEventListener(new LocationTextExtractionStrategy(), filter);

                    // Extract text from the page using the FilteredTextEventListener
                    string text = PdfTextExtractor.GetTextFromPage(page, listener);

                    return text;
                }

            }
            catch
            {
                throw;
            }
        }

        public static MemoryStream SignPages(PdfReader reader, ICipherParameters pk, X509Certificate[] chain, int pageNo, string LetterType, bool areFourLinesInParagraph)
        {
            try
            {
                MemoryStream signedPdfStream = new MemoryStream();

                PdfSigner signer = new PdfSigner(reader, signedPdfStream, new StampingProperties().UseAppendMode());
                signer.SetFieldName("Sign" + (pageNo == 3 ? 2 : pageNo));
                signer.SetLocation("Gurugram");
                signer.SetReason("Tanu Kumar (HR Head)");

                Rectangle rect = new Rectangle(36, 648, 180, 100);
                if (pageNo == 1)
                {
                    if ((LetterType == "Structure Change Letter" || LetterType == "Increment Letter" || LetterType == "Promotion & Increment Letter") && !areFourLinesInParagraph)
                        rect = new Rectangle(45, 335, 160, 60);
                    else if ((LetterType == "Structure Change Letter" || LetterType == "Increment Letter" || LetterType == "Promotion & Increment Letter") && areFourLinesInParagraph)
                        rect = new Rectangle(45, 320, 160, 60);

                }
                else if (pageNo == 3)
                {
                    if (LetterType == "Increment Letter" || LetterType == "Structure Change Letter")
                        rect = new Rectangle(45, 555, 160, 60);
                    else if (LetterType == "Promotion & Increment Letter")
                        rect = new Rectangle(45, 613, 160, 60);
                }

                signer.SetPageRect(rect);
                signer.SetPageNumber(pageNo);

                SignatureFieldAppearance appearance = new SignatureFieldAppearance("app");
                //Customizing the text style

                SignedAppearanceText appearanceText = new SignedAppearanceText();

                appearanceText.SetReasonLine("" + signer.GetReason());

                appearance.SetContent(null, appearanceText);

                FontProvider fontProvider = new FontProvider();

                string[] fontsPath = {
                System.IO.Path.Combine(GetDirectory("Fonts"), "ARIAL.TTF"),
                //System.IO.Path.Combine(GetDirectory("Fonts"), "ARIALBD.TTF"),
                //System.IO.Path.Combine(GetDirectory("Fonts"), "ARIALBI.TTF")
                };

                PdfFont pdfFont = PdfFontFactory.CreateFont(StandardFonts.COURIER);

                foreach (string fontPath in fontsPath)
                {
                    FontProgram fontProgram = FontProgramFactory.CreateFont(fontPath);

                    pdfFont = PdfFontFactory.CreateFont(fontProgram, PdfEncodings.IDENTITY_H);
                }

                appearance.SetFont(pdfFont);
                //appearance.SetFontColor(new DeviceRgb(0xF9, 0x9D, 0x25));//orange
                appearance.SetFontColor(new DeviceRgb(0x00, 0x00, 0x00)); // Black color

                appearance.SetFontSize(9);

                signer.SetSignatureAppearance(appearance);

                IExternalSignature pks = new PrivateKeySignature(new PrivateKeyBC(pk), DigestAlgorithms.SHA256);

                IX509Certificate[] certificateWrappers = new IX509Certificate[chain.Length];
                for (int i = 0; i < certificateWrappers.Length; ++i)
                {
                    certificateWrappers[i] = new X509CertificateBC(chain[i]);
                }

                signer.SignDetached(pks, certificateWrappers, null, null, null, 0, iText.Signatures.PdfSigner.CryptoStandard.CMS);

                return signedPdfStream;
            }
            catch
            {
                throw;
            }
        }

        public static MemoryStream EncryptPdf(MemoryStream pdfStream, string pdfPassword)
        {
            try
            {
                using (MemoryStream encryptedPdfStream = new MemoryStream())
                {

                    PdfReader reader = new PdfReader(pdfStream);
                    byte[] userPassword = System.Text.Encoding.UTF8.GetBytes(pdfPassword);
                    byte[] ownerPassword = System.Text.Encoding.UTF8.GetBytes(pdfPassword);
                    int permissions = EncryptionConstants.ALLOW_SCREENREADERS;

                    PdfWriter writer = new PdfWriter(encryptedPdfStream, new WriterProperties().SetStandardEncryption(userPassword, ownerPassword, permissions, EncryptionConstants.ENCRYPTION_AES_256));
                    PdfDocument pdfDoc = new PdfDocument(reader, writer);
                    pdfDoc.Close();

                    return encryptedPdfStream;
                }
            }
            catch
            {
                throw;
            }
        }

        static string FindPrivateKeyAlias(Pkcs12Store pk12)
        {
            foreach (string al in pk12.Aliases)
            {
                if (pk12.IsKeyEntry(al) && pk12.GetKey(al).Key.IsPrivate)
                {
                    return al;
                }
            }
            throw new Exception("Private key not found in the certificate store.");
        }
    }
}
