using PdfDigitalSigner.Models;
using System.Collections.Generic;
using System.IO;
using System;
using iText.Kernel.Pdf;
using iText.Html2pdf;
using iText.IO.Font;
using iText.Layout.Font;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using iText.Html2pdf.Resolver.Font;

namespace PdfDigitalSigner.Helpers
{
    public class PdfGenerator
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
            return Path.Combine(basePath, flag);
        }

        public static bool GeneratePDF(UserDataModel userDataModel, string[] certificateDetails)
        {
            try
            {
                bool isSuccess = false;

                string headerImagePath = Path.Combine(GetDirectory("Images"), "HeaderLogoEUI.png");
                byte[] headerImageBytes = File.ReadAllBytes(headerImagePath);

                string headerBase64String = Convert.ToBase64String(headerImageBytes);

                string[] fontsPath = {
                    Path.Combine(GetDirectory("Fonts"), "ARIAL.TTF"),
                    Path.Combine(GetDirectory("Fonts"), "ARIALBD.TTF"),
                    Path.Combine(GetDirectory("Fonts"), "ARIALBI.TTF")
                };

                int totalLettersToGenerate = 0;
                if (userDataModel.SelectedEmployee != null)
                {
                    if (userDataModel.LetterType == "Select All")
                    {
                        totalLettersToGenerate = userDataModel.SelectedEmployee.Count();
                    }
                    else
                    {
                        totalLettersToGenerate = userDataModel.SelectedEmployee
                            .Count(employee => employee.LetterType == userDataModel.LetterType);
                    }
                }
                int lettersGenerated = 0;

                foreach (var employee in userDataModel.SelectedEmployee)
                {

                    string content = "";
                    string outputFilePath = "";

                    if (employee.LetterType == "Increment Letter" && (userDataModel.LetterType == "Increment Letter" || userDataModel.LetterType == "Select All"))
                    {
                        string outputFolder = Path.Combine(userDataModel.OutputLocation, employee.LetterType);

                        if (!Directory.Exists(outputFolder))
                        {
                            Directory.CreateDirectory(outputFolder);
                        }

                        outputFilePath = Path.Combine(outputFolder, $"PaySlip-{employee.ECN}({employee.FirstName}).pdf");

                        string letterPath = Path.Combine(GetDirectory("Templates"), employee.LetterType + ".html");

                        string letterContent = string.Empty;

                        if (File.Exists(letterPath))
                            letterContent = File.ReadAllText(letterPath);
                        else
                            throw new FileNotFoundException($"{employee.LetterType} Template Not Found.");

                        #region content
                        content = letterContent.Replace("{{Structure_Effective_Date}}", FormatDateWithOrdinalSuffix(employee.StructureEffectiveDate))
                                               .Replace("{{Letter_Issue_Date}}", FormatDateWithOrdinalSuffix(employee.LetterIssueDate))
                                               .Replace("{{First_Name}}", employee.FirstName)
                                               .Replace("{{DESIGNATION}}", employee.Designation)
                                               .Replace("{{ECN}}", employee.ECN)
                                               .Replace("{{Email_ID}}", employee.EmailID)
                                               .Replace("{{Amount_Desc}}", employee.AmountDesc)
                                               .Replace("{{Total}}", employee.Total)

                                               .Replace("{{Basic_m}}", employee.BasicMonthly)
                                               .Replace("{{Basic_y}}", employee.BasicY)

                                               .Replace("{{PF_m}}", employee.PFM)
                                               .Replace("{{PF_y}}", employee.YearlyPF)

                                               .Replace("{{Statutory Bonus_m}}", employee.StatutoryBonusM)
                                               .Replace("{{Statutory Bonus_y}}", employee.StatutoryBonusY)

                                               .Replace("{{HRA_m}}", employee.HRAM)
                                               .Replace("{{HRA_y}}", employee.HRAY)

                                               .Replace("{{Telephone_m}}", employee.TelephoneM)
                                               .Replace("{{Telephone_y}}", employee.TelephoneY)

                                               .Replace("{{Mobile_Handset_y}}", employee.MobileHandsetY)

                                               .Replace("{{LTA_m}}", employee.LTAM)
                                               .Replace("{{LTA_y}}", employee.LTAY)

                                               .Replace("{{Child_Education_m}}", employee.ChildEducationM)
                                               .Replace("{{Child_Education_y}}", employee.ChildEducationY)

                                               .Replace("{{Professional_Development_y}}", employee.ProfessionalDevelopmentY)

                                               .Replace("{{CarBike_m}}", employee.CarBikeM)
                                               .Replace("{{CarBike_y}}", employee.CarBikeY)

                                               .Replace("{{Meal_m}}", employee.MealM)
                                               .Replace("{{Meal_y}}", employee.MealY)

                                               .Replace("{{Gift_y}}", employee.GiftY)

                                               .Replace("{{Special_m}}", employee.SpecialM)
                                               .Replace("{{Special_y}}", employee.SpecialY)

                                               .Replace("{{Monthly}}", employee.Monthly)

                                               .Replace("{{Subtotal}}", employee.Subtotal)

                                               .Replace("{{Bonus}}", employee.Bonus)

                                               .Replace("{{Gratuity}}", employee.Gratuity)

                                               .Replace("{{Total}}", employee.Total)

                                               .Replace("{{headerBase64String}}", headerBase64String)

                                               ;
                        #endregion
                    }

                    else if (employee.LetterType == "Structure Change Letter" && (userDataModel.LetterType == "Structure Change Letter" || userDataModel.LetterType == "Select All"))
                    {
                        string outputFolder = Path.Combine(userDataModel.OutputLocation, employee.LetterType);

                        if (!Directory.Exists(outputFolder))
                        {
                            Directory.CreateDirectory(outputFolder);
                        }

                        outputFilePath = Path.Combine(outputFolder, $"PaySlip-{employee.ECN}({employee.FirstName}).pdf");

                        string letterPath = Path.Combine(GetDirectory("Templates"), employee.LetterType + ".html");

                        string letterContent = string.Empty;

                        if (File.Exists(letterPath))
                            letterContent = File.ReadAllText(letterPath);
                        else
                            throw new FileNotFoundException($"{employee.LetterType} Template Not Found.");

                        #region content
                        content = letterContent.Replace("{{Structure_Effective_Date}}", FormatDateWithOrdinalSuffix(employee.StructureEffectiveDate))
                                               .Replace("{{Letter_Issue_Date}}", FormatDateWithOrdinalSuffix(employee.LetterIssueDate))
                                               .Replace("{{First_Name}}", employee.FirstName)
                                               .Replace("{{DESIGNATION}}", employee.Designation)
                                               .Replace("{{ECN}}", employee.ECN)
                                               .Replace("{{Email_ID}}", employee.EmailID)
                                               .Replace("{{Amount_Desc}}", employee.AmountDesc)
                                               .Replace("{{Total}}", employee.Total)

                                               .Replace("{{Basic_m}}", employee.BasicMonthly)
                                               .Replace("{{Basic_y}}", employee.BasicY)

                                               .Replace("{{PF_m}}", employee.PFM)
                                               .Replace("{{PF_y}}", employee.YearlyPF)

                                               .Replace("{{Statutory_Bonus_m}}", employee.StatutoryBonusM)
                                               .Replace("{{Statutory_Bonus_y}}", employee.StatutoryBonusY)

                                               .Replace("{{HRA_m}}", employee.HRAM)
                                               .Replace("{{HRA_y}}", employee.HRAY)

                                               .Replace("{{Telephone_m}}", employee.TelephoneM)
                                               .Replace("{{Telephone_y}}", employee.TelephoneY)

                                               .Replace("{{Mobile_Handset_y}}", employee.MobileHandsetY)

                                               .Replace("{{LTA_m}}", employee.LTAM)
                                               .Replace("{{LTA_y}}", employee.LTAY)

                                               .Replace("{{Child_Education_m}}", employee.ChildEducationM)
                                               .Replace("{{Child_Education_y}}", employee.ChildEducationY)

                                               .Replace("{{Professional_Development_y}}", employee.ProfessionalDevelopmentY)

                                               .Replace("{{CarBike_m}}", employee.CarBikeM)
                                               .Replace("{{CarBike_y}}", employee.CarBikeY)

                                               .Replace("{{Meal_m}}", employee.MealM)
                                               .Replace("{{Meal_y}}", employee.MealY)

                                               .Replace("{{Gift_y}}", employee.GiftY)

                                               .Replace("{{Special_m}}", employee.SpecialM)
                                               .Replace("{{Special_y}}", employee.SpecialY)

                                               .Replace("{{Monthly}}", employee.Monthly)

                                               .Replace("{{Subtotal}}", employee.Subtotal)

                                               .Replace("{{Bonus}}", employee.Bonus)

                                               .Replace("{{Gratuity}}", employee.Gratuity)

                                               .Replace("{{Total}}", employee.Total)

                                               .Replace("{{headerBase64String}}", headerBase64String)

                                               ;
                        #endregion

                    }

                    else if (employee.LetterType == "Promotion & Increment Letter" && (userDataModel.LetterType == "Promotion & Increment Letter" || userDataModel.LetterType == "Select All"))
                    {
                        string outputFolder = Path.Combine(userDataModel.OutputLocation, employee.LetterType);

                        if (!Directory.Exists(outputFolder))
                        {
                            Directory.CreateDirectory(outputFolder);
                        }

                        outputFilePath = Path.Combine(outputFolder, $"PaySlip-{employee.ECN}({employee.FirstName}).pdf");

                        string letterPath = Path.Combine(GetDirectory("Templates"), employee.LetterType + ".html");

                        string letterContent = string.Empty;

                        if (File.Exists(letterPath))
                            letterContent = File.ReadAllText(letterPath);
                        else
                            throw new FileNotFoundException($"{employee.LetterType} Template Not Found.");

                        #region content
                        content = letterContent.Replace("{{Structure_Effective_Date}}", FormatDateWithOrdinalSuffix(employee.StructureEffectiveDate))
                                               .Replace("{{Letter_Issue_Date}}", FormatDateWithOrdinalSuffix(employee.LetterIssueDate))
                                               .Replace("{{First_Name}}", employee.FirstName)
                                               .Replace("{{DESIGNATION}}", employee.Designation)
                                               .Replace("{{ECN}}", employee.ECN)
                                               .Replace("{{Email_ID}}", employee.EmailID)
                                               .Replace("{{Amount_Desc}}", employee.AmountDesc)
                                               .Replace("{{Total}}", employee.Total)

                                               .Replace("{{Basic_m}}", employee.BasicMonthly)
                                               .Replace("{{Basic_y}}", employee.BasicY)

                                               .Replace("{{PF_m}}", employee.PFM)
                                               .Replace("{{PF_y}}", employee.YearlyPF)

                                               .Replace("{{Statutory_Bonus_m}}", employee.StatutoryBonusM)
                                               .Replace("{{Statutory_Bonus_y}}", employee.StatutoryBonusY)

                                               .Replace("{{HRA_m}}", employee.HRAM)
                                               .Replace("{{HRA_y}}", employee.HRAY)

                                               .Replace("{{Telephone_m}}", employee.TelephoneM)
                                               .Replace("{{Telephone_y}}", employee.TelephoneY)

                                               .Replace("{{Mobile_Handset_y}}", employee.MobileHandsetY)

                                               .Replace("{{LTA_m}}", employee.LTAM)
                                               .Replace("{{LTA_y}}", employee.LTAY)

                                               .Replace("{{Child_Education_m}}", employee.ChildEducationM)
                                               .Replace("{{Child_Education_y}}", employee.ChildEducationY)

                                               .Replace("{{Professional_Development_y}}", employee.ProfessionalDevelopmentY)

                                               .Replace("{{CarBike_m}}", employee.CarBikeM)
                                               .Replace("{{CarBike_y}}", employee.CarBikeY)

                                               .Replace("{{Meal_m}}", employee.MealM)
                                               .Replace("{{Meal_y}}", employee.MealY)

                                               .Replace("{{Gift_y}}", employee.GiftY)

                                               .Replace("{{Special_m}}", employee.SpecialM)
                                               .Replace("{{Special_y}}", employee.SpecialY)

                                               .Replace("{{Monthly}}", employee.Monthly)

                                               .Replace("{{Subtotal}}", employee.Subtotal)

                                               .Replace("{{Bonus}}", employee.Bonus)

                                               .Replace("{{Gratuity}}", employee.Gratuity)

                                               .Replace("{{Total}}", employee.Total)

                                               .Replace("{{headerBase64String}}", headerBase64String)

                                               ;
                        #endregion

                    }

                    if (content != "" && outputFilePath != "")
                    {
                        MemoryStream pdfStream = ConvertHtmlToPdf(content, fontsPath);

                        List<int> pagesToSign = new List<int> { 1, 3 };

                        string panNo = employee.PanNo;

                        if (string.IsNullOrEmpty(panNo))
                            throw new Exception("PanNo Not Found.");

                        MemoryStream signedPdfStream = PdfSigner2.EncryptAndSignPdf(pdfStream, pagesToSign, employee.LetterType, panNo.ToLower(), certificateDetails);

                        File.WriteAllBytes(outputFilePath, signedPdfStream.ToArray());

                        isSuccess = true;

                        lettersGenerated++;

                        WeakReferenceMessenger.Default.Send($"{lettersGenerated} Letter generated out of {totalLettersToGenerate}");
                    }

                }
                return isSuccess;
            }
            catch
            {
                throw;
            }
        }

        public static string FormatDateWithOrdinalSuffix(string date)
        {
            // Split the date string into parts
            string[] parts = date.Split(' ');

            // Extract the day part and remove the suffix
            string dayPart = parts[0].Substring(0, parts[0].Length - 2);

            // Parse the day part into an integer
            int day = int.Parse(dayPart);

            // Determine the ordinal suffix
            string suffix;
            if (day % 100 >= 11 && day % 100 <= 13)
            {
                suffix = "th";
            }
            else
            {
                switch (day % 10)
                {
                    case 1:
                        suffix = "st";
                        break;
                    case 2:
                        suffix = "nd";
                        break;
                    case 3:
                        suffix = "rd";
                        break;
                    default:
                        suffix = "th";
                        break;
                }
            }

            // Reconstruct the day part with the suffix
            string formattedDay = $"{day}<sup>{suffix}</sup>";

            // Reconstruct the date string with the formatted day part and the rest of the parts
            string formattedDate = $"{formattedDay} {string.Join(" ", parts, 1, parts.Length - 1)}";

            return formattedDate;
        }

        static MemoryStream ConvertHtmlToPdf(string incrementLetterContent, string[] fontsPath)
        {
            try
            {
                MemoryStream pdfStream = new MemoryStream();

                PdfWriter writer = new PdfWriter(pdfStream);

                var pdf = new PdfDocument(writer);

                pdf.SetDefaultPageSize(iText.Kernel.Geom.PageSize.A4);

                ConverterProperties converterProperties = new ConverterProperties();

                FontProvider fontProvider = new DefaultFontProvider();


                FontProgram fontProgram;

                foreach (string fontPath in fontsPath)
                {
                    fontProgram = FontProgramFactory.CreateFont(fontPath);

                    fontProvider.AddFont(fontProgram, PdfEncodings.IDENTITY_H);
                }
                converterProperties.SetFontProvider(fontProvider);

                HtmlConverter.ConvertToPdf(incrementLetterContent, pdfStream, converterProperties);

                return pdfStream;
            }
            catch
            {
                throw;
            }
        }

        //Below using iTextSharp
        //private static MemoryStream ConvertHtmlToPdf(string htmlContent)
        //{
        //    MemoryStream pdfStream = new MemoryStream();

        //    using (var document = new Document(PageSize.A4))
        //    {
        //        PdfWriter pdfWriter = PdfWriter.GetInstance(document, pdfStream);
        //        document.Open();

        //        var htmlWorker = new HTMLWorker(document);
        //        htmlWorker.Parse(new StringReader(htmlContent));

        //        document.Close();
        //    }

        //    return pdfStream;
        //}
    }
}
