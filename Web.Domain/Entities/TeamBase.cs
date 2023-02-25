using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Domain.Entities
{
    public class TeamBase : BaseEntity
    {
        public string Name { get; set; }

        public TeamBase GetInstance()
        {
            return new TeamBase { Id = this.Id, Name = this.Name };
        }
    }
}
