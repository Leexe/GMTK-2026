using UnityEngine;
using UnityEngine.UI;

public class EngineIntegrityUI : MonoBehaviour
{
	[SerializeField]
	private Image _engineIntegrityImage;

	private void OnEnable()
	{
		GameManager.Instance.OnEngineUpdate += UpdateUI;
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnEngineUpdate -= UpdateUI;
		}
	}

	private void Start()
	{
		UpdateUI();
	}

	private void UpdateUI()
	{
		_engineIntegrityImage.fillAmount = GameManager.Instance.EngineIntegrityNormalized;
	}
}
