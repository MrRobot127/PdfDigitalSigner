using ClosedXML.Excel;
using CommunityToolkit.Mvvm.Messaging;
using PdfDigitalSigner.Helpers;
using PdfDigitalSigner.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Excel = Microsoft.Office.Interop.Excel;

namespace PdfDigitalSigner.UserControls
{
    public partial class ExcelDataUploadAndPreview : UserControl
    {
        public ExcelDataUploadAndPreview()
        {
            InitializeComponent();
            DataContext = this;
            EmployeeUI = new ObservableCollection<EmployeeData>();

            WeakReferenceMessenger.Default.Register<string, string>(this, "UpdateProgressBar", (recipient, message) => UpdateProgressBarText(message));

        }

        private void UpdateProgressBarText(string message)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressBarTextBlock.Text = message;
            });
        }

        public ObservableCollection<EmployeeData> EmployeeUI { get; set; }

        public List<EmployeeData> EmployeeList { get; set; }

        private string[] GetCertificateDetails()
        {
            var certificateWindow = new CertificateDetailWindow();

            bool? result = certificateWindow.ShowDialog();

            string[] certificateDetails = new string[2];

            if (result == true)
            {
                certificateDetails[0] = certificateWindow.CertificateFilePath;

                certificateDetails[1] = certificateWindow.CertificatePassword;

                return certificateDetails;
            }
            return null;
        }

        private async void GenerateLetterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (OutputLocationTextBlock.Text == "")
                {
                    MessageBoxHelper.ShowError("Please Select Output Location.");
                    return;
                }

                if (!EmployeeUI.Any(x => x.IsSelected))
                {
                    MessageBoxHelper.ShowError("Please Select at least One Employee.");
                    return;
                }


                UserDataModel userDataModel = new UserDataModel();
                userDataModel.LetterType = (LetterTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

                userDataModel.SelectedEmployee = EmployeeList
                                                 .Where(m => EmployeeUI.Any(x => x.IsSelected && x.FirstName == m.FirstName && x.ECN == m.ECN))
                                                 .ToList();

                userDataModel.OutputLocation = OutputLocationTextBlock.Text;

                bool any = true;

                if (userDataModel.LetterType != "Select All")
                    any = userDataModel.SelectedEmployee.Any(m => m.LetterType == userDataModel.LetterType);

                if (!any)
                {
                    MessageBoxHelper.ShowError("Selected Mapping not found in uploaded Excel.");
                    return;
                }

                bool isPanNoMissingForAnyOfTheEmployee = false;

                if (userDataModel.SelectedEmployee.Any(m => m.PanNo == null || m.PanNo == ""))
                    isPanNoMissingForAnyOfTheEmployee = true;

                if (isPanNoMissingForAnyOfTheEmployee)
                {
                    MessageBoxHelper.ShowError("Please update the PanNo in the Excel.");
                    return;
                }

                if (userDataModel.SelectedEmployee.Any())
                {
                    string[] certificateDetails = GetCertificateDetails();

                    if (certificateDetails != null && certificateDetails.Length == 2)
                    {
                        ToggleProgressBarVisibility(true, "Generating Letters ...");

                        EnableControls(false);

                        bool isSuccess = await Task.Run(() => PdfGenerator.GeneratePDF(userDataModel, certificateDetails));

                        if (isSuccess)
                        {
                            await Application.Current.Dispatcher.InvokeAsync(() =>
                            {
                                ToggleProgressBarVisibility(false);
                                MessageBoxHelper.ShowSuccess("Letters Generated Successfully.");
                            });
                        }
                        ToggleProgressBarVisibility(false);
                        EnableControls(true);
                    }
                    else
                    {
                        throw new Exception("Please Enter Certificate Details to Proceed.");
                    }
                }
                else
                {
                    ToggleProgressBarVisibility(false);
                    EnableControls(true);
                    MessageBoxHelper.ShowError("Please Select Employee.");
                }
            }
            catch (Exception ex)
            {
                ToggleProgressBarVisibility(false);
                EnableControls(true);
                MessageBoxHelper.ShowError(ex.Message);
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UploadExcelSubmitButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void EyeToggleButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PasswordUnmask.Visibility = Visibility.Visible;
            PasswordUnmask.Text = PasswordBox.Password;
            PasswordBox.Visibility = Visibility.Collapsed;
        }

        private void EyeToggleButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            PasswordBox.Visibility = Visibility.Visible;
            PasswordUnmask.Visibility = Visibility.Collapsed;
            PasswordUnmask.Text = string.Empty;
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Excel Files (*.xlsx, *.xlsm)|*.xlsx;*.xlsm";
            openFileDialog.DefaultExt = ".xlsx";
            openFileDialog.Title = "Please Choose Excel";

            if (openFileDialog.ShowDialog().HasValue)
            {
                string selectedFileName = openFileDialog.FileName;
                string extension = System.IO.Path.GetExtension(selectedFileName);
                if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xlsm")
                {
                    FileNameTextBlock.Text = selectedFileName;
                }
                else
                {
                    MessageBoxHelper.ShowError("Please select an Excel file (.xlsx).");
                }
            }

        }

        public string OutputLocation { get; set; }

        private void OutPutButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    OutputLocationTextBlock.Text = dialog.SelectedPath;
                    OutputLocation = dialog.SelectedPath;
                    OutputLocationTextBlock.Visibility = Visibility.Visible;
                }
            }
        }

        private void ResetUserSelection()
        {
            UserSelection.Visibility = Visibility.Hidden;
            SelectAllEmployeeCheckBox.IsChecked = false;
            LetterTypeComboBox.SelectedIndex = 0;
        }

        private async void UploadExcelSubmitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ResetUserSelection();

                if (string.IsNullOrEmpty(FileNameTextBlock.Text) || FileNameTextBlock.Text == "No File Chosen")
                {
                    MessageBoxHelper.ShowError("Please select an Excel file first.");
                    return;
                }

                string selectedFilePath = FileNameTextBlock.Text;
                string password = PasswordBox.Password;

                await Task.Run(() => ProcessExcel(selectedFilePath, password));
            }
            catch (Exception ex)
            {
                ToggleProgressBarVisibility(false);
                EnableControls(true);
                MessageBoxHelper.ShowError(ex.Message);
            }
        }

        private void EnableControls(bool isEnabled)
        {
            Dispatcher.Invoke(() =>
            {
                UploadButton.IsEnabled = isEnabled;
                UploadExcelSubmitButton.IsEnabled = isEnabled;
                PasswordBox.IsEnabled = isEnabled;
                PasswordUnmask.IsEnabled = isEnabled;
                EyeToggleButton.IsEnabled = isEnabled;

                if (UserSelection.Visibility == Visibility.Visible)
                {
                    SelectAllEmployeeCheckBox.IsEnabled = isEnabled;
                    LetterTypeComboBox.IsEnabled = isEnabled;
                    EmployeeTable.IsEnabled = isEnabled;
                    OutPutButton.IsEnabled = isEnabled;
                    GenerateLetterButton.IsEnabled = isEnabled;
                }
            });
        }

        private void ToggleProgressBarVisibility(bool isShow, string progreeBarText = "")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (isShow)
                {
                    ProgressBar.Visibility = Visibility.Visible;
                    ProgressBarTextBlock.Text = progreeBarText;
                }
                else
                {
                    ProgressBar.Visibility = Visibility.Collapsed;
                    ProgressBarTextBlock.Text = string.Empty;
                }
            });
        }

        private void ProcessExcel(string selectedFilePath, string password)
        {
            EnableControls(false);
            ToggleProgressBarVisibility(true, "Validating Excel...");

            Excel.Application interopExcelApp = null;
            Excel.Workbook interopWorkbook = null;

            string tempFileFullPath = "";

            try
            {
                interopExcelApp = new Excel.Application();
                interopWorkbook = interopExcelApp.Workbooks.Open(selectedFilePath, Password: password);

                string originalExtension = Path.GetExtension(selectedFilePath);
                string tempFilePath = Path.GetTempPath();
                string tempFileName = Path.GetFileNameWithoutExtension(selectedFilePath) + $"_Copy.{originalExtension}";
                tempFileFullPath = Path.Combine(tempFilePath, tempFileName);

                if (File.Exists(tempFileFullPath))
                {
                    File.Delete(tempFileFullPath);
                }

                interopWorkbook.SaveAs(tempFileFullPath, Password: "", WriteResPassword: Type.Missing);
                interopWorkbook.Close(false);

                List<EmployeeData> employeeList = new List<EmployeeData>();

                using (XLWorkbook closedXmlWorkbook = new XLWorkbook(tempFileFullPath))
                {
                    var worksheet = closedXmlWorkbook.Worksheet(1);

                    string[] expectedColumns = {
                    "Active Status", "ECN", "First Name", "Designation", "Email ID",
                    "Structure Effective Date","Letter Issue Date", "Letter Type", "Basic_m", "PF_m",
                    "Statutory Bonus_m", "HRA_m", "Telephone_m", "LTA_m",
                    "Child Education_m", "CarBike_m", "Meal_m", "Special_m",
                    "Monthly", "Basic Y", "Yearly PF", "Statutory Bonus_y",
                    "HRA_y", "Telephone_y", "Mobile Handset_y", "LTA_y",
                    "Child Education_y", "Professional Development_y",
                    "CarBike_y", "Meal_y", "Gift_y", "Special_y",
                    "Subtotal", "Bonus", "Gratuity", "Total",
                    "Amount_Desc", "Actual New CTC", "PanNo"
                    };

                    bool columnsMatch = false;
                    List<string> missingColumns = new List<string>();

                    (columnsMatch, missingColumns) = CheckExcelColumns(worksheet, expectedColumns);

                    if (!columnsMatch)
                    {
                        throw new Exception($"Uploaded Excel is not Valid as {string.Join(", ", missingColumns)} is Missing.");
                    }

                    ToggleProgressBarVisibility(true, "Copying Employee Data ...");

                    employeeList = GetEmployeeData(worksheet, expectedColumns).OrderBy(m => m.FirstName).ToList();
                }

                (bool isDuplicate, string duplicateNames) = CheckForDuplicates(employeeList);

                if (isDuplicate)
                {
                    throw new Exception(duplicateNames);
                }

                (bool isLetterTypeNameCorrect, string letterData) = CheckLetterTypeName(employeeList);

                if (!isLetterTypeNameCorrect)
                {
                    throw new Exception(letterData);
                }

                EmployeeList = employeeList;

                var distinctEmployee = employeeList.Select(m => new { m.FirstName, m.ECN }).Distinct().ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (distinctEmployee.Any())
                    {
                        //Storing data in such a way so that column wise binding can be done order by EmployeeName
                        AddEmployeeByCustomOrder(distinctEmployee);

                        UserSelection.Visibility = Visibility.Visible;
                    }
                });

                ToggleProgressBarVisibility(false);
                EnableControls(true);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (interopExcelApp != null)
                {
                    interopExcelApp.Quit();
                    Marshal.ReleaseComObject(interopExcelApp);
                }

                if (tempFileFullPath != "")
                {
                    if (File.Exists(tempFileFullPath))
                    {
                        try
                        {
                            File.Delete(tempFileFullPath);
                        }
                        catch
                        {

                        }
                    }

                }

            }
        }

        private void AddEmployeeByCustomOrder(dynamic distinctEmployee)
        {
            EmployeeUI.Clear();
            int columns = 4;
            int rowCount = (distinctEmployee.Count + columns - 1) / columns; // Calculate number of rows needed

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    int index = i + j * rowCount;

                    if (index < distinctEmployee.Count)
                    {
                        var employee = new EmployeeData
                        {
                            FirstName = distinctEmployee[index].FirstName,
                            ECN = distinctEmployee[index].ECN
                        };
                        EmployeeUI.Add(employee);
                    }
                    else
                    {
                        break; // If no more employees, exit inner loop
                    }
                }
            }
        }

        private (bool, string) CheckLetterTypeName(List<EmployeeData> employeeList)
        {
            var names = employeeList
                        .Where(m => m.LetterType != "Increment Letter" && m.LetterType != "Promotion & Increment Letter" && m.LetterType != "Structure Change Letter")
                        .ToList();

            if (names.Any())
            {
                var message = new StringBuilder();
                message.AppendLine("Letter Type Name should be either Increment Letter/Promotion & Increment Letter/Structure Change Letter. Please Correct LetterType Name For:");

                foreach (var name in names)
                {
                    var data = name.FirstName + "-" + name.ECN + "-" + name.LetterType;

                    message.AppendLine(data);
                }

                return (false, message.ToString());
            }
            return (true, "");
        }

        private (bool, string) CheckForDuplicates(List<EmployeeData> employeeList)
        {
            var duplicates = employeeList
                                .GroupBy(m => new { m.ECN, m.LetterType })
                                .Where(m => m.Count() > 1)
                                .Select(group => group.First())
                                .ToList();

            if (duplicates.Any())
            {
                var duplicate = new StringBuilder();

                duplicate.AppendLine("Duplicate Records Found:");

                foreach (var item in duplicates)
                {
                    var duplicateNames = item.FirstName + "-" + item.ECN + "-" + item.LetterType;

                    duplicate.AppendLine(duplicateNames);
                }
                return (true, duplicate.ToString());
            }

            return (false, "");
        }

        private List<EmployeeData> GetEmployeeData(IXLWorksheet worksheet, string[] expectedColumns)
        {
            int rowCount = worksheet.LastRowUsed().RowNumber();
            int columnCount = worksheet.LastColumnUsed().ColumnNumber();

            List<EmployeeData> employeeList = new List<EmployeeData>();

            Dictionary<string, int> columnIndexMap = new Dictionary<string, int>();

            for (int j = 1; j <= columnCount; j++)
            {
                string header = GetValue(worksheet.Cell(1, j));

                columnIndexMap[header] = j;
            }

            for (int i = 2; i <= rowCount; i++)
            {
                EmployeeData employee = new EmployeeData();

                foreach (string column in expectedColumns)
                {
                    if (columnIndexMap.TryGetValue(column, out int columnIndex))
                    {
                        string value = GetValue(worksheet.Cell(i, columnIndex));
                        SetPropertyValue(employee, column, value);
                    }
                }

                employeeList.Add(employee);

                WeakReferenceMessenger.Default.Send($"{i - 1} rows Copied out of {rowCount - 1}");
            }

            return employeeList;
        }

        private static void SetPropertyValue(EmployeeData employee, string propertyName, string value)
        {
            switch (propertyName)
            {
                case "Active Status":
                    employee.ActiveStatus = value;
                    break;
                case "ECN":
                    employee.ECN = value;
                    break;
                case "First Name":
                    employee.FirstName = value;
                    break;
                case "Designation":
                    employee.Designation = value;
                    break;
                case "Email ID":
                    employee.EmailID = value;
                    break;
                case "Structure Effective Date":
                    employee.StructureEffectiveDate = value;
                    break;
                case "Letter Issue Date":
                    employee.LetterIssueDate = value;
                    break;
                case "Letter Type":
                    employee.LetterType = value;
                    break;
                case "Basic_m":
                    employee.BasicMonthly = value;
                    break;
                case "PF_m":
                    employee.PFM = value;
                    break;
                case "Statutory Bonus_m":
                    employee.StatutoryBonusM = value;
                    break;
                case "HRA_m":
                    employee.HRAM = value;
                    break;
                case "Telephone_m":
                    employee.TelephoneM = value;
                    break;
                case "LTA_m":
                    employee.LTAM = value;
                    break;
                case "Child Education_m":
                    employee.ChildEducationM = value;
                    break;
                case "CarBike_m":
                    employee.CarBikeM = value;
                    break;
                case "Meal_m":
                    employee.MealM = value;
                    break;
                case "Special_m":
                    employee.SpecialM = value;
                    break;
                case "Monthly":
                    employee.Monthly = value;
                    break;
                case "Basic Y":
                    employee.BasicY = value;
                    break;
                case "Yearly PF":
                    employee.YearlyPF = value;
                    break;
                case "Statutory Bonus_y":
                    employee.StatutoryBonusY = value;
                    break;
                case "HRA_y":
                    employee.HRAY = value;
                    break;
                case "Telephone_y":
                    employee.TelephoneY = value;
                    break;
                case "Mobile Handset_y":
                    employee.MobileHandsetY = value;
                    break;
                case "LTA_y":
                    employee.LTAY = value;
                    break;
                case "Child Education_y":
                    employee.ChildEducationY = value;
                    break;
                case "Professional Development_y":
                    employee.ProfessionalDevelopmentY = value;
                    break;
                case "CarBike_y":
                    employee.CarBikeY = value;
                    break;
                case "Meal_y":
                    employee.MealY = value;
                    break;
                case "Gift_y":
                    employee.GiftY = value;
                    break;
                case "Special_y":
                    employee.SpecialY = value;
                    break;
                case "Subtotal":
                    employee.Subtotal = value;
                    break;
                case "Bonus":
                    employee.Bonus = value;
                    break;
                case "Gratuity":
                    employee.Gratuity = value;
                    break;
                case "Total":
                    employee.Total = value;
                    break;
                case "Amount_Desc":
                    employee.AmountDesc = value;
                    break;
                case "Actual New CTC":
                    employee.ActualNewCTC = value;
                    break;
                case "PanNo":
                    employee.PanNo = value;
                    break;
            }
        }

        private string GetValue(IXLCell cell)
        {
            if (cell != null && !cell.IsEmpty())
            {
                if (cell.DataType == XLDataType.Number)
                {
                    double numericValue = cell.GetDouble();
                    double roundedValue = Math.Round(numericValue, 0);
                    return roundedValue.ToString();

                }
                else
                {
                    return cell.GetString();
                }
            }
            else
            {
                return string.Empty;
            }
        }

        private (bool, List<string>) CheckExcelColumns(IXLWorksheet worksheet, string[] expectedColumns)
        {
            var headerRow = worksheet.Row(1);

            var excelColumns = headerRow.CellsUsed().Select(cell => cell.GetString()).ToList();

            var missingColumns = expectedColumns.Except(excelColumns).ToList();

            bool allColumnsExist = !missingColumns.Any();

            return (allColumnsExist, missingColumns);
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StackPanel stackPanel = FindVisualParent<StackPanel>((TextBlock)sender);

            CheckBox checkBox = stackPanel?.Children.OfType<CheckBox>().FirstOrDefault();

            if (checkBox != null)
            {
                checkBox.IsChecked = !checkBox.IsChecked;
            }
        }

        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            if (parentObject is T parent)
                return parent;

            return FindVisualParent<T>(parentObject);
        }

        private void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = ((CheckBox)sender).IsChecked ?? false;

            foreach (var employee in EmployeeUI)
            {
                employee.IsSelected = isChecked;
            }
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();

            var filteredEmployees = EmployeeList.Where(emp => emp.FirstName.ToLower().Contains(searchText))
                                         .Select(m => new { m.FirstName, m.ECN })
                                         .Distinct()
                                         .ToList();

            AddEmployeeByCustomOrder(filteredEmployees);
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Length > 0)
            {
                textBox.Text = char.ToUpper(textBox.Text[0]) + textBox.Text.Substring(1);

                textBox.CaretIndex = textBox.Text.Length;

                SelectAllEmployeeCheckBox.IsChecked = false;
            }
        }
    }
}
