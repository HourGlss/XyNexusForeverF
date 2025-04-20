using System.Numerics;
using NexusForever.Game.Abstract.Entity.Movement.Force;

namespace NexusForever.Game.Entity.Movement.Force
{
    public class ProjectileKeyGenerator : IProjectileKeyGenerator
    {
        /// <summary>
        /// Generate key data for a projectile with the supplied parameters.
        /// </summary>
        public void Calculate(TimeSpan flightTime, float gravity, Vector3 start, Vector3 end, out List<Vector3> positions, out List<Vector3> velocities, out List<uint> times)
        {
            positions  = [];
            velocities = [];
            times      = [];

            var gravityVector = new Vector3(0, -gravity, 0);

            Vector3 initialVelocity = (end - start) / (float)flightTime.TotalSeconds;
            initialVelocity -= 0.5f * (float)flightTime.TotalSeconds * gravityVector;

            TimeSpan stepSize = TimeSpan.FromMilliseconds(100);
            for (uint i = 0; i <= (uint)Math.Ceiling(flightTime / stepSize); i++)
            {
                TimeSpan currentTime = stepSize * i;
                if (currentTime > flightTime)
                    currentTime = flightTime;

                times.Add((uint)currentTime.TotalMilliseconds);

                float currentTimeSeconds = (float)currentTime.TotalSeconds;

                Vector3 velocityDisplacement = initialVelocity * currentTimeSeconds;
                Vector3 gravityDisplacement  = 0.5f * gravityVector * currentTimeSeconds * currentTimeSeconds;
                Vector3 position = start + velocityDisplacement + gravityDisplacement;
                positions.Add(position);

                Vector3 velocity = initialVelocity + gravityVector * currentTimeSeconds;
                velocities.Add(velocity);
            }
        }
    }
}
