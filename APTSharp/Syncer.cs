﻿using System;
using System.Collections.Generic;
using System.Text;
using static APTSharp.Utils;

namespace APTSharp
{
    public struct SyncerMatch
    {
        public enum SyncerMatchPattern
        {
            NO_MATCH,
            SYNC_A_MATCH,
            SYNC_B_MATCH
        };
        public SyncerMatchPattern MatchPattern { get; private set; }

        public long Offset { get; private set; }

        public float Fidelity { get; private set; }

        public SyncerMatch(SyncerMatchPattern matchPattern, long offset, float fidelity)
        {
            this.MatchPattern = matchPattern;
            this.Offset = offset;
            this.Fidelity = fidelity;
        }
    }

    public class Syncer
    {
        /// <summary>
        /// The sync A pattern defined by Automatic Picture Transmission format
        /// </summary>
        public const string SYNC_A = "0000110011001100110011001100110000000000";

        /// <summary>
        /// The sync B pattern defined by Automatic Picture Transmission format
        /// </summary>
        public const string SYNC_B = "0000111001110011100111001110011100111000";

        /// <summary>
        /// The minimum fidelity in % for a sync pattern to be considered valid
        /// </summary>
        public const float MIN_FIDELITY = 0.875f;

        public float averageLevel = 0.5f;

        public float[] StateStack = new float[40];

        private float[] Samples;

        private long Pos = 0;

        public delegate void OnSampleDelegate(float sample);
        public event OnSampleDelegate OnSample = null;

        public delegate void OnSyncADelegate(float sample);
        public event OnSyncADelegate OnSyncA = null;

        public delegate void OnSyncBDelegate(float sample);
        public event OnSyncBDelegate OnSyncB = null;

        public Syncer(float[] samples)
        {
            this.Samples = samples;
            for (int i = 0; i < SYNC_A.Length; i++)
            {
                StateStack[i] = samples[i];
                averageLevel = 0.25f * samples[i] + averageLevel * 0.75f;
            }
        }

        public void Synchronize()
        {
            float fidelity = 0.0f;
            float currentSample = 0.0f;

            for (long i = 0; i < Samples.Length; i++)
            {
                bool fidA = MatchSyncA(Samples, i) > MIN_FIDELITY;
                bool fidB = MatchSyncB(Samples, i) > MIN_FIDELITY;

                currentSample = StateStack[Pos];    
                StateStack[Pos] = Samples[i];
                averageLevel = 0.25f * Samples[i] + averageLevel * 0.75f;
                Pos = (Pos + 1) % SYNC_A.Length;
                if (fidA) {
                    Console.WriteLine("Sync A Fid:{0}", fidA);
                    OnSyncA?.Invoke(currentSample);
                } else if (fidB) {
                    Console.WriteLine("Sync B Fid:{0}", fidA);
                    OnSyncB?.Invoke(currentSample);
                } else {
                    OnSample?.Invoke(currentSample);
                }
            }
        }

        private float MatchSyncA(float[] samples, long offset)
        {
            bool currentValue = false;
            int fidCounter = 0;
            
            if (samples.Length - offset < SYNC_A.Length)
                return (0);
            for (int i = 0; i < SYNC_A.Length; i++)
            {
                long pos = (Pos + i) % SYNC_A.Length;

                currentValue = (SYNC_A[i] == '1');
                if (Utils.AnalogValueToBool(StateStack[pos] / (averageLevel * 2)) == currentValue)
                    fidCounter++;
            }
            return (fidCounter / (float)SYNC_A.Length);
        }

        private float MatchSyncB(float[] samples, long offset)
        {
            bool currentValue = false;
            int fidCounter = 0;

            if (samples.Length - offset < SYNC_B.Length)
                return (0);
            for (int i = 0; i < SYNC_B.Length; i++)
            {
                long pos = (Pos + i) % SYNC_B.Length;

                currentValue = (SYNC_B[i] == '1');
                if (Utils.AnalogValueToBool(StateStack[pos] / (averageLevel * 2)) == currentValue)
                    fidCounter++;
            }
            return (fidCounter / (float)SYNC_B.Length);
        }
    }
}