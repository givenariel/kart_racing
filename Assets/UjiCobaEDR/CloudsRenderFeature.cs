using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CloudsRenderFeature : ScriptableRendererFeature
{
    class CloudsRenderPass : ScriptableRenderPass
    {
        private Material material;
        private RTHandle tempRT;
        private Texture valueNoiseImage;
        private Transform sun;
        private float minHeight, maxHeight, fadeDist, scale, steps;

        public CloudsRenderPass(Material material, Texture noiseTexture, Transform sunTransform,
                                float minHeight, float maxHeight, float fadeDist, float scale, float steps)
        {
            this.material = material;
            this.valueNoiseImage = noiseTexture;
            this.sun = sunTransform;
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
            this.fadeDist = fadeDist;
            this.scale = scale;
            this.steps = steps;
        }

        [System.Obsolete]
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;

            tempRT = RTHandles.Alloc(descriptor.width, descriptor.height,
                                     depthBufferBits: DepthBits.None,
                                     dimension: TextureDimension.Tex2D,
                                     name: "_TempCloudsTexture");

            if (tempRT == null)
            {
                Debug.LogError("tempRT gagal dialokasikan!");
            }
        }


        Matrix4x4 GetFrustumCorners(Camera cam)
        {
            Matrix4x4 frustumCorners = Matrix4x4.identity;
            Vector3[] fCorners = new Vector3[4];

            cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, fCorners);

            for (int i = 0; i < 4; i++)
            {
                fCorners[i] = cam.transform.TransformPoint(fCorners[i]); // Ubah ke world space
            }

            //frustumCorners.SetRow(0, fCorners[1]); // Kiri atas
            frustumCorners.SetRow(1, fCorners[2]); // Kanan atas
            frustumCorners.SetRow(2, fCorners[3]); // Kanan bawah
            frustumCorners.SetRow(3, fCorners[0]); // Kiri bawah

            return frustumCorners;
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (tempRT == null)
            {
                Debug.LogError("tempRT belum diinisialisasi di Execute!");
                return;
            }

            if (material == null || valueNoiseImage == null || tempRT == null)
            {
                Debug.LogError("Material, Texture, atau RTHandle tidak diinisialisasi dengan benar!");
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get("Render Clouds Effect");

            var camera = renderingData.cameraData.camera;
            material.SetTexture("_ValueNoise", valueNoiseImage);
            material.SetVector("_SunDir", sun ? -sun.forward : Vector3.up);
            material.SetFloat("_MinHeight", minHeight);
            material.SetFloat("_MaxHeight", maxHeight);
            material.SetFloat("_FadeDist", fadeDist);
            material.SetFloat("_Scale", scale);
            material.SetFloat("_Steps", steps);
            material.SetMatrix("_FrustumCornersWS", GetFrustumCorners(camera));
            material.SetMatrix("_CameraInvViewMatrix", camera.cameraToWorldMatrix);
            material.SetVector("_CameraPosWS", camera.transform.position);

            cmd.Blit(renderingData.cameraData.renderer.cameraColorTargetHandle, tempRT, material);
            cmd.Blit(tempRT, renderingData.cameraData.renderer.cameraColorTargetHandle);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (tempRT != null)
            {
                tempRT.Release();
                tempRT = null;
            }
        }

    }

    [SerializeField] private Shader cloudShader;
    [SerializeField] private Texture valueNoiseImage;
    [SerializeField] private Transform sun;
    [SerializeField] private float minHeight = 0.0f;
    [SerializeField] private float maxHeight = 5.0f;
    [SerializeField] private float fadeDist = 2.0f;
    [SerializeField] private float scale = 5.0f;
    [SerializeField] private float steps = 50;

    private Material material;
    private CloudsRenderPass renderPass;

    public override void Create()
    {
        if (cloudShader == null)
        {
            Debug.LogError("Cloud Shader belum diassign di Inspector!");
            return;
        }

        material = CoreUtils.CreateEngineMaterial(cloudShader);
        renderPass = new CloudsRenderPass(material, valueNoiseImage, sun, minHeight, maxHeight, fadeDist, scale, steps)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (material == null)
        {
            Debug.LogError("Material belum dibuat! Pastikan shader terpasang di Inspector.");
            return;
        }

        renderer.EnqueuePass(renderPass);
    }
}
