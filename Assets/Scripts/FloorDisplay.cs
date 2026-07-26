using Animancer;
using UnityEngine;

public class FloorDisplay : MonoBehaviour
{
    public AnimancerComponent ArrowAnimancer;
    public AnimationClip ArrowAnim;
    public MeshRenderer FloorMeshRenderer;
    public Material[] Materials;

    private void OnEnable()
    {
        GameManager.Instance.OnStartDescent += Blink;
        GameManager.Instance.OnNewFloor += HandleNewFloor;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStartDescent -= Blink;
        GameManager.Instance.OnNewFloor -= HandleNewFloor;
        }
    }

    private void HandleNewFloor()
    {
        ChangeToFloor(GameManager.Instance.CurrentFloor);
    }

    public void ChangeToFloor(int i)
    {
        if (i is < 0 or >= 10)
        {
            return;
        }
        FloorMeshRenderer.material = Materials[9 - i];
    }

    public void Blink()
    {
        ArrowAnimancer.Play(ArrowAnim);
    }
}