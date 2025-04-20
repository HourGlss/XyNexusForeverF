using System.Numerics;
using NexusForever.Game.Abstract.Entity.Movement.Force;

namespace NexusForever.Game.Entity.Movement.Force
{
    public class RotationKeyGenerator : IRotationKeyGenerator
    {
        /// <summary>
        /// Generate key data for rotation with the supplied parameters.
        /// </summary>
        public void Calculate(Vector3 startRotation, TimeSpan duration, float spin, out List<Vector3> rotations, out List<uint> times)
        {
            rotations = [];
            times     = [];

            TimeSpan stepSize = TimeSpan.FromMilliseconds(100);
            for (uint i = 0; i <= (uint)Math.Ceiling(duration / stepSize); i++)
            {
                TimeSpan currentTime = stepSize * i;
                if (currentTime > duration)
                    currentTime = duration;

                times.Add((uint)currentTime.TotalMilliseconds);

                float currentTimeSeconds = (float)(currentTime - duration).TotalSeconds;

                float angle = (currentTimeSeconds * spin + startRotation.X + MathF.PI) * (1f / (2f * MathF.PI));
                angle -= MathF.Floor(angle);
                float normalisedAngle = angle * (2f * MathF.PI) - MathF.PI;

                var rotation = new Vector3(normalisedAngle, startRotation.Y, startRotation.Z);
                rotations.Add(rotation);
            }
        }
    }
}
