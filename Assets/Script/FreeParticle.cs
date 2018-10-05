using UnityEngine;
using System.Collections;

public class FreeParticle : MonoBehaviour
{
    private struct Particle
    {
        public Vector3 position;
        public Vector3 velocity;
    }

    private const int SIZE_PARTICLE = 24;

    public int particleCount = 1000;

    public Material material;

    public ComputeShader computeShader;

    private int mComputeShaderKernelID;

    ComputeBuffer particleBuffer;

    private const int WARP_SIZE = 256;

    private int mWarpCount;

    [SerializeField]
    public float m_MaxAge ;

    public float MaxAge 
    {
        get { return m_MaxAge;}
        set { m_MaxAge = value;}
    }

    private float lastSwitchTime;
    
    [SerializeField]
    public float switchDuration = 30f;

    [SerializeField]
    public bool m_autoSwitch ;

    [SerializeField]
    public Vector4 m_randomRange = Vector4.one;

    public bool autoSwitch 
    {
        get { return m_autoSwitch;}
        set { m_autoSwitch = value;}
    }
    public Vector4 Parameters 
    {
        get { return m_parameters; }
        set { m_parameters = value; }
    }

    [SerializeField]
    private Vector4 m_parameters;

    public Vector4 WeightsA 
    {
        get { return m_weightsA; }
        set { m_weightsA = value; }
    }
    public Vector4 WeightsB 
    {
        get { return m_weightsB; }
        set { m_weightsB = value; }
    }
    public Vector4 WeightsC 
    {
        get { return m_weightsC; }
        set { m_weightsC = value; }
    }
    public Vector4 WeightsD 
    {
        get { return m_weightsD; }
        set { m_weightsD = value; }
    }

    public Vector4 WeightModulationA 
    {
        get { return m_weightModulationA; }
        set { m_weightModulationA = value; }
    }
    public Vector4 WeightModulationB 
    {
        get { return m_weightModulationB; }
        set { m_weightModulationB = value; }
    }
    public Vector4 WeightModulationC 
    {
        get { return m_weightModulationC; }
        set { m_weightModulationC = value; }
    }
    public Vector4 WeightModulationD 
    {
        get { return m_weightModulationD; }
        set { m_weightModulationD = value; }
    }
 

    public float OffsetX 
    {
        set { Parameters = new Vector4(value, Parameters.y, Parameters.z, Parameters.w); }
    }

   public float OffsetY 
    {
        set { Parameters = new Vector4(Parameters.x, value, Parameters.z, Parameters.w); }
    }

   public float OffsetZ
    {
        set { Parameters = new Vector4(Parameters.x, Parameters.y, value, Parameters.w); }
    }


    public Vector4 m_weightsA;
    public Vector4 m_weightsB;
    public Vector4 m_weightsC;
    public Vector4 m_weightsD;

    public Vector4 m_weightModulationA;
    public Vector4 m_weightModulationB;
    public Vector4 m_weightModulationC;
    public Vector4 m_weightModulationD;


    public float TransitionSpeed = 0.99f;

    private Vector4 NextWeightsA;
    private Vector4 NextWeightsB;
    private Vector4 NextWeightsC;
    private Vector4 NextWeightsD;

    public Vector4 NextParameters ;

    private Vector4 NextModA;
    private Vector4 NextModB;
    private Vector4 NextModC;
    private Vector4 NextModD;

    private bool isTransition = false;

    public Vector3 Emitter {get; set;}

    Texture2D noiseTexture;

    public void Reset()
    {
        Start();
    }

