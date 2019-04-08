using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilingNoiseGenerator {
	public TilingNoiseGenerator () {
		noiseShader = Resources.Load<ComputeShader>("TilingNoiseComputeShader");
		Debug.Assert(noiseShader != null, "Missing noise compute shader.");

		seed = Random.value;
	}

	public Texture2D GenerateNoise (int width, int height, Vector2Int cellNum) {
		width = ((width + 7) / 8) * 8;
		height = ((height + 7) / 8) * 8;

		noiseShader.SetInt("width", width);
		noiseShader.SetInt("height", height);
		noiseShader.SetFloat("cellSizeX", ((float)width) / cellNum.x);
		noiseShader.SetFloat("cellSizeY", ((float)height) / cellNum.y);

		var renderTexture = GenerateNoiseRenderTexture(width, height);
		var result = ConvertToTexture2D(renderTexture, width, height);
		GameObject.Destroy(renderTexture);

		return result;
	}

	private RenderTexture GenerateNoiseRenderTexture (int width, int height) {
		var format = RenderTextureFormat.ARGB32;
		var res = new RenderTexture(width, height, 32, format);
		res.enableRandomWrite = true;
		res.Create();

		int kernelIndex = noiseShader.FindKernel("NoiseGenerator");
		noiseShader.SetTexture(kernelIndex, "Result", res);
		noiseShader.Dispatch(kernelIndex, width / 8, height / 8, 1);
		return res;
	}

	private Texture2D ConvertToTexture2D (RenderTexture rt, int w, int h) {
		Texture2D result = new Texture2D(w, h);

		var curActive = RenderTexture.active;
		RenderTexture.active = rt;

		result.ReadPixels(new Rect(0, 0, w, h), 0, 0);
		
		RenderTexture.active = curActive;
		result.Apply();

		return result;
	}

	public float seed {
		set {
			noiseShader.SetFloat("seed", value);
		}
	}

	private ComputeShader noiseShader;
}
