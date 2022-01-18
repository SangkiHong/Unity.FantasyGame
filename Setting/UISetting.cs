using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UISetting : ScriptableObject
{
    // 존재할 파일 위치
    private const string SettingFileDirectory = "Assets/Resources";
    // 에셋의 세부적인 위치
    private const string SettingFilePath = "Assets/Resources/UISetting.asset";

    private static UISetting _instance;

    // 지연 생성 싱글톤
    public static UISetting Instance
    {
        get 
        {
            if (_instance != null)
            {
                return _instance;
            }

            // Resources폴더에서 에셋 가져오기
            _instance = Resources.Load<UISetting>("UISetting");

#if UNITY_EDITOR
            // 만든적이 한번도 없는지 확인(Editor Time)
            if (_instance == null)
            {
                // 해당 경로에 폴더 존재 유무 확인
                if (!AssetDatabase.IsValidFolder(SettingFileDirectory))
                {
                    // 에셋 폴더 생성
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                // 미쳐 가져오지 못한 경우에 다시 가져오기
                _instance = AssetDatabase.LoadAssetAtPath<UISetting>(SettingFilePath);

                // 에셋 만들기
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
