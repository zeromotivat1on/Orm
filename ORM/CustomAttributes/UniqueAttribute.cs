using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.CustomAttributes
{
    [System.AttributeUsage(
        System.AttributeTargets.Property,
        AllowMultiple = false)
    ]
    public class UniqueAttribute : System.Attribute
    {

    }
}
