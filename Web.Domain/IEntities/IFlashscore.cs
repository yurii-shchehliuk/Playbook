using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Domain.IEntities
{
    public interface IFlashscore
    {
       List<string> Summary { get; set; }
       List<string> Stats0 { get; set; }
       List<string> Stats1 { get; set; }
       List<string> Stats2 { get; set; }
    }
}
