using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    void Start()
    {
        var frame = GameObject.Find("RedFrame")?.GetComponent<RawImage>();
        if (frame != null)
        {
            WitchManager.Instance.redFrame = frame;
        }
    }
}
