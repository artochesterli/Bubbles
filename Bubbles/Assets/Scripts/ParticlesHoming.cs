using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class ParticlesHoming : MonoBehaviour
{


    public Vector3 Target;
    public float Force;

    private ParticleSystem _ps;
    private ParticleSystem.MainModule _psMain;
    private ParticleSystem.Particle[] _particles;

    private void LateUpdate()
    {
        

        _ps.GetParticles(_particles);

        var originalTargetPos = Target + transform.position;


        Vector3 resultTargetPos = new Vector3();

        switch (_psMain.simulationSpace)
        {
            case ParticleSystemSimulationSpace.Local:
            {
                resultTargetPos = transform.InverseTransformPoint(originalTargetPos);
                break;
            }
            case ParticleSystemSimulationSpace.Custom:
            {
                resultTargetPos = _psMain.customSimulationSpace.InverseTransformPoint(originalTargetPos);
                break;
            }
            case ParticleSystemSimulationSpace.World:
            {
                resultTargetPos = originalTargetPos;
                break;
            }
        }

        int particleCount = _ps.particleCount;

        for (int i = 0; i < particleCount; i++)
        {
            if (Vector3.Distance(resultTargetPos, _particles[i].position)<0.35f)
            {
                _particles[i].velocity = Vector3.zero;
            }
            else if (Vector3.Distance(resultTargetPos, _particles[i].position)<0.7f)
            {
                var dir = Vector3.Normalize(resultTargetPos - _particles[i].position);
                var force = dir * Force * 2f;
                _particles[i].velocity += force;
            }
            else
            {
                var dir = Vector3.Normalize(resultTargetPos - _particles[i].position);
                var force = dir * Force;
                _particles[i].velocity += force;
            }
            
        }

        _ps.SetParticles(_particles, particleCount);
    }

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
        _psMain = _ps.main;
        _particles = new ParticleSystem.Particle[_psMain.maxParticles];
    }

}
