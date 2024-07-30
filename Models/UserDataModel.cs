using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfDigitalSigner.Models
{
    public class UserDataModel
    {
        public string LetterType { get; set; }

        public List<EmployeeData> SelectedEmployee { get; set; }

        public string OutputLocation { get; set; }
    }
}
