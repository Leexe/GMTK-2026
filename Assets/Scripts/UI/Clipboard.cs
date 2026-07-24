using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;

// fills in clipboard data

public class Clipboard : MonoBehaviour
{
    public RectTransform OwnTransform;
    public HoverTarget HoverTarget;

    public TMP_Text WorkersText;
    public TMP_Text PsychologistsText;
    public TMP_Text GuardsText;

    [Header("Options")]
    public Vector2 DefaultPosition;
    public Vector2 HiddenPosition;
    public Vector2 OutPosition;

    public float MoveSharpness = 4f;
    public float HideDuration;
    public float HideReturnDuration;

    private Sequence _sequence;

    private void OnEnable()
    {
        GameManager.Instance.OnNpcUpdate += UpdateInfo;
    }
    
    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNpcUpdate -= UpdateInfo;
        }

        _sequence.Stop();
    }

    private void Update()
    {
        // if not doing the number-update animation, exp lerp to the right position.
        if (!_sequence.isAlive)
        {
            Vector2 targetPos = HoverTarget.Hovered ? OutPosition : DefaultPosition;
            var nextPos = Vector2.Lerp(targetPos, OwnTransform.anchoredPosition, Mathf.Exp(-MoveSharpness * Time.deltaTime));
            OwnTransform.anchoredPosition = nextPos;
        }
    }

    private void UpdateInfo()
    {
        PlayNumberUpdateAnim();
    }

    private void PlayNumberUpdateAnim()
    {
        RectTransform self = GetComponent<RectTransform>();

        _sequence = Sequence.Create()
            .Chain(Tween.UIAnchoredPosition(OwnTransform, endValue: HiddenPosition, HideDuration))
            .ChainCallback(SetNumbers)
            .Chain(Tween.UIAnchoredPosition(OwnTransform, endValue: DefaultPosition, HideReturnDuration));
    }

    private void SetNumbers()
    {
		Dictionary<NpcRoles, int> counts = GameManager.Instance.NpcCount;
        
        WorkersText.text = counts[NpcRoles.Worker].ToString();
        PsychologistsText.text = counts[NpcRoles.Psychologist].ToString();
        GuardsText.text = counts[NpcRoles.Guard].ToString();
    }
}
