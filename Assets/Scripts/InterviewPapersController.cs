using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

// handles showing/hiding of interview paper

public class InterviewPapersController : MonoBehaviour
{
    public InterviewPaper Paper;
    public Vector2 ShownPosition;
    public Vector2 HiddenPosition;

    [Header("Options")]
    public float AnimDuration = 1f;
    public float FastAnimDuration = 0.2f;

    private Sequence _activeSequence;
    private InterviewResults _currentShownInfo = null;
    private InterviewResults _infoToShow = null;


    /** Unity Messages **/

    public void OnDisable()
    {
        _activeSequence.Stop();
    }

    private readonly InterviewResults _a = new()
    {
        Name = "Victor Wembanyama",
        Role = NpcRoles.Psychologist,
        HeightInches = 90,
        FloorsTheyveBeen = new() { 10, 11},
        Questions = new()
        {
            new() { Question = "What is Obama's last name?", Answer = "Barack"},
            new() { Question = "What day is it?", Answer = "Tuesday probably"},
        }
    };
    
    private readonly InterviewResults _b = new()
    {
        Name = "Bob Wolfeschlegelsteinhausenbergerdorff",
        Role = NpcRoles.Worker,
        HeightInches = 67,
        FloorsTheyveBeen = new() { 5 },
        Questions = new()
        {
            new() { Question = "What is your last name?", Answer = "Wolfeschlegelsteinhausenbergerdorff"},
            new() { Question = "How are you doing?", Answer = "I'm doing alright-ish. I appreciate you asking!"},
        }
    };

    [Button]
    public void ShowA() => ShowInfo(_a);
    [Button]
    public void ShowB() => ShowInfo(_b);
    [Button]
    public void Clear() => HideInfo();

    /** Public Methods **/

    public void ShowInfo(InterviewResults info)
    {
        _infoToShow = info;
        if (!_activeSequence.isAlive)
        {
            Sync();
        }
    }

    public void HideInfo()
    {
        ShowInfo(null);
    }

    /** Private Helpers **/

    public void Sync()
    {
        if (_infoToShow != _currentShownInfo)
        {
            if (_currentShownInfo != null)
            {
                HideTween(fast: _infoToShow != null);
            }
            else
            {
                ShowTween(_infoToShow);
            }
        }
    }

    private void ShowTween(InterviewResults info)
    {
        // set all info - just text for now
        Paper.SetInfo(info);
        _currentShownInfo = info;

        float rot = Random.Range(-2f, 2f);

        _activeSequence = Sequence.Create()
            .Chain(Tween.UIAnchoredPosition(Paper.GetComponent<RectTransform>(), HiddenPosition, ShownPosition, AnimDuration, Ease.OutCubic))
            .Group(Tween.Rotation(Paper.transform, Quaternion.identity, Quaternion.Euler(0f, 0f, rot), AnimDuration))
            
            .ChainCallback(Sync);
    }

    private void HideTween(bool fast = false)
    {
        float duration = fast ? FastAnimDuration : AnimDuration;

        _currentShownInfo = null;
        _activeSequence = Sequence.Create()
            .Chain(Tween.UIAnchoredPosition(Paper.GetComponent<RectTransform>(), ShownPosition, HiddenPosition, duration, Ease.OutCubic))
            .ChainCallback(Sync);
    }
}
