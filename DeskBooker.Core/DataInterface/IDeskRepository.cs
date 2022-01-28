using DeskBooker.Core.Domain;
using System;
using System.Collections.Generic;

namespace DeskBooker.Core.DataInterface
{
    public interface IDeskRepository
    {
        IEnumerable<Desk> GetAvaliableDesks(DateTime date);
    }
}
