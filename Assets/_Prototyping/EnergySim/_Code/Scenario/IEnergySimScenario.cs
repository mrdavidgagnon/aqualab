using System;
using BeauData;
using BeauUtil;
using UnityEngine;

namespace ProtoAqua.Energy
{
    public interface IEnergySimScenario : IUpdateVersioned
    {
        ushort TotalTicks();
        int TickActionCount();
        int TickScale();

        void Initialize(EnergySimState ioState, ISimDatabase inDatabase);
        bool TryCalculateProperty(FourCC inPropertyId, IEnergySimStateReader inReader, ISimDatabase inDatabase, out float outValue);
    }
}