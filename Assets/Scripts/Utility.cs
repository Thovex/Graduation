using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Thovex
{
    public static class Utility
    {

        public static T2 TryGet<T1, T2>(Dictionary<T1, T2> dictionary, T1 key, T2 defaultValue)
        {
            if (dictionary.ContainsKey(key)) {
                dictionary.TryGetValue(key, out T2 value);
                return value;
            }

            return defaultValue;
        }

        public static void AddOrUpdate<T1, T2>(Dictionary<T1, T2> dictionary, T1 key, T2 value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            } else
            {
                dictionary.Add(key, value);
            }
        }

        public static void For3b(Vector3Int data, bool breakout, Action<int, int, int, bool> iterator)
        {
            for (int x = 0; x < data.x; x++)
            {
                if (breakout) goto end;
                for (int y = 0; y < data.y; y++)
                {
                    if (breakout) goto end;
                    for (int z = 0; z < data.z; z++)
                    {
                        if (breakout) goto end;
                        iterator(x, y, z, breakout);
                    }
                }
            }

        end:
            return;
        }

        public static void For3b<T>(Matrix3<T> data, bool breakout, Action<int, int, int, bool> iterator)
        {
            if (data != null)
            {
                Vector3Int loopSize = new Vector3Int(data.SizeX, data.SizeY, data.SizeZ);
                For3b(loopSize, breakout, iterator);
            }
        }

        public static void For3(Vector3Int data, Action<int, int, int> iterator)
        {
            for (int x = 0; x < data.x; x++)
            {
                for (int y = 0; y < data.y; y++)
                {
                    for (int z = 0; z < data.z; z++)
                    {
                        iterator(x, y, z);
                    }
                }
            }
        }

        public static void For3(Vector3Int data, int iteratorJumpSize, Action<int, int, int> iterator)
        {
            for (int x = 0; x < data.x; x += iteratorJumpSize)
            {
                for (int y = 0; y < data.y; y += iteratorJumpSize)
                {
                    for (int z = 0; z < data.z; z += iteratorJumpSize)
                    {
                        iterator(x, y, z);
                    }
                }
            }
        }

        public static void For3<T>(Matrix3<T> data, Action<int, int, int> iterator)
        {
            if (data != null)
            {
                Vector3Int loopSize = new Vector3Int(data.SizeX, data.SizeY, data.SizeZ);
                For3(loopSize, iterator);
            }
        }

        public static void For3<T>(Matrix3<T> data, int iteratorJumpSize, Action<int, int, int> iterator)
        {
            if (data != null)
            {
                Vector3Int loopSize = new Vector3Int(data.SizeX, data.SizeY, data.SizeZ);
                For3(loopSize, iteratorJumpSize, iterator);
            }
        }

        public static Vector3Int Negate(Vector3Int input)
        {
            return input * -1;
        }

        public static Vector3Int V3ToV3I(Vector3 input)
        {
            return new Vector3Int(Mathf.RoundToInt(input.x), Mathf.RoundToInt(input.y), Mathf.RoundToInt(input.z));
        }

        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }

        public static float SetScale(float currentValue, float oldMinScale, float oldMaxScale, float newMinScale, float newMaxScale)
        {

            return (((currentValue - oldMinScale) * (newMaxScale - newMinScale)) / (oldMaxScale - oldMinScale)) + newMinScale;
        }
    }
}