using BattleshipStateTracker.Request;
using BattleshipStateTracker.Response;
using BattleshipStateTracker.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleshipStateTracker.Logic
{
    class Board
    {
        public const int xCoordinator = 10;
        public const int yCoordinator = 10;
        private Dictionary<Coordinate, ShotHistory> ShotHistory;
        private int _currentShipIndex;

        public Ship[] Ships { get; private set; }

        public Board()
        {
            ShotHistory = new Dictionary<Coordinate, ShotHistory>();
            Ships = new Ship[5];
            _currentShipIndex = 0;
        }

        public ShotResponse FireShot(Coordinate coordinate)
        {
            var responses = new ShotResponse();

            // is this coordinate on the board?
            if (!IsValidCoordinate(coordinate))
            {
                responses.ShotStatus = ShotStatus.Invalid;
                return responses;
            }

            // did they already try this position?
            if (ShotHistory.ContainsKey(coordinate))
            {
                responses.ShotStatus = ShotStatus.Duplicate;
                return responses;
            }

            CheckShipsForHit(coordinate, responses);
            CheckForVictory(responses);

            return responses;
        }

        public ShotHistory CheckCoordinate(Coordinate coordinate)
        {
            if (ShotHistory.ContainsKey(coordinate))
            {
                return ShotHistory[coordinate];
            }
            else
            {
                return Response.ShotHistory.Unknown;
            }
        }

        public ShotPlace PlaceShip(ShipRequest request)
        {
            if (_currentShipIndex > 4)
                throw new Exception("You can not add another ship, 5 is the limit!");

            if (!IsValidCoordinate(request.Coordinate))
                return ShotPlace.NotEnoughSpace;

            Ship newShip = ShipMaker.CreateShip(request.ShipType);
            switch (request.Direction)
            {
                case ShipDirection.Down:
                    return PlaceShipDown(request.Coordinate, newShip);
                case ShipDirection.Up:
                    return PlaceShipUp(request.Coordinate, newShip);
                case ShipDirection.Left:
                    return PlaceShipLeft(request.Coordinate, newShip);
                default:
                    return PlaceShipRight(request.Coordinate, newShip);
            }
        }

        private void CheckForVictory(ShotResponse response)
        {
            if (response.ShotStatus == ShotStatus.HitAndSunk)
            {
                // did they win?
                if (Ships.All(s => s.IsSunk))
                    response.ShotStatus = ShotStatus.Victory;
            }
        }

        private void CheckShipsForHit(Coordinate coordinate, ShotResponse response)
        {
            response.ShotStatus = ShotStatus.Miss;

            foreach (var ship in Ships)
            {
                // no need to check sunk ships
                if (ship.IsSunk)
                    continue;

                ShotStatus status = ship.FireAtShip(coordinate);

                switch (status)
                {
                    case ShotStatus.HitAndSunk:
                        response.ShotStatus = ShotStatus.HitAndSunk;
                
                        ShotHistory.Add(coordinate, Response.ShotHistory.Hit);
                        break;
                    case ShotStatus.Hit:
                        response.ShotStatus = ShotStatus.Hit;
                      
                        ShotHistory.Add(coordinate, Response.ShotHistory.Hit);
                        break;
                }

                // if they hit something, no need to continue looping
                if (status != ShotStatus.Miss)
                    break;
            }

            if (response.ShotStatus == ShotStatus.Miss)
            {
                ShotHistory.Add(coordinate, Response.ShotHistory.Miss);
            }
        }

        private bool IsValidCoordinate(Coordinate coordinate)
        {
            return coordinate.XCoordinate >= 1 && coordinate.XCoordinate <= xCoordinator &&
            coordinate.YCoordinate >= 1 && coordinate.YCoordinate <= yCoordinator;
        }

        private ShotPlace PlaceShipRight(Coordinate coordinate, Ship newShip)
        {
            // y coordinate gets bigger
            int positionIndex = 0;
            int maxY = coordinate.YCoordinate + newShip.BoardPositions.Length;

            for (int i = coordinate.YCoordinate; i < maxY; i++)
            {
                var currentCoordinate = new Coordinate(coordinate.XCoordinate, i);
                if (!IsValidCoordinate(currentCoordinate))
                    return ShotPlace.NotEnoughSpace;

                if (OverlapsAnotherShip(currentCoordinate))
                    return ShotPlace.Overlap;

                newShip.BoardPositions[positionIndex] = currentCoordinate;
                positionIndex++;
            }

            AddShipToBoard(newShip);
            return ShotPlace.Ok;
        }

        private ShotPlace PlaceShipLeft(Coordinate coordinate, Ship newShip)
        {
            // y coordinate gets smaller
            int positionIndex = 0;
            int minY = coordinate.YCoordinate - newShip.BoardPositions.Length;

            for (int i = coordinate.YCoordinate; i > minY; i--)
            {
                var currentCoordinate = new Coordinate(coordinate.XCoordinate, i);

                if (!IsValidCoordinate(currentCoordinate))
                    return ShotPlace.NotEnoughSpace;

                if (OverlapsAnotherShip(currentCoordinate))
                    return ShotPlace.Overlap;

                newShip.BoardPositions[positionIndex] = currentCoordinate;
                positionIndex++;
            }

            AddShipToBoard(newShip);
            return ShotPlace.Ok;
        }

        private ShotPlace PlaceShipUp(Coordinate coordinate, Ship newShip)
        {
            // x coordinate gets smaller
            int positionIndex = 0;
            int minX = coordinate.XCoordinate - newShip.BoardPositions.Length;

            for (int i = coordinate.XCoordinate; i > minX; i--)
            {
                var currentCoordinate = new Coordinate(i, coordinate.YCoordinate);

                if (!IsValidCoordinate(currentCoordinate))
                    return ShotPlace.NotEnoughSpace;

                if (OverlapsAnotherShip(currentCoordinate))
                    return ShotPlace.Overlap;

                newShip.BoardPositions[positionIndex] = currentCoordinate;
                positionIndex++;
            }

            AddShipToBoard(newShip);
            return ShotPlace.Ok;
        }

        private ShotPlace PlaceShipDown(Coordinate coordinate, Ship newShip)
        {
            // y coordinate gets bigger
            int positionIndex = 0;
            int maxX = coordinate.XCoordinate + newShip.BoardPositions.Length;

            for (int i = coordinate.XCoordinate; i < maxX; i++)
            {
                var currentCoordinate = new Coordinate(i, coordinate.YCoordinate);

                if (!IsValidCoordinate(currentCoordinate))
                    return ShotPlace.NotEnoughSpace;

                if (OverlapsAnotherShip(currentCoordinate))
                    return ShotPlace.Overlap;

                newShip.BoardPositions[positionIndex] = currentCoordinate;
                positionIndex++;
            }

            AddShipToBoard(newShip);
            return ShotPlace.Ok;
        }

        private void AddShipToBoard(Ship newShip)
        {
            Ships[_currentShipIndex] = newShip;
            _currentShipIndex++;
        }

        private bool OverlapsAnotherShip(Coordinate coordinate)
        {
            foreach (var ship in Ships)
            {
                if (ship != null)
                {
                    if (ship.BoardPositions.Contains(coordinate))
                        return true;
                }
            }

            return false;
        }
    }
}
