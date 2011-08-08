using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3Engines.D3D;

namespace S33M3Physics.Euler
{

    //1)a spring gives a force proportional to the relative displacement of its ends
    //2)a damper gives a force proportional to the relative velocity of its ends
    // Think of mass as providing a resistance to changes in motion.

    //This integrator will assume a cst Acceleration (That could be 0) ! Otherwhile it give bad precision !
    public class EulerSimulator_Vector3
    {
        Vector3 _velocity;
        Vector3 _acceleration;
        Vector3 _position;
        bool _isrunning;

        public bool Isrunning { get { return _isrunning; } set { _isrunning = value; } }
        public Vector3 Position { get { return _position; } set { _position = value; } }
        public Vector3 Velocity { get { return _velocity; } set { _velocity = value; } }
        public Vector3 Acceleration { get { return _acceleration; } set { _acceleration = value; } }

        public EulerSimulator_Vector3()
        {
            _isrunning = false;
        }

        public void StartSimulation(Vector3 startPosition, Vector3 startVelocity, Vector3 acceleration)
        {
            _position = startPosition;
            _acceleration = acceleration;
            _velocity = startVelocity;
            _isrunning = true;
        }

        public void StopSimulation()
        {
            _isrunning = false;
        }

        public void DoSimulation(ref GameTime dt, out Vector3 newPosition)
        {
            Vector3 newVelocity = _velocity + (_acceleration * dt.ElapsedGameTimeInS_LD);
            newPosition = _position + (_velocity * dt.ElapsedGameTimeInS_LD) + (0.5f * _acceleration * dt.ElapsedGameTimeInS_LD * dt.ElapsedGameTimeInS_LD);

            _position = newPosition;
            _velocity = newVelocity;
        }

        public void DoSimulation(ref float dt, out Vector3 newPosition)
        {
            Vector3 newVelocity = _velocity + (_acceleration * dt);
            newPosition = _position + (_velocity * dt) + (0.5f * _acceleration * dt * dt);

            _position = newPosition;
            _velocity = newVelocity;
        }
    }

    public class EulerSimulator_Float
    {
        float _velocity;
        float _acceleration;
        float _position;
        bool _isrunning;

        public bool Isrunning { get { return _isrunning; } set { _isrunning = value; } }
        public float Position { get { return _position; } set { _position = value; } }
        public float Velocity { get { return _velocity; } set { _velocity = value; } }
        public float Acceleration { get { return _acceleration; } set { _acceleration = value; } }

        public EulerSimulator_Float()
        {
            _isrunning = false;
        }

        public void StartSimulation(float startPosition, float startVelocity, float acceleration)
        {
            _position = startPosition;
            _acceleration = acceleration;
            _velocity = startVelocity;
            _isrunning = true;
        }

        public void StopSimulation()
        {
            _isrunning = false;
        }

        public void DoSimulation(ref GameTime dt, out float newPosition)
        {
            float newVelocity = _velocity + (_acceleration * dt.ElapsedGameTimeInS_LD);
            newPosition = _position + (_velocity * dt.ElapsedGameTimeInS_LD) + (0.5f * _acceleration * dt.ElapsedGameTimeInS_LD * dt.ElapsedGameTimeInS_LD);

            _position = newPosition;
            _velocity = newVelocity;
        }

        public void DoSimulation(ref float dt, out float newPosition)
        {
            float newVelocity = _velocity + (_acceleration * dt);
            newPosition = _position + (_velocity * dt) + (0.5f * _acceleration * dt * dt);

            _position = newPosition;
            _velocity = newVelocity;
        }
    }

}
