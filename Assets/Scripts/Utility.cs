using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Thovex
{
    public static class Utility
    {

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

        public static Vector3Int NegateVector3Int(Vector3Int input)
        {
            return new Vector3Int(-input.x, -input.y, -input.z);
        }
    }
}