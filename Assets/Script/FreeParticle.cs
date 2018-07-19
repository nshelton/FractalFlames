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

    public bool autoSwitch 
    {
        get { return m_autoSwitch;}
        set { m_autoSwitch = value;}
    }
    
    public Vector3 Parameters {get; set;}

    public Vector4 WeightsA {get; set;}
    public Vector4 WeightsB {get; set;}
    public Vector4 WeightsC {get; set;}
    public Vector4 WeightsD {get; set;}

    public Vector4 WeightModulationA {get; set;}
    public Vector4 WeightModulationB {get; set;}
    public Vector4 WeightModulationC {get; set;}
    public Vector4 WeightModulationD {get; set;}


    public float TransitionSpeed = 0.99f;

    private Vector4 NextWeightsA;
    private Vector4 NextWeightsB;
    private Vector4 NextWeightsC;
    private Vector4 NextWeightsD;

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

    public void Randomize()
    {
        NextWeightsA.x = Random.value * 2f - 1f;
        NextWeightsA.y = Random.value * 2f - 1f;
        NextWeightsA.z = Random.value * 2f - 1f;
        NextWeightsA.w = Random.value * 2f - 1f;

        NextWeightsB.x = Random.value * 2f - 1f;
        NextWeightsB.y = Random.value * 2f - 1f;
        NextWeightsB.z = Random.value * 2f - 1f;
        NextWeightsB.w = Random.value * 2f - 1f;

        NextWeightsC.x = Random.value * 2f - 1f;
        NextWeightsC.y = Random.value * 2f - 1f;
        NextWeightsC.z = Random.value * 2f - 1f;
        NextWeightsC.w = Random.value * 2f - 1f;

        NextWeightsD.x = Random.value * 2f - 1f;
        NextWeightsD.y = Random.value * 2f - 1f;
        NextWeightsD.z = Random.value * 2f - 1f;
        NextWeightsD.w = Random.value * 2f - 1f;

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
        computeShader.SetVector("Emitter", Emitter);

        Vector3 paramMap = Parameters - (Vector3.up * 0.2f);
        paramMap *= 15f;

        computeShader.SetVector("Parameters", paramMap );

        computeShader.Dispatch(mComputeShaderKernelID, mWarpCount, 1, 1);

        if (isTransition)
        {
            WeightsA = Vector4.Lerp(NextWeightsA, WeightsA, TransitionSpeed);
            WeightsB = Vector4.Lerp(NextWeightsB, WeightsB, TransitionSpeed);
            WeightsC = Vector4.Lerp(NextWeightsC, WeightsC, TransitionSpeed);
            WeightsD = Vector4.Lerp(NextWeightsD, WeightsD, TransitionSpeed);

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
