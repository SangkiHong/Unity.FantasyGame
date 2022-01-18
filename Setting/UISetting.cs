using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UISetting : ScriptableObject
{
    // ������ ���� ��ġ
    private const string SettingFileDirectory = "Assets/Resources";
    // ������ �������� ��ġ
    private const string SettingFilePath = "Assets/Resources/UISetting.asset";

    private static UISetting _instance;

    // ���� ���� �̱���
    public static UISetting Instance
    {
        get 
        {
            if (_instance != null)
            {
                return _instance;
            }

            // Resources�������� ���� ��������
            _instance = Resources.Load<UISetting>("UISetting");

#if UNITY_EDITOR
            // �������� �ѹ��� ������ Ȯ��(Editor Time)
            if (_instance == null)
            {
                // �ش� ��ο� ���� ���� ���� Ȯ��
                if (!AssetDatabase.IsValidFolder(SettingFileDirectory))
                {
                    // ���� ���� ����
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                // ���� �������� ���� ��쿡 �ٽ� ��������
                _instance = AssetDatabase.LoadAssetAtPath<UISetting>(SettingFilePath);

                // ���� �����
                if (_instance == null)
                {
                    _instance = CreateInstance<UISetting>();
                    AssetDatabase.CreateAsset(_instance, SettingFilePath);
                }
            }
#endif
            return _instance;
        }
    }

    public string language = "kr";

    public Color themeColor;
    public Sprite emptyThumbnailSprite;
    public GameObject popupPrefab;

    public Font defaultFont;
    public int defaultFontsize = 180;
    public Color defaultFontColor = Color.gray;
}
