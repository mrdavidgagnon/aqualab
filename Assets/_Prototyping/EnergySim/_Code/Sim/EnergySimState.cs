using System;
using BeauData;
using BeauUtil;
using UnityEngine;

namespace ProtoAqua.Energy
{
    public sealed class EnergySimState : IEnergySimStateReader
    {
        public const int MaxActors = 256;

        public EnvironmentState Environment;
        
        public readonly ActorState[] Actors = new ActorState[MaxActors];
        public ushort ActorCount;

        public ushort[] Populations;
        public uint[] Masses;
        
        public ushort Timestamp;
        public uint NextSeed;
        public ushort NextActorId;

        public void Reset(in EnergySimContext inContext)
        {
            Environment = default(EnvironmentState);
            Array.Clear(Actors, 0, Actors.Length);
            ActorCount = 0;
            Timestamp = 0;
            NextSeed = 0;
            NextActorId = 0;

            Configure(inContext);
        }

        public void CopyFrom(in EnergySimState inState)
        {
            Environment = inState.Environment;

            Array.Copy(inState.Actors, Actors, MaxActors);
            ActorCount = inState.ActorCount;

            if (inState.Populations != null)
            {
                Array.Resize(ref Populations, inState.Populations.Length);
                Array.Copy(inState.Populations, Populations, Populations.Length);
            }

            if (inState.Masses != null)
            {
                Array.Resize(ref Masses, inState.Masses.Length);
                Array.Copy(inState.Masses, Masses, Masses.Length);
            }
            
            Timestamp = inState.Timestamp;
            NextSeed = inState.NextSeed;
            NextActorId = inState.NextActorId;
        }

        public void Configure(in EnergySimContext inContext)
        {
            FourCC[] actorTypeIds = inContext.Database.ActorTypeIds();
            
            Array.Resize(ref Populations, actorTypeIds.Length);
            Array.Resize(ref Masses, actorTypeIds.Length);
            for(int i = 0; i < Populations.Length; ++i)
            {
                Populations[i] = 0;
                Masses[i] = 0;
            }
        }

        public ref ActorState AddActor(ActorType inType)
        {
            if (ActorCount >= MaxActors)
            {
                throw new Exception("Already at max actor count " + MaxActors);
            }

            ref ActorState state = ref Actors[ActorCount++];
            state.Id = NextActorId++;
            inType.CreateActor(ref state);
            return ref state;
        }

        public void AddActors(ActorType inType, int inCount)
        {
            if (ActorCount >= MaxActors)
            {
                throw new Exception("Already at max actor count " + MaxActors);
            }
            
            while(--inCount >= 0)
            {
                ref ActorState state = ref Actors[ActorCount++];
                state.Id = NextActorId++;
                inType.CreateActor(ref state);
            }
        }

        public void DeleteActor(ushort inActorIndex)
        {
            ref ActorState actor = ref Actors[inActorIndex];
            actor = default(ActorState);

            if (inActorIndex < ActorCount - 1)
            {
                Ref.Swap(ref Actors[inActorIndex], ref Actors[ActorCount - 1]);
            }
            --ActorCount;
        }
    
        public void AddResourceToEnvironment(EnergySimDatabase inDatabase, FourCC inResourceType, ushort inCount)
        {
            int idx = inDatabase.ResourceVarToIndex(inResourceType);
            Environment.OwnedResources[idx] += inCount;
        }

        public void SetPropertyInEnvironment(EnergySimDatabase inDatabase, FourCC inPropertyType, float inValue)
        {
            int idx = inDatabase.PropertyVarToIndex(inPropertyType);
            Environment.Properties[idx] = inValue;
        }

        #region IEnergySimStateReader

        ushort IEnergySimStateReader.GetEnvironmentResource(FourCC inResourceId, EnergySimDatabase inDatabase)
        {
            int idx = inDatabase.ResourceVarToIndex(inResourceId);
            return Environment.OwnedResources[idx];
        }

        float IEnergySimStateReader.GetEnvironmentProperty(FourCC inPropertyId, EnergySimDatabase inDatabase)
        {
            int idx = inDatabase.PropertyVarToIndex(inPropertyId);
            return Environment.Properties[idx];
        }

        ushort IEnergySimStateReader.GetActorCount(FourCC inActorId, EnergySimDatabase inDatabase)
        {
            int idx = inDatabase.ActorTypeToIndex(inActorId);
            return Populations[idx];
        }

        uint IEnergySimStateReader.GetActorMass(FourCC inActorId, EnergySimDatabase inDatabase)
        {
            int idx = inDatabase.ActorTypeToIndex(inActorId);
            return Masses[idx];
        }

        FourCC IEnergySimStateReader.GetEnvironmentType()
        {
            return Environment.Type;
        }

        ushort IEnergySimStateReader.GetTickId()
        {
            return Timestamp;
        }

        #endregion // IEnergySimStateReader
    }
}