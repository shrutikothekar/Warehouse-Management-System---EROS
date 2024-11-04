using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static eros.Controllers.Loading_Dispatch_OperationController;

namespace eros.Models
{
    
    public class Dispatch_New
    {

        public Loading_Dispatch_Operation Loading_Dispatch_Operation { get; set; }
        public List<Loading_Dispatch_Operation> Loading_Dispatch_Operation1 { get; set; }
        public List<Courier_Transport> TableData { get; set; }

    }

}
