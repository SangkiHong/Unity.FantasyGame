using UnityEngine;
using UnityEngine.Events;
using Sangki;
using Sangki.Player;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public event UnityAction<int> OnInitialize;
    public UIManager UIManager;

    private void Awake()
    {
        if (GameManager.Instance != null) Destroy(this);
        else Instance = this;
    }

    private void Start()
    {
        if (OnInitialize != null) OnInitialize(PlayerController.Instance.maxHealth);
    }
}
