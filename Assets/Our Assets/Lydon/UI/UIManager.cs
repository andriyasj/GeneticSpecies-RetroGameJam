using UnityEngine;
using UnityEngine.UIElements;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    public UIDocument ui;
    public Texture2D healthTexture;

    private VisualElement healthMask;
    private Label ammoCounter;
    private Label waveCounter;
    private Label enemyCounter;
    private Image barImage;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    void OnEnable()
    {
        var root = ui.rootVisualElement;
        healthMask = root.Q<VisualElement>("HealthMask");
        ammoCounter = root.Q<Label>("AmmoCounter");
        waveCounter = root.Q<Label>("WaveCounter");
        enemyCounter = root.Q<Label>("EnemyCounter");

        healthMask.style.overflow = Overflow.Hidden;

        barImage = new Image
        {
            image = healthTexture,
            scaleMode = ScaleMode.StretchToFill
        };

        barImage.style.position = Position.Absolute;
        barImage.style.bottom = 0;
        barImage.style.left = 0;
        barImage.style.width = Length.Percent(100);
        barImage.style.height = Length.Percent(100);

        healthMask.Add(barImage);

        healthMask.parent.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            
        if (healthMask.parent.resolvedStyle.height > 0)
        {
            barImage.style.height = healthMask.parent.resolvedStyle.height;
        }
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        barImage.style.height = evt.newRect.height;
    }

    public void SetHealth(float normalized)
    {
        healthMask.style.height = Length.Percent(normalized * 100f);
    }
    public void SetAmmo(int ammo)
    {
        ammoCounter.text = ammo.ToString();
    }
    public void SetWaveCount(int wave)
    {
        waveCounter.text = wave.ToString();
    }
    public void SetEnemyCount(int count)
    {
        enemyCounter.text = count.ToString();
    }
}