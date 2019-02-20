using BattleshipStateTracker.Ships;
using System;
using System.Collections.Generic;
using System.Text;

namespace BattleshipStateTracker.Request
{
  public class ShipRequest
    {
        public Coordinate Coordinate { get; set; }
        public ShipDirection Direction { get; set; }
        public ShipType ShipType { get; set; }
    }
}
