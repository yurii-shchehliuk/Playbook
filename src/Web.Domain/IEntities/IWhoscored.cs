using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Domain.Entities;

namespace Web.Domain.IEntities
{
    public interface IWhoscored
    {
        PositionalReport AttackSides { get; set; }
        PositionalReport ShotDirections { get; set; }
        PositionalReport ShotZones { get; set; }
        PositionalReport ActionZones { get; set; }
    }
}
