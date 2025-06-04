using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xhunter74.CollectionManager.Data.Entity;

public enum FieldTypes
{
    Text = 0,
    Number = 1,
    DateTime = 2,
    Boolean = 3,
    Select = 4,
    MultiSelect = 5,
    File = 6,
    Image = 7,
    Link = 8,
    RichText = 9,
    ColorPicker = 10,
    Rating = 11,
    Tags = 12,
    CustomObject = 13,
    Location = 14,
    Currency = 15,
}
