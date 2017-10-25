using System.Collections.Generic;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Address
{
    public interface AddressBase  
    {
        void LoadPage();
        object CreateAddressFromControl(string typeName);
        object CreateAddressFromControl();
        bool IsEditable();
        bool Validate();
        void displayValidationMessages(List<string> errors);
        bool IsOkToSave();

        string XMLFile { get; set; }
        List<string> ErrorList { get; set; }
        object DataContext { get; set; }
    }

    
}