    void Start()
    {
        if (particleBuffer != null)
            particleBuffer.Release();

        // Calculate the number of warp needed to handle all the particles
        if (particleCount <= 0)
            particleCount = 1;

        mWarpCount = Mathf.CeilToInt((float)particleCount / WARP_SIZE);

        // Initialize the Particle at the start
        Particle[] particleArray = new Particle[particleCount];
        for (int i = 0; i < particleCount; ++i)
        {
            particleArray[i].position = Vector3.one;
            particleArray[i].velocity = Vector3.one * float.MaxValue;
            particleArray[i].velocity.x = Random.value * MaxAge;
        }

        noiseTexture = new Texture2D(
            4096,
            Mathf.CeilToInt(particleCount / 4096),
            TextureFormat.RGBAFloat, false, false );

        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                noiseTexture.SetPixel(x, y, new Vector4(Random.value, Random.value, Random.value, Random.value));
            }
        }

        noiseTexture.Apply();

        // Create the ComputeBuffer holding the Particles
        particleBuffer = new ComputeBuffer(particleCount, SIZE_PARTICLE);
        particleBuffer.SetData(particleArray);

        // Find the id of the kernel
        mComputeShaderKernelID = computeShader.FindKernel("CSMain");

        // Bind the ComputeBuffer to the shader and the compute shader
        computeShader.SetBuffer(mComputeShaderKernelID, "particleBuffer", particleBuffer);
        material.SetBuffer("particleBuffer", particleBuffer);
        computeShader.SetTexture(mComputeShaderKernelID, "NoiseTex", noiseTexture);
    }

    void OnDestroy()
    {
        if (particleBuffer != null)
            particleBuffer.Release();
    }

    public Vector4 RandomVector()
    {
        return new Vector4(Random.value,Random.value,Random.value,Random.value);
    }

    public void Randomize()
    {

        NextWeightsA = Vector4.Scale(RandomVector(), m_randomRange);
        NextWeightsB = Vector4.Scale(RandomVector(), m_randomRange);
        NextWeightsC = Vector4.Scale(RandomVector(), m_randomRange);
        NextWeightsD = Vector4.Scale(RandomVector(), m_randomRange);
        NextModA = Vector4.Scale(RandomVector(), m_randomRange);
        NextModB = Vector4.Scale(RandomVector(), m_randomRange);
        NextModC = Vector4.Scale(RandomVector(), m_randomRange);
        NextModD = Vector4.Scale(RandomVector(), m_randomRange);

        NextParameters = Vector4.Scale(RandomVector(), m_randomRange);
        isTransition = true;
    }

    void Update()
    {
        computeShader.SetFloat("Time", Time.time);
        computeShader.SetFloat("NumParticles", particleCount);
        computeShader.SetFloat("MaxAge", MaxAge);

        computeShader.SetVector("WeightsA", WeightModulationA + WeightsA);
        computeShader.SetVector("WeightsB", WeightModulationB + WeightsB);
        computeShader.SetVector("WeightsC", WeightModulationC + WeightsC);
        computeShader.SetVector("WeightsD", WeightModulationD + WeightsD);
        computeShader.SetVector("Parameters", m_parameters);
        computeShader.SetVector("Emitter", Emitter);

        computeShader.Dispatch(mComputeShaderKernelID, mWarpCount, 1, 1);

        if (isTransition)
        {
            WeightsA = Vector4.Lerp(NextWeightsA, WeightsA, TransitionSpeed);
            WeightsB = Vector4.Lerp(NextWeightsB, WeightsB, TransitionSpeed);
            WeightsC = Vector4.Lerp(NextWeightsC, WeightsC, TransitionSpeed);
            WeightsD = Vector4.Lerp(NextWeightsD, WeightsD, TransitionSpeed);

            WeightModulationA = Vector4.Lerp(NextModA, WeightModulationA, TransitionSpeed);
            WeightModulationB = Vector4.Lerp(NextModB, WeightModulationB, TransitionSpeed);
            WeightModulationC = Vector4.Lerp(NextModC, WeightModulationC, TransitionSpeed);
            WeightModulationD = Vector4.Lerp(NextModD, WeightModulationD, TransitionSpeed);

            //m_parameters = Vector4.Lerp(NextParameters, m_parameters, TransitionSpeed);

            if ( (WeightsA - NextWeightsA).sqrMagnitude < 0.0001f)
                isTransition = false;
        }

        if ( autoSwitch && Time.time - lastSwitchTime > switchDuration)
        {
            Randomize();
            lastSwitchTime = Time.time;
        }

    }

    void OnRenderObject()
    {
        material.SetMatrix("modelToWorld", transform.localToWorldMatrix);
        material.SetPass(0);

        Graphics.DrawProcedural(MeshTopology.Points, 1, particleCount);
    }

}
