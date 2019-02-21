using BattleshipStateTracker.Ships;
using System;
using System.Collections.Generic;
using System.Text;

namespace BattleshipStateTracker.Logic
{
    class ShipMaker
    {
        public static Ship CreateShip(ShipType type)
        {
            switch (type)
            {
                case ShipType.Destroyer:
                    return new Ship(ShipType.Destroyer, 2);
                case ShipType.Cruiser:
                    return new Ship(ShipType.Cruiser, 3);
                case ShipType.Submarine:
                    return new Ship(ShipType.Submarine, 3);
                case ShipType.Battleship:
                    return new Ship(ShipType.Battleship, 4);
                default:
                    return new Ship(ShipType.Carrier, 5);
            }
        }
    }
}
