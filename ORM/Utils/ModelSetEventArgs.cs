using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.Utils
{
    public class ModelSetEventArgs
    {
        public string Message { get; }
        public object Model { get; }
        public int ModelId { get; }

        public ModelSetEventArgs(string message)
        {
            this.Message = message;
        }

        public ModelSetEventArgs(string message, object model)
        {
            this.Message = message;
            this.Model = model;
        }

        public ModelSetEventArgs(string message, int id)
        {
            this.Message = message;
            this.ModelId = id;
        }
    }
}
