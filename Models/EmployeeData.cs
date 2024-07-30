using PdfDigitalSigner.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfDigitalSigner.Models
{
    public class EmployeeData : ObservableObject
    {
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        public string PanNo { get; set; }
        public string ActiveStatus { get; set; }
        public string ECN { get; set; }
        public string FirstName { get; set; }
        public string Designation { get; set; }
        public string EmailID { get; set; }
        public string StructureEffectiveDate { get; set; }
        public string LetterIssueDate { get; set; }
        public string LetterType { get; set; }

        public string BasicMonthly { get; set; }
        public string PFM { get; set; }
        public string StatutoryBonusM { get; set; }
        public string HRAM { get; set; }
        public string TelephoneM { get; set; }
        public string LTAM { get; set; }
        public string ChildEducationM { get; set; }
        public string CarBikeM { get; set; }
        public string MealM { get; set; }
        public string SpecialM { get; set; }
        public string Monthly { get; set; }
        public string BasicY { get; set; }
        public string YearlyPF { get; set; }
        public string StatutoryBonusY { get; set; }
        public string HRAY { get; set; }
        public string TelephoneY { get; set; }
        public string MobileHandsetY { get; set; }
        public string LTAY { get; set; }
        public string ChildEducationY { get; set; }
        public string ProfessionalDevelopmentY { get; set; }
        public string CarBikeY { get; set; }
        public string MealY { get; set; }
        public string GiftY { get; set; }
        public string SpecialY { get; set; }
        public string Subtotal { get; set; }
        public string Bonus { get; set; }
        public string Gratuity { get; set; }
        public string Total { get; set; }
        public string AmountDesc { get; set; }
        public string ActualNewCTC { get; set; }

    }
}
