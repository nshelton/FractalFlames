using UnityEngine;
using System.Collections;

public class FreeParticle : MonoBehaviour
{
    /// <summary>
    /// Particle data structure used by the shader and the compute shader.
    /// </summary>
    private struct Particle
    {
        public Vector3 position;
        public Vector3 velocity;
    }

    /// <summary>
    /// Size in octet of the Particle struct.
    /// </summary>
    private const int SIZE_PARTICLE = 24;

    /// <summary>
    /// Number of Particle created in the system.
    /// </summary>
    public int particleCount = 1000;

    /// <summary>
    /// Material used to draw the Particle on screen.
    /// </summary>
    public Material material;

    /// <summary>
    /// Compute shader used to update the Particles.
    /// </summary>
    public ComputeShader computeShader;

    /// <summary>
    /// Id of the kernel used.
    /// </summary>
    private int mComputeShaderKernelID;

    /// <summary>
    /// Buffer holding the Particles.
    /// </summary>
    ComputeBuffer particleBuffer;

    /// <summary>
    /// Number of particle per warp.
    /// </summary>
    private const int WARP_SIZE = 256;

    /// <summary>
    /// Number of warp needed.
    /// </summary>
    private int mWarpCount;
	
	[SerializeField]
    private float MaxAge;

    private Vector2 m_touchPos;

    public float touchX
    {
        get { return m_touchPos.x; }
        set { m_touchPos.x = value; }
    }

    public float touchY
    {
        get { return m_touchPos.y; }
        set { m_touchPos.y = value; }
    }

	public float[] Weights;


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
            particleArray[i].position = Random.insideUnitSphere;
            particleArray[i].velocity = Vector3.one * float.MaxValue;
			particleArray[i].velocity.x = Random.value * MaxAge;
        }

        noiseTexture = new Texture2D(4096, Mathf.CeilToInt(particleCount / 4096));

        for (int x = 0; x < noiseTexture.width; x ++) 
		{
	        for (int y = 0; y < noiseTexture.height; y ++)
			{	
				noiseTexture.SetPixel(x,y,new Color(Random.value, Random.value, Random.value));
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

    void Update()
    {
        computeShader.SetFloat("Time", Time.time);
        computeShader.SetVector("TouchPos", m_touchPos);
        computeShader.SetFloat("NumParticles", particleCount);
        computeShader.SetFloat("MaxAge", MaxAge);

		computeShader.SetFloats("Weights", Weights);

        computeShader.Dispatch(mComputeShaderKernelID, mWarpCount, 1, 1);
    }

    void OnRenderObject()
    {
        material.SetMatrix("modelToWorld", transform.localToWorldMatrix);
        material.SetPass(0);

        Graphics.DrawProcedural(MeshTopology.Points, 1, particleCount);
    }

}
