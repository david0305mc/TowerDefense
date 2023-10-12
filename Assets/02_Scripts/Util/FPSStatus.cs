using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;

public class FPSStatus : MonoBehaviour
{
	/* obejct refs */
	[SerializeField]  private TextMeshProUGUI fpsText;

	/* Public Variables */
	[SerializeField] float frequency = 1f;
	private int framesPerSec;
	private void Start()
	{
        fpsText.text = "120 FPS";
        UniTask.Create(async () =>
        {
            while (true)
            {
                int lastFrameCount = Time.frameCount;
                float lastTime = Time.realtimeSinceStartup;
                await UniTask.WaitForSeconds(frequency, cancellationToken: this.GetCancellationTokenOnDestroy());
                float timeSpan = Time.realtimeSinceStartup - lastTime;
                int frameCount = Time.frameCount - lastFrameCount;
                framesPerSec = Mathf.RoundToInt(frameCount / timeSpan);
                fpsText.text = $"{framesPerSec} FPS";
            }
        });
	}
}